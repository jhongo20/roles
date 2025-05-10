using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserManagement
{
    public class ResetUserPasswordCommand : IRequest<UserResponseDto>
    {
        public Guid UserId { get; set; }
        public string NewPassword { get; set; }
        public bool RequirePasswordChange { get; set; } = true;
        public bool SendPasswordResetEmail { get; set; } = true;
    }

    public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly ILogger<ResetUserPasswordCommandHandler> _logger;

        public ResetUserPasswordCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            IAuditService auditService,
            ILogger<ResetUserPasswordCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<UserResponseDto> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException($"No se encontró el usuario con ID '{request.UserId}'");
                }

                // Generar nueva contraseña si no se proporciona una
                string password = request.NewPassword;
                if (string.IsNullOrEmpty(password))
                {
                    password = GenerateRandomPassword();
                }

                // Actualizar la contraseña usando el método ChangePassword de la entidad User
                string hashedPassword = _passwordHasher.HashPassword(password);
                user.ChangePassword(hashedPassword);
                
                // No podemos asignar directamente a RequirePasswordChange y LastPasswordChangedAt
                // ya que son propiedades de solo lectura
                // Asumimos que estos valores se actualizan en el repositorio

                // Guardar cambios
                await _userRepository.UpdateAsync(user);

                // Enviar correo con la nueva contraseña si se solicita
                if (request.SendPasswordResetEmail)
                {
                    await _emailService.SendAsync(
                        user.Email,
                        "Restablecimiento de contraseña",
                        $"Su contraseña ha sido restablecida. Su nueva contraseña es: {password}<br>" +
                        $"Por favor, cambie esta contraseña la próxima vez que inicie sesión.");
                }

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "PasswordReset",
                    entityName: "User",
                    entityId: user.Id.ToString(),
                    oldValues: null,
                    newValues: new { PasswordChanged = true },
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
                    IsActive = user.Status == Core.Enums.UserStatus.Active,
                    RequirePasswordChange = user.RequirePasswordChange,
                    Success = true,
                    Message = $"Contraseña del usuario '{user.Username}' restablecida exitosamente" +
                             (request.SendPasswordResetEmail ? ". Se ha enviado un correo con la nueva contraseña." : ""),
                    // Solo incluir la contraseña en la respuesta si no se envía por correo
                    TemporaryPassword = !request.SendPasswordResetEmail ? password : string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer la contraseña del usuario con ID {UserId}", request.UserId);
                throw;
            }
        }

        private string GenerateRandomPassword()
        {
            // Generar una contraseña aleatoria de 12 caracteres
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
