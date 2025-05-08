using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.Common;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Email
{
    public class GenerateEmailConfirmationTokenCommandHandler : IRequestHandler<GenerateEmailConfirmationTokenCommand, EmailCommandResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly EmailSenderHelper _emailSender;
        private readonly ILogger<GenerateEmailConfirmationTokenCommandHandler> _logger;

        public GenerateEmailConfirmationTokenCommandHandler(
            IUserRepository userRepository,
            EmailSenderHelper emailSender,
            ILogger<GenerateEmailConfirmationTokenCommandHandler> logger)
        {
            _userRepository = userRepository;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<EmailCommandResult> Handle(GenerateEmailConfirmationTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generando token de confirmación para {Email}", request.Email);

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

            // Generar token aleatorio
            var token = Guid.NewGuid().ToString("N");
            
            // Almacenar token en la base de datos
            await _userRepository.StoreEmailConfirmationTokenAsync(user.Id, token);
            
            // Enviar correo de confirmación
            _emailSender.QueueConfirmationEmail(request.Email, user.Id, token);
            
            _logger.LogInformation("Token de confirmación generado y enviado para {Email}", request.Email);
            
            return EmailCommandResult.Success("Se ha enviado un correo de confirmación", user.Id);
        }
    }
}