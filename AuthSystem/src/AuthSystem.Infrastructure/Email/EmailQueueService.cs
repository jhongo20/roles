using System;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Email
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailQueueService> _logger;

        public EmailQueueService(IEmailService emailService, ILogger<EmailQueueService> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async void QueueConfirmationEmail(string email, Guid userId, string token)
        {
            try
            {
                _logger.LogInformation("Enviando email de confirmación para {Email}", email);
                
                // Enviar email directamente usando IEmailService
                await _emailService.SendConfirmationEmailAsync(email, userId.ToString(), token);
                
                _logger.LogInformation("Email de confirmación enviado a {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de confirmación para {Email}", email);
                throw;
            }
        }

        public async void QueuePasswordResetEmail(string email, Guid userId, string token)
        {
            try
            {
                _logger.LogInformation("Enviando email de restablecimiento de contraseña para {Email}", email);
                
                // Enviar email directamente usando IEmailService
                await _emailService.SendPasswordResetEmailAsync(email, userId.ToString(), token);
                
                _logger.LogInformation("Email de restablecimiento de contraseña enviado a {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de restablecimiento de contraseña para {Email}", email);
                throw;
            }
        }

        public async void QueueTwoFactorCodeEmail(string email, string code)
        {
            try
            {
                _logger.LogInformation("Enviando email con código de verificación para {Email}", email);
                
                // Enviar email directamente usando IEmailService
                await _emailService.SendTwoFactorCodeAsync(email, code);
                
                _logger.LogInformation("Email con código de verificación enviado a {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email con código de verificación para {Email}", email);
                throw;
            }
        }

        public async void QueueGenericEmail(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                _logger.LogInformation("Enviando email genérico para {To} con asunto '{Subject}'", to, subject);
                
                // Enviar email directamente usando IEmailService
                await _emailService.SendAsync(to, subject, body, isHtml);
                
                _logger.LogInformation("Email genérico enviado a {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email genérico para {To}", to);
                throw;
            }
        }
    }
}
