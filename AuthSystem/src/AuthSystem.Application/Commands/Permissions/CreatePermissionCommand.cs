using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Permissions
{
    public class CreatePermissionCommand : IRequest<PermissionResponseDto>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, PermissionResponseDto>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<CreatePermissionCommandHandler> _logger;

        public CreatePermissionCommandHandler(
            IPermissionRepository permissionRepository,
            IAuditService auditService,
            ILogger<CreatePermissionCommandHandler> logger)
        {
            _permissionRepository = permissionRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PermissionResponseDto> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si ya existe un permiso con el mismo código
                var existingPermission = await _permissionRepository.GetByCodeAsync(request.Code);
                if (existingPermission != null)
                {
                    throw new InvalidOperationException($"Ya existe un permiso con el código '{request.Code}'");
                }

                // Crear nuevo permiso
                // Crear un nuevo permiso usando el constructor de la entidad
                var permission = new Permission(
                    name: request.Name,
                    code: request.Code,
                    description: request.Description,
                    category: request.Category
                );

                // Guardar en la base de datos
                var createdPermission = await _permissionRepository.CreateAsync(permission);

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Create",
                    entityName: "Permission",
                    entityId: createdPermission.Id.ToString(),
                    oldValues: null,
                    newValues: new { 
                        Name = createdPermission.Name, 
                        Code = createdPermission.Code,
                        Description = createdPermission.Description,
                        Category = createdPermission.Category
                    },
                    ipAddress: null,
                    userAgent: null);

                // Mapear a DTO de respuesta
                return new PermissionResponseDto
                {
                    Id = createdPermission.Id,
                    Name = createdPermission.Name,
                    Code = createdPermission.Code,
                    Description = createdPermission.Description,
                    Category = createdPermission.Category,
                    IsActive = true, // La entidad Permission no tiene la propiedad IsActive, asumimos que todos los permisos están activos
                    CreatedAt = createdPermission.CreatedAt,
                    Message = $"Permiso '{createdPermission.Name}' creado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el permiso {PermissionName}", request.Name);
                throw;
            }
        }
    }
}
