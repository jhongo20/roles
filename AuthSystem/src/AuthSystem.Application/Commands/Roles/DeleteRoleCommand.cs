using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Roles
{
    public class DeleteRoleCommand : IRequest<DeleteRoleResponseDto>
    {
        public Guid Id { get; set; }
    }

    public class DeleteRoleResponseDto
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
    }

    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, DeleteRoleResponseDto>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<DeleteRoleCommandHandler> _logger;

        public DeleteRoleCommandHandler(
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IAuditService auditService,
            ILogger<DeleteRoleCommandHandler> logger)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<DeleteRoleResponseDto> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Buscar el rol por ID
                var role = await _roleRepository.GetByIdAsync(request.Id);
                if (role == null)
                {
                    return new DeleteRoleResponseDto
                    {
                        Succeeded = false,
                        Error = $"No se encontró un rol con el ID '{request.Id}'"
                    };
                }

                // Verificar si el rol está asignado a usuarios
                var isAssignedToUsers = await _userRoleRepository.IsRoleAssignedToAnyUserAsync(request.Id);
                if (isAssignedToUsers)
                {
                    return new DeleteRoleResponseDto
                    {
                        Succeeded = false,
                        Error = $"No se puede eliminar el rol '{role.Name}' porque está asignado a uno o más usuarios"
                    };
                }

                // Desactivar el rol en lugar de eliminarlo físicamente
                role.Deactivate();
                await _roleRepository.UpdateAsync(role);
                await _roleRepository.SaveChangesAsync();

                // Registrar la acción en la auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Delete",
                    entityName: "Role",
                    entityId: role.Id.ToString(),
                    oldValues: new { IsActive = true },
                    newValues: new { IsActive = false },
                    ipAddress: null,
                    userAgent: null
                );

                return new DeleteRoleResponseDto
                {
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol: {Message}", ex.Message);
                return new DeleteRoleResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al eliminar el rol. Por favor, inténtelo de nuevo."
                };
            }
        }
    }
}
