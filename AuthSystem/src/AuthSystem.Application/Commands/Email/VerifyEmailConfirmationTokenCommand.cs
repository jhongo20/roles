using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.Common;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Email
{
    public class VerifyEmailConfirmationTokenCommand : IRequest<EmailCommandResult>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }

    public class VerifyEmailConfirmationTokenCommandHandler : IRequestHandler<VerifyEmailConfirmationTokenCommand, EmailCommandResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<VerifyEmailConfirmationTokenCommandHandler> _logger;

        public VerifyEmailConfirmationTokenCommandHandler(
            IUserRepository userRepository,
            ILogger<VerifyEmailConfirmationTokenCommandHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<EmailCommandResult> Handle(VerifyEmailConfirmationTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Verificando token de confirmación para usuario {UserId}", request.UserId);

            var (isValid, user) = await _userRepository.ValidateEmailConfirmationTokenAsync(request.UserId, request.Token);
            
            if (!isValid || user == null)
            {
                _logger.LogWarning("Token de confirmación inválido para usuario {UserId}", request.UserId);
                return EmailCommandResult.Failure("Token de confirmación inválido o expirado");
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("El email del usuario {UserId} ya está confirmado", request.UserId);
                return EmailCommandResult.Success("El email ya está confirmado", user.Id, true);
            }

            // Confirmar el email del usuario
            var confirmed = await _userRepository.ConfirmEmailAsync(user.Id);
            
            if (!confirmed)
            {
                _logger.LogError("Error al confirmar el email del usuario {UserId}", request.UserId);
                return EmailCommandResult.Failure("Error al confirmar el email");
            }
            
            _logger.LogInformation("Email confirmado exitosamente para usuario {UserId}", request.UserId);
            
            // Eliminar todos los tokens de confirmación para este usuario
            await _userRepository.DeleteAllEmailConfirmationTokensForUserAsync(user.Id);
            
            return EmailCommandResult.Success("Cuenta confirmada exitosamente", user.Id, true);
        }
    }
}