using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.Common;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Email
{
    public class ResendEmailConfirmationCommand : IRequest<EmailCommandResult>
    {
        public string Email { get; set; }
    }

    public class ResendEmailConfirmationCommandHandler : IRequestHandler<ResendEmailConfirmationCommand, EmailCommandResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly EmailSenderHelper _emailSender;
        private readonly IRateLimitService _rateLimitService;
        private readonly ILogger<ResendEmailConfirmationCommandHandler> _logger;

        public ResendEmailConfirmationCommandHandler(
            IUserRepository userRepository,
            EmailSenderHelper emailSender,
            IRateLimitService rateLimitService,
            ILogger<ResendEmailConfirmationCommandHandler> logger)
        {
            _userRepository = userRepository;
            _emailSender = emailSender;
            _rateLimitService = rateLimitService;
            _logger = logger;
        }

        public async Task<EmailCommandResult> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reenviando correo de confirmación para {Email}", request.Email);

            // Verificar si se ha excedido el límite de intentos
            var isRateLimited = await _rateLimitService.IsRateLimitedAsync(
                $"resend_confirmation:{request.Email}",
                maxAttempts: 3,
                timeWindow: TimeSpan.FromHours(24));
            
            if (isRateLimited)
            {
                _logger.LogWarning("Límite de intentos excedido para reenviar correo de confirmación a {Email}", request.Email);
                return EmailCommandResult.Failure("Has excedido el límite de intentos. Por favor, intenta más tarde.");
            }

            var user = await _userRepository.GetByEmailAsync(request.Email);
            
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para email {Email}", request.Email);
                return EmailCommandResult.Failure("Usuario no encontrado");
            }

            if (user.EmailConfirmed)
            {
                _logger.LogInformation("El email {Email} ya está confirmado", request.Email);
                return EmailCommandResult.Failure("El email ya está confirmado");
            }

            // Eliminar tokens anteriores
            await _userRepository.DeleteAllEmailConfirmationTokensForUserAsync(user.Id);
            
            // Generar nuevo token
            var token = Guid.NewGuid().ToString("N");
            
            // Almacenar token en la base de datos
            await _userRepository.StoreEmailConfirmationTokenAsync(user.Id, token);
            
            // Enviar correo de confirmación
            _emailSender.QueueConfirmationEmail(request.Email, user.Id, token);
            
            _logger.LogInformation("Correo de confirmación reenviado para {Email}", request.Email);
            
            return EmailCommandResult.Success("Se ha reenviado el correo de confirmación", user.Id);
        }
    }
}