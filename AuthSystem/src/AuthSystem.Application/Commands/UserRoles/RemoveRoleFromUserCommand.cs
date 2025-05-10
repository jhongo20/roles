using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserRoles
{
    public class RemoveRoleFromUserCommand : IRequest<UserRoleResponseDto>
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }

    public class RemoveRoleFromUserCommandHandler : IRequestHandler<RemoveRoleFromUserCommand, UserRoleResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<RemoveRoleFromUserCommandHandler> _logger;

        public RemoveRoleFromUserCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IAuditService auditService,
            ILogger<RemoveRoleFromUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<UserRoleResponseDto> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
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

                // Verificar si el usuario tiene el rol asignado
                var isInRole = await _userRepository.IsInRoleAsync(request.UserId, request.RoleId);
                if (!isInRole)
                {
                    throw new InvalidOperationException($"El usuario '{user.Username}' no tiene asignado el rol '{role.Name}'");
                }

                // Quitar el rol del usuario
                var success = await _userRoleRepository.RemoveRoleFromUserAsync(request.UserId, request.RoleId);
                if (!success)
                {
                    throw new InvalidOperationException($"No se pudo quitar el rol '{role.Name}' del usuario");
                }

                // Registrar auditoría usando el método correcto
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción
                    action: "RemoveRole",
                    entityName: "UserRole",
                    entityId: $"{request.UserId}:{request.RoleId}",
                    oldValues: new { 
                        UserId = user.Id, 
                        UserName = user.Username,
                        RoleId = role.Id,
                        RoleName = role.Name
                    },
                    newValues: null,
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
                    Message = $"Rol '{role.Name}' quitado exitosamente del usuario '{user.Username}'"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar el rol {RoleId} del usuario {UserId}", request.RoleId, request.UserId);
                throw;
            }
        }
    }
}
