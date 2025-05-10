using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Modules
{
    public class AddPermissionToModuleCommand : IRequest<ModulePermissionResponseDto>
    {
        public Guid ModuleId { get; set; }
        public Guid PermissionId { get; set; }
        public Guid? AssignedBy { get; set; }
    }

    public class AddPermissionToModuleCommandHandler : IRequestHandler<AddPermissionToModuleCommand, ModulePermissionResponseDto>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<AddPermissionToModuleCommandHandler> _logger;

        public AddPermissionToModuleCommandHandler(
            IModuleRepository moduleRepository,
            IPermissionRepository permissionRepository,
            IAuditService auditService,
            ILogger<AddPermissionToModuleCommandHandler> logger)
        {
            _moduleRepository = moduleRepository;
            _permissionRepository = permissionRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ModulePermissionResponseDto> Handle(AddPermissionToModuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el módulo
                var module = await _moduleRepository.GetByIdAsync(request.ModuleId);
                if (module == null)
                {
                    throw new InvalidOperationException($"No se encontró el módulo con ID '{request.ModuleId}'");
                }

                // Verificar si existe el permiso
                var permission = await _permissionRepository.GetByIdAsync(request.PermissionId);
                if (permission == null)
                {
                    throw new InvalidOperationException($"No se encontró el permiso con ID '{request.PermissionId}'");
                }

                // Verificar si el permiso ya está asociado al módulo
                var isAssociated = await _moduleRepository.HasPermissionAsync(request.ModuleId, request.PermissionId);
                if (isAssociated)
                {
                    throw new InvalidOperationException($"El permiso '{permission.Name}' ya está asociado al módulo '{module.Name}'");
                }

                // Crear la relación módulo-permiso
                // Nota: Parece que ModulePermission es una clase interna o una tabla de relación sin una entidad completa
                // Usamos directamente el método AddPermissionAsync con los IDs necesarios
                
                // Creamos un objeto simple para pasar al repositorio
                var modulePermission = new ModulePermission
                {
                    ModuleId = request.ModuleId,
                    PermissionId = request.PermissionId
                    // No incluimos AssignedBy y AssignedAt ya que no existen en la entidad
                };

                var success = await _moduleRepository.AddPermissionAsync(modulePermission);
                if (!success)
                {
                    throw new InvalidOperationException($"No se pudo asociar el permiso '{permission.Name}' al módulo '{module.Name}'");
                }

                // Registrar auditoría usando el método correcto
                await _auditService.LogActionAsync(
                    userId: request.AssignedBy, // Usamos el AssignedBy como userId para la auditoría
                    action: "AddPermission",
                    entityName: "ModulePermission",
                    entityId: $"{request.ModuleId}:{request.PermissionId}",
                    oldValues: null,
                    newValues: new { 
                        ModuleId = module.Id, 
                        ModuleName = module.Name,
                        PermissionId = permission.Id,
                        PermissionName = permission.Name
                    },
                    ipAddress: null,
                    userAgent: null);

                // Retornar respuesta
                return new ModulePermissionResponseDto
                {
                    ModuleId = request.ModuleId,
                    ModuleName = module.Name,
                    PermissionId = request.PermissionId,
                    PermissionName = permission.Name,
                    PermissionCode = permission.Code,
                    Success = true,
                    Message = $"Permiso '{permission.Name}' asociado exitosamente al módulo '{module.Name}'"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asociar el permiso {PermissionId} al módulo {ModuleId}", request.PermissionId, request.ModuleId);
                throw;
            }
        }
    }
}
