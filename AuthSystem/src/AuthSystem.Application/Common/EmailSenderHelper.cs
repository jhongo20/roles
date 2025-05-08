using System;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Common
{
    public class EmailSenderHelper
    {
        private readonly IEmailQueueService _emailQueueService;
        private readonly ILogger<EmailSenderHelper> _logger;

        public EmailSenderHelper(IEmailQueueService emailQueueService, ILogger<EmailSenderHelper> logger)
        {
            _emailQueueService = emailQueueService;
            _logger = logger;
        }

        public void QueueConfirmationEmail(string email, Guid userId, string token)
        {
            try
            {
                _logger.LogInformation("Encolando email de confirmación para {Email}", email);
                _emailQueueService.QueueConfirmationEmail(email, userId, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar email de confirmación para {Email}", email);
                throw;
            }
        }

        public void QueuePasswordResetEmail(string email, Guid userId, string token)
        {
            try
            {
                _logger.LogInformation("Encolando email de restablecimiento de contraseña para {Email}", email);
                _emailQueueService.QueuePasswordResetEmail(email, userId, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar email de restablecimiento de contraseña para {Email}", email);
                throw;
            }
        }

        public void QueueTwoFactorCodeEmail(string email, string code)
        {
            try
            {
                _logger.LogInformation("Encolando email con código de verificación para {Email}", email);
                _emailQueueService.QueueTwoFactorCodeEmail(email, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar email con código de verificación para {Email}", email);
                throw;
            }
        }

        public void QueueGenericEmail(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                _logger.LogInformation("Encolando email genérico para {To} con asunto '{Subject}'", to, subject);
                _emailQueueService.QueueGenericEmail(to, subject, body, isHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar email genérico para {To}", to);
                throw;
            }
        }
    }
}
