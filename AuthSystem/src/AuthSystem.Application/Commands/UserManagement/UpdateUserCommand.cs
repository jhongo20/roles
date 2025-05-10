using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserManagement
{
    public class UpdateUserCommand : IRequest<UserResponseDto>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<UpdateUserCommandHandler> _logger;

        public UpdateUserCommandHandler(
            IUserRepository userRepository,
            IAuditService auditService,
            ILogger<UpdateUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<UserResponseDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el usuario
                var user = await _userRepository.GetByIdAsync(request.Id);
                if (user == null)
                {
                    throw new InvalidOperationException($"No se encontró el usuario con ID '{request.Id}'");
                }

                // Guardar valores originales para auditoría
                var originalFirstName = user.FirstName;
                var originalLastName = user.LastName;
                var originalPhoneNumber = user.PhoneNumber;
                // Obtener el estado actual del usuario
                var originalStatus = user.Status;

                // Actualizar propiedades del perfil usando el método de la entidad
                user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
                
                // Actualizar el estado del usuario si es necesario
                if (request.IsActive && user.Status != Core.Enums.UserStatus.Active)
                {
                    // Si el usuario está bloqueado, lo desbloqueamos
                    if (user.Status == Core.Enums.UserStatus.Blocked)
                    {
                        user.UnlockAccount();
                    }
                    // Si está suspendido, necesitamos activarlo de otra manera
                    else if (user.Status == Core.Enums.UserStatus.Suspended || user.Status == Core.Enums.UserStatus.Registered)
                    {
                        // Como no hay un método Activate() directo, usamos ConfirmEmail que cambia el estado a Active
                        if (!user.EmailConfirmed)
                        {
                            user.ConfirmEmail();
                        }
                        else
                        {
                            // Si el email ya está confirmado, usamos UnlockAccount que establece el estado a Active
                            user.UnlockAccount();
                        }
                    }
                }
                else if (!request.IsActive && user.Status == Core.Enums.UserStatus.Active)
                {
                    user.Suspend();
                }

                // Guardar cambios
                await _userRepository.UpdateAsync(user);

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Update",
                    entityName: "User",
                    entityId: user.Id.ToString(),
                    oldValues: new { 
                        FirstName = originalFirstName, 
                        LastName = originalLastName, 
                        PhoneNumber = originalPhoneNumber,
                        Status = originalStatus
                    },
                    newValues: new { 
                        FirstName = user.FirstName, 
                        LastName = user.LastName, 
                        PhoneNumber = user.PhoneNumber,
                        Status = user.Status
                    },
                    ipAddress: null,
                    userAgent: null
                );

                // Retornar respuesta
                return new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.Status == Core.Enums.UserStatus.Active,
                    EmailConfirmed = user.EmailConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Success = true,
                    Message = $"Usuario '{user.Username}' actualizado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID {UserId}", request.Id);
                throw;
            }
        }
    }
}
