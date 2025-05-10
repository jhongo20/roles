using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Permissions
{
    public class UpdatePermissionCommand : IRequest<PermissionResponseDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, PermissionResponseDto>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<UpdatePermissionCommandHandler> _logger;

        public UpdatePermissionCommandHandler(
            IPermissionRepository permissionRepository,
            IAuditService auditService,
            ILogger<UpdatePermissionCommandHandler> logger)
        {
            _permissionRepository = permissionRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PermissionResponseDto> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el permiso
                var permission = await _permissionRepository.GetByIdAsync(request.Id);
                if (permission == null)
                {
                    throw new InvalidOperationException($"No se encontró el permiso con ID '{request.Id}'");
                }

                // Guardar valores originales para auditoría
                var originalName = permission.Name;
                var originalDescription = permission.Description;
                var originalCategory = permission.Category;
                var originalCode = permission.Code;

                // Actualizar propiedades usando el método Update de la entidad
                permission.Update(
                    name: request.Name,
                    code: permission.Code, // Mantenemos el código original ya que no se puede cambiar
                    description: request.Description,
                    category: request.Category
                );

                // Guardar cambios
                await _permissionRepository.UpdateAsync(permission);

                // Registrar auditoría usando el método correcto
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Update",
                    entityName: "Permission",
                    entityId: permission.Id.ToString(),
                    oldValues: new { 
                        Name = originalName, 
                        Description = originalDescription,
                        Category = originalCategory,
                        Code = originalCode
                    },
                    newValues: new { 
                        Name = permission.Name, 
                        Description = permission.Description,
                        Category = permission.Category,
                        Code = permission.Code
                    },
                    ipAddress: null,
                    userAgent: null);

                // Mapear a DTO de respuesta
                return new PermissionResponseDto
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Code = permission.Code,
                    Description = permission.Description,
                    Category = permission.Category,
                    IsActive = true, // La entidad Permission no tiene la propiedad IsActive, asumimos que siempre está activo
                    CreatedAt = permission.CreatedAt,
                    UpdatedAt = permission.UpdatedAt,
                    Message = $"Permiso '{permission.Name}' actualizado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el permiso con ID {PermissionId}", request.Id);
                throw;
            }
        }
    }
}
