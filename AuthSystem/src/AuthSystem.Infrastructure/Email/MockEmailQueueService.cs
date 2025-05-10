using System;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Email
{
    /// <summary>
    /// Implementación mock del servicio de cola de emails para pruebas y desarrollo
    /// </summary>
    public class MockEmailQueueService : IEmailQueueService
    {
        private readonly ILogger<MockEmailQueueService> _logger;

        public MockEmailQueueService(ILogger<MockEmailQueueService> logger)
        {
            _logger = logger;
        }

        public void QueueConfirmationEmail(string email, Guid userId, string token)
        {
            _logger.LogInformation("MOCK: Email de confirmación enviado a {Email} con token {Token}", email, token);
        }

        public void QueuePasswordResetEmail(string email, Guid userId, string token)
        {
            _logger.LogInformation("MOCK: Email de restablecimiento de contraseña enviado a {Email} con token {Token}", email, token);
        }

        public void QueueTwoFactorCodeEmail(string email, string code)
        {
            _logger.LogInformation("MOCK: Email con código de verificación enviado a {Email} con código {Code}", email, code);
        }
        
        public void QueueGenericEmail(string to, string subject, string body, bool isHtml = true)
        {
            _logger.LogInformation("MOCK: Email genérico enviado a {To} con asunto {Subject}", to, subject);
        }
    }
}
