using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Enums;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserManagement
{

    public class ChangeUserStatusCommand : IRequest<UserResponseDto>
    {
        public Guid UserId { get; set; }
        public UserStatus Status { get; set; }
        public string Reason { get; set; }
        public DateTime? LockoutEnd { get; set; }
    }

    public class ChangeUserStatusCommandHandler : IRequestHandler<ChangeUserStatusCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<ChangeUserStatusCommandHandler> _logger;

        public ChangeUserStatusCommandHandler(
            IUserRepository userRepository,
            IAuditService auditService,
            ILogger<ChangeUserStatusCommandHandler> logger)
        {
            _userRepository = userRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<UserResponseDto> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException($"No se encontró el usuario con ID '{request.UserId}'");
                }

                string action = "";
                
                // Verificar si el usuario ya está en el estado solicitado
                // Convertir el UserStatus de la aplicación al UserStatus del Core
                var coreUserStatus = MapToUserStatus(request.Status);
                if (user.Status == coreUserStatus)
                {
                    // Retornar respuesta sin realizar cambios
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
                        LockoutEnd = user.LockoutEnd?.DateTime,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt,
                        Success = true,
                        Message = $"Usuario '{user.Username}' ya se encuentra en el estado '{request.Status}'"
                    };
                }

                // Aplicar el cambio de estado según lo solicitado
                switch (request.Status)
                {
                    case Core.Enums.UserStatus.Active:
                        // Usar el método UnlockAccount que establece Status a Active
                        user.UnlockAccount();
                        action = "activado";
                        break;
                    
                    case Core.Enums.UserStatus.Registered:
                        // Para usuarios registrados pero no activos
                        // No hay un método específico, pero podríamos usar Suspend
                        user.Suspend();
                        action = "desactivado";
                        break;
                    
                    case Core.Enums.UserStatus.Suspended:
                        // Usar el método Suspend que establece Status a Suspended
                        user.Suspend();
                        action = "suspendido";
                        break;
                    
                    case Core.Enums.UserStatus.Blocked:
                        // Usar el método LockAccount que maneja internamente LockoutEnd y Status
                        TimeSpan lockDuration = request.LockoutEnd.HasValue 
                            ? request.LockoutEnd.Value - DateTime.UtcNow 
                            : TimeSpan.FromDays(36500); // Bloqueo efectivamente permanente (100 años)
                        user.LockAccount(lockDuration);
                        action = "bloqueado";
                        break;
                }

                // UpdatedAt se actualiza automáticamente en el repositorio

                // Guardar cambios
                await _userRepository.UpdateAsync(user);

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    null, // userId del que realiza la acción (null si es sistema)
                    "StatusChange",
                    "User",
                    user.Id.ToString(),
                    null, // oldValues
                    new { Status = request.Status, Reason = request.Reason },
                    null, // ipAddress
                    null  // userAgent
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
                    LockoutEnd = user.LockoutEnd?.DateTime,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Success = true,
                    Message = $"Usuario '{user.Username}' ha sido {action} exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el estado del usuario con ID {UserId}", request.UserId);
                throw;
            }
        }
        
        // Método auxiliar para mapear entre los dos tipos de UserStatus
        private Core.Enums.UserStatus MapToUserStatus(Core.Enums.UserStatus status)
        {
            // Como estamos usando el mismo tipo, simplemente devolvemos el valor
            // Pero asegurándonos de que los valores coincidan con los del enum del Core
            return status switch
            {
                UserStatus.Active => UserStatus.Active,
                UserStatus.Registered => UserStatus.Registered,
                UserStatus.Suspended => UserStatus.Suspended,
                UserStatus.Blocked => UserStatus.Blocked,
                UserStatus.Deleted => UserStatus.Deleted,
                _ => UserStatus.Registered // Valor por defecto
            };
        }
    }
}
