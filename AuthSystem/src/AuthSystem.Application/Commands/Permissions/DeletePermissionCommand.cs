using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Permissions
{
    public class DeletePermissionCommand : IRequest<DeletePermissionResponseDto>
    {
        public Guid Id { get; set; }
    }

    public class DeletePermissionResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, DeletePermissionResponseDto>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<DeletePermissionCommandHandler> _logger;

        public DeletePermissionCommandHandler(
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IAuditService auditService,
            ILogger<DeletePermissionCommandHandler> logger)
        {
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<DeletePermissionResponseDto> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el permiso
                var permission = await _permissionRepository.GetByIdAsync(request.Id);
                if (permission == null)
                {
                    return new DeletePermissionResponseDto
                    {
                        Id = request.Id,
                        Success = false,
                        Message = $"No se encontró el permiso con ID '{request.Id}'"
                    };
                }

                // Verificar si el permiso está asignado a algún rol
                var isAssignedToRole = await _roleRepository.IsPermissionAssignedToAnyRoleAsync(request.Id);
                if (isAssignedToRole)
                {
                    return new DeletePermissionResponseDto
                    {
                        Id = permission.Id,
                        Code = permission.Code,
                        Name = permission.Name,
                        Success = false,
                        Message = $"No se puede eliminar el permiso '{permission.Name}' porque está asignado a uno o más roles"
                    };
                }

                // Eliminar permiso (eliminación física ya que Permission no tiene propiedad IsActive)
                await _permissionRepository.DeleteAsync(permission.Id);

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Delete",
                    entityName: "Permission",
                    entityId: permission.Id.ToString(),
                    oldValues: new { 
                        Name = permission.Name, 
                        Code = permission.Code,
                        Description = permission.Description,
                        Category = permission.Category
                    },
                    newValues: null,
                    ipAddress: null,
                    userAgent: null);

                return new DeletePermissionResponseDto
                {
                    Id = permission.Id,
                    Code = permission.Code,
                    Name = permission.Name,
                    Success = true,
                    Message = $"Permiso '{permission.Name}' eliminado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el permiso con ID {PermissionId}", request.Id);
                
                return new DeletePermissionResponseDto
                {
                    Id = request.Id,
                    Success = false,
                    Message = $"Error al eliminar el permiso: {ex.Message}"
                };
            }
        }
    }
}
