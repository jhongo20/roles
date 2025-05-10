using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserRoles
{
    public class AssignRoleToUserCommand : IRequest<UserRoleResponseDto>
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? AssignedBy { get; set; }
    }

    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, UserRoleResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<AssignRoleToUserCommandHandler> _logger;

        public AssignRoleToUserCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IAuditService auditService,
            ILogger<AssignRoleToUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<UserRoleResponseDto> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException($"No se encontró el usuario con ID '{request.UserId}'");
                }

                // Verificar si existe el rol
                var role = await _roleRepository.GetByIdAsync(request.RoleId);
                if (role == null)
                {
                    throw new InvalidOperationException($"No se encontró el rol con ID '{request.RoleId}'");
                }

                // Verificar si el rol está activo
                if (!role.IsActive)
                {
                    throw new InvalidOperationException($"El rol '{role.Name}' está inactivo y no puede ser asignado");
                }

                // Asignar el rol al usuario
                var success = await _userRoleRepository.AssignRoleToUserAsync(request.UserId, request.RoleId, request.AssignedBy);
                if (!success)
                {
                    throw new InvalidOperationException($"No se pudo asignar el rol '{role.Name}' al usuario");
                }

                // Registrar auditoría usando el método correcto
                await _auditService.LogActionAsync(
                    userId: request.AssignedBy, // ID del usuario que realiza la acción
                    action: "AssignRole",
                    entityName: "UserRole",
                    entityId: $"{request.UserId}:{request.RoleId}",
                    oldValues: null,
                    newValues: new { 
                        UserId = user.Id, 
                        UserName = user.Username,
                        RoleId = role.Id,
                        RoleName = role.Name
                    },
                    ipAddress: null,
                    userAgent: null);

                // Retornar respuesta
                return new UserRoleResponseDto
                {
                    UserId = request.UserId,
                    RoleId = request.RoleId,
                    RoleName = role.Name,
                    Username = user.Username,
                    Success = true,
                    Message = $"Rol '{role.Name}' asignado exitosamente al usuario '{user.Username}'"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar el rol {RoleId} al usuario {UserId}", request.RoleId, request.UserId);
                throw;
            }
        }
    }
}
