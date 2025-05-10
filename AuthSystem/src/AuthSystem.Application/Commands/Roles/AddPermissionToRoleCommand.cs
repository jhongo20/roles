using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Roles
{
    public class AddPermissionToRoleCommand : IRequest<PermissionRoleResponseDto>
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public Guid? AssignedBy { get; set; }
    }

    public class PermissionRoleResponseDto
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
    }

    public class AddPermissionToRoleCommandHandler : IRequestHandler<AddPermissionToRoleCommand, PermissionRoleResponseDto>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<AddPermissionToRoleCommandHandler> _logger;

        public AddPermissionToRoleCommandHandler(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IAuditService auditService,
            ILogger<AddPermissionToRoleCommandHandler> logger)
        {
            _roleRepository = roleRepository;
            _permissionRepository = permissionRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<PermissionRoleResponseDto> Handle(AddPermissionToRoleCommand request, CancellationToken cancellationToken)
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

                // Agregar el permiso al rol
                var result = await _roleRepository.AddPermissionAsync(
                    request.RoleId, 
                    request.PermissionId, 
                    request.AssignedBy
                );

                if (!result)
                {
                    return new PermissionRoleResponseDto
                    {
                        Succeeded = false,
                        Error = $"No se pudo agregar el permiso '{permission.Name}' al rol '{role.Name}'. Es posible que ya esté asignado."
                    };
                }

                // Registrar la acción en la auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "AddPermission",
                    entityName: "Role",
                    entityId: role.Id.ToString(),
                    oldValues: null,
                    newValues: new { PermissionId = permission.Id, PermissionName = permission.Name },
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
                _logger.LogError(ex, "Error al agregar permiso al rol: {Message}", ex.Message);
                return new PermissionRoleResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al agregar el permiso al rol. Por favor, inténtelo de nuevo."
                };
            }
        }
    }
}
