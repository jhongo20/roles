using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserManagement
{
    public class ResendActivationEmailCommand : IRequest<UserResponseDto>
    {
        public Guid UserId { get; set; }
    }

    public class ResendActivationEmailCommandHandler : IRequestHandler<ResendActivationEmailCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly ILogger<ResendActivationEmailCommandHandler> _logger;

        public ResendActivationEmailCommandHandler(
            IUserRepository userRepository,
            IEmailService emailService,
            IAuditService auditService,
            ILogger<ResendActivationEmailCommandHandler> logger)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<UserResponseDto> Handle(ResendActivationEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException($"No se encontró el usuario con ID '{request.UserId}'");
                }

                // Verificar si el email ya está confirmado
                if (user.EmailConfirmed)
                {
                    throw new InvalidOperationException($"El email del usuario '{user.Username}' ya está confirmado");
                }

                // Eliminar tokens de confirmación anteriores
                await _userRepository.DeleteAllEmailConfirmationTokensForUserAsync(user.Id);

                // Generar nuevo token de confirmación
                var token = Guid.NewGuid().ToString();
                await _userRepository.StoreEmailConfirmationTokenAsync(user.Id, token);

                // Enviar email de activación
                await _emailService.SendAsync(
                    user.Email,
                    "Activación de cuenta",
                    $"Por favor, active su cuenta haciendo clic en el siguiente enlace: " +
                    $"<a href='https://yourdomain.com/activate?userId={user.Id}&token={token}'>Activar cuenta</a>");

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "ResendActivation",
                    entityName: "User",
                    entityId: user.Id.ToString(),
                    oldValues: null,
                    newValues: new { Email = user.Email },
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
                    EmailConfirmed = user.EmailConfirmed,
                    Success = true,
                    Message = $"Correo de activación reenviado exitosamente al usuario '{user.Username}'"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar el correo de activación al usuario con ID {UserId}", request.UserId);
                throw;
            }
        }
    }
}
