using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Roles
{
    public class RemovePermissionFromRoleCommand : IRequest<PermissionRoleResponseDto>
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
    }

    public class RemovePermissionFromRoleCommandHandler : IRequestHandler<RemovePermissionFromRoleCommand, PermissionRoleResponseDto>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<RemovePermissionFromRoleCommandHandler> _logger;

        public RemovePermissionFromRoleCommandHandler(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IAuditService auditService,
            ILogger<RemovePermissionFromRoleCommandHandler> logger)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PermissionRoleResponseDto> Handle(RemovePermissionFromRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si el rol existe
                var role = await _roleRepository.GetByIdAsync(request.RoleId);
                if (role == null)
                {
                    return new PermissionRoleResponseDto
                    {
                        Succeeded = false,
                        Error = $"No se encontró un rol con el ID '{request.RoleId}'"
                    };
                }

                // Verificar si el permiso existe
                var permission = await _permissionRepository.GetByIdAsync(request.PermissionId);
                if (permission == null)
                {
                    return new PermissionRoleResponseDto
                    {
                        Succeeded = false,
                        Error = $"No se encontró un permiso con el ID '{request.PermissionId}'"
                    };
                }

                // Eliminar el permiso del rol
                var result = await _roleRepository.RemovePermissionAsync(
                    request.RoleId,
                    request.PermissionId
                );

                if (!result)
                {
                    return new PermissionRoleResponseDto
                    {
                        Succeeded = false,
                        Error = $"No se pudo eliminar el permiso '{permission.Name}' del rol '{role.Name}'. Es posible que no esté asignado."
                    };
                }

                // Registrar la acción en la auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "RemovePermission",
                    entityName: "Role",
                    entityId: role.Id.ToString(),
                    oldValues: new { PermissionId = permission.Id, PermissionName = permission.Name },
                    newValues: null,
                    ipAddress: null,
                    userAgent: null
                );

                return new PermissionRoleResponseDto
                {
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso del rol: {Message}", ex.Message);
                return new PermissionRoleResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al eliminar el permiso del rol. Por favor, inténtelo de nuevo."
                };
            }
        }
    }
}
