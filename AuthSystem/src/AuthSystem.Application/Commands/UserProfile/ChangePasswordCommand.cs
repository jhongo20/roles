using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserProfile
{
    public class ChangePasswordCommand : IRequest<ChangePasswordResponse>
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordResponse
    {
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
    }

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<ChangePasswordCommandHandler> _logger;

        public ChangePasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ILogger<ChangePasswordCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validar que las contraseñas coincidan
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new ChangePasswordResponse
                    {
                        Succeeded = false,
                        Error = "La nueva contraseña y la confirmación no coinciden"
                    };
                }

                // Validar que la nueva contraseña no esté vacía
                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return new ChangePasswordResponse
                    {
                        Succeeded = false,
                        Error = "La nueva contraseña no puede estar vacía"
                    };
                }

                // Obtener el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UserId}", request.UserId);
                    return new ChangePasswordResponse
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado"
                    };
                }

                // Verificar la contraseña actual
                if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
                {
                    _logger.LogWarning("Contraseña actual incorrecta para el usuario: {UserId}", request.UserId);
                    return new ChangePasswordResponse
                    {
                        Succeeded = false,
                        Error = "La contraseña actual es incorrecta"
                    };
                }

                // Verificar que la nueva contraseña no sea igual a la actual
                if (request.CurrentPassword == request.NewPassword)
                {
                    return new ChangePasswordResponse
                    {
                        Succeeded = false,
                        Error = "La nueva contraseña no puede ser igual a la actual"
                    };
                }

                // Generar el hash de la nueva contraseña
                string newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

                // Actualizar la contraseña usando el método ChangePassword
                user.ChangePassword(newPasswordHash);
                await _userRepository.UpdateAsync(user);

                // El flag de cambio de contraseña ya se actualiza en el método ChangePassword
                // No es necesario hacer nada más aquí

                // Registrar el cambio de contraseña en el historial
                var passwordHistory = new AuthSystem.Core.Entities.PasswordHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    PasswordHash = newPasswordHash,
                    ChangedAt = DateTime.UtcNow,
                    IPAddress = "127.0.0.1" // En una implementación real, esto vendría del request
                };
                await _userRepository.AddPasswordToHistoryAsync(passwordHistory);

                return new ChangePasswordResponse
                {
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña del usuario {UserId}", request.UserId);
                return new ChangePasswordResponse
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al cambiar la contraseña"
                };
            }
        }
    }
}
