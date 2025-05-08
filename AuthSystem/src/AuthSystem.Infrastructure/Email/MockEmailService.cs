using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Email
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;
        private readonly IEmailTemplateService _templateService;
        private readonly List<EmailMessage> _sentEmails = new List<EmailMessage>();

        public MockEmailService(ILogger<MockEmailService> logger, IEmailTemplateService templateService)
        {
            _logger = logger;
            _templateService = templateService;
        }

        public async Task SendAsync(string to, string subject, string body, bool isHtml = true)
        {
            var message = new EmailMessage
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = isHtml,
                SentAt = DateTime.UtcNow
            };

            _sentEmails.Add(message);
            _logger.LogInformation("Mock email sent to {To} with subject '{Subject}'", to, subject);
            
            await Task.CompletedTask;
        }

        public async Task SendConfirmationEmailAsync(string email, string userId, string token)
        {
            var templateData = new Dictionary<string, string>
            {
                { "UserName", email.Split('@')[0] },
                { "ConfirmationUrl", $"https://localhost:5001/auth/confirm-email?userId={userId}&token={token}" },
                { "CurrentYear", DateTime.Now.Year.ToString() },
                { "CompanyName", "AuthSystem" }
            };

            var body = await _templateService.RenderTemplateAsync("EmailConfirmation", templateData);
            await SendAsync(email, "Confirma tu cuenta", body);
        }

        public async Task SendPasswordResetEmailAsync(string email, string userId, string token)
        {
            var templateData = new Dictionary<string, string>
            {
                { "UserName", email.Split('@')[0] },
                { "ResetUrl", $"https://localhost:5001/auth/reset-password?userId={userId}&token={token}" },
                { "CurrentYear", DateTime.Now.Year.ToString() },
                { "CompanyName", "AuthSystem" }
            };

            var body = await _templateService.RenderTemplateAsync("PasswordReset", templateData);
            await SendAsync(email, "Recuperación de contraseña", body);
        }

        public async Task SendTwoFactorCodeAsync(string email, string code)
        {
            var templateData = new Dictionary<string, string>
            {
                { "UserName", email.Split('@')[0] },
                { "VerificationCode", code },
                { "CurrentYear", DateTime.Now.Year.ToString() },
                { "CompanyName", "AuthSystem" }
            };

            var body = await _templateService.RenderTemplateAsync("TwoFactorCode", templateData);
            await SendAsync(email, "Código de verificación", body);
        }

        public List<EmailMessage> GetSentEmails()
        {
            return _sentEmails;
        }

        public void ClearSentEmails()
        {
            _sentEmails.Clear();
        }
    }

    public class EmailMessage
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; }
        public DateTime SentAt { get; set; }
    }
}
