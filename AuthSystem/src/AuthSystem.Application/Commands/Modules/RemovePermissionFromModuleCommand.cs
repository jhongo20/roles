using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Modules
{
    public class RemovePermissionFromModuleCommand : IRequest<ModulePermissionResponseDto>
    {
        public Guid ModuleId { get; set; }
        public Guid PermissionId { get; set; }
    }

    public class RemovePermissionFromModuleCommandHandler : IRequestHandler<RemovePermissionFromModuleCommand, ModulePermissionResponseDto>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<RemovePermissionFromModuleCommandHandler> _logger;

        public RemovePermissionFromModuleCommandHandler(
            IModuleRepository moduleRepository,
            IPermissionRepository permissionRepository,
            IAuditService auditService,
            ILogger<RemovePermissionFromModuleCommandHandler> logger)
        {
            _moduleRepository = moduleRepository;
            _permissionRepository = permissionRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ModulePermissionResponseDto> Handle(RemovePermissionFromModuleCommand request, CancellationToken cancellationToken)
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

                // Verificar si el permiso está asociado al módulo
                var isAssociated = await _moduleRepository.HasPermissionAsync(request.ModuleId, request.PermissionId);
                if (!isAssociated)
                {
                    throw new InvalidOperationException($"El permiso '{permission.Name}' no está asociado al módulo '{module.Name}'");
                }

                // Quitar el permiso del módulo
                var success = await _moduleRepository.RemovePermissionAsync(request.ModuleId, request.PermissionId);
                if (!success)
                {
                    throw new InvalidOperationException($"No se pudo quitar el permiso '{permission.Name}' del módulo '{module.Name}'");
                }

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Remove",
                    entityName: "ModulePermission",
                    entityId: $"{request.ModuleId}:{request.PermissionId}",
                    oldValues: new { ModuleId = module.Id, PermissionId = permission.Id },
                    newValues: null,
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
                    Message = $"Permiso '{permission.Name}' quitado exitosamente del módulo '{module.Name}'"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar el permiso {PermissionId} del módulo {ModuleId}", request.PermissionId, request.ModuleId);
                throw;
            }
        }
    }
}
