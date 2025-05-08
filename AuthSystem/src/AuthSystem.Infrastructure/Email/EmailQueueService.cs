using System;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Email
{
    public class EmailQueueService : IEmailQueueService
    {
        private readonly BackgroundEmailSender _emailSender;
        private readonly ILogger<EmailQueueService> _logger;

        public EmailQueueService(BackgroundEmailSender emailSender, ILogger<EmailQueueService> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public void QueueConfirmationEmail(string email, Guid userId, string token)
        {
            try
            {
                _logger.LogInformation("Encolando email de confirmación para {Email}", email);
                
                var emailItem = new EmailQueueItem
                {
                    To = email,
                    UserId = userId,
                    Token = token,
                    EmailType = EmailType.Confirmation
                };
                
                _emailSender.QueueEmail(emailItem);
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
                
                var emailItem = new EmailQueueItem
                {
                    To = email,
                    UserId = userId,
                    Token = token,
                    EmailType = EmailType.PasswordReset
                };
                
                _emailSender.QueueEmail(emailItem);
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
                
                var emailItem = new EmailQueueItem
                {
                    To = email,
                    Token = code,
                    EmailType = EmailType.TwoFactorCode
                };
                
                _emailSender.QueueEmail(emailItem);
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
                
                var emailItem = new EmailQueueItem
                {
                    To = to,
                    Subject = subject,
                    Body = body,
                    IsHtml = isHtml,
                    EmailType = EmailType.Generic
                };
                
                _emailSender.QueueEmail(emailItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al encolar email genérico para {To}", to);
                throw;
            }
        }
    }
}
