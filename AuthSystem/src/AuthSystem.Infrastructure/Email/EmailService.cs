// Email/EmailService.cs
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthSystem.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IEmailTemplateService _templateService;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailSettings> emailSettings,
            IEmailTemplateService templateService,
            ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _templateService = templateService;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body, bool isHtml = true)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(to);

            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            await client.SendMailAsync(message);
        }

        public async Task SendConfirmationEmailAsync(string email, string userId, string token)
        {
            try
            {
                var callbackUrl = $"{_emailSettings.WebsiteBaseUrl}/auth/confirm-email?userId={userId}&token={token}";

                var templateData = new Dictionary<string, string>
                {
                    { "UserName", email.Split('@')[0] }, // Usamos la parte del email antes del @ como nombre de usuario
                    { "ConfirmationUrl", callbackUrl },
                    { "CurrentYear", DateTime.Now.Year.ToString() },
                    { "CompanyName", _emailSettings.FromName }
                };

                var body = await _templateService.RenderTemplateAsync("EmailConfirmation", templateData);
                var subject = "Confirma tu cuenta";

                await SendAsync(email, subject, body);
                _logger.LogInformation($"Correo de confirmación enviado a {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de confirmación a {email}");
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string email, string userId, string token)
        {
            try
            {
                var callbackUrl = $"{_emailSettings.WebsiteBaseUrl}/auth/reset-password?userId={userId}&token={token}";

                var templateData = new Dictionary<string, string>
                {
                    { "UserName", email.Split('@')[0] }, // Usamos la parte del email antes del @ como nombre de usuario
                    { "ResetUrl", callbackUrl },
                    { "CurrentYear", DateTime.Now.Year.ToString() },
                    { "CompanyName", _emailSettings.FromName }
                };

                var body = await _templateService.RenderTemplateAsync("PasswordReset", templateData);
                var subject = "Recuperación de contraseña";

                await SendAsync(email, subject, body);
                _logger.LogInformation($"Correo de recuperación de contraseña enviado a {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de recuperación de contraseña a {email}");
                throw;
            }
        }

        public async Task SendTwoFactorCodeAsync(string email, string code)
        {
            try
            {
                var templateData = new Dictionary<string, string>
                {
                    { "UserName", email.Split('@')[0] }, // Usamos la parte del email antes del @ como nombre de usuario
                    { "VerificationCode", code },
                    { "CurrentYear", DateTime.Now.Year.ToString() },
                    { "CompanyName", _emailSettings.FromName }
                };

                var body = await _templateService.RenderTemplateAsync("TwoFactorCode", templateData);
                var subject = "Código de verificación";

                await SendAsync(email, subject, body);
                _logger.LogInformation($"Código de verificación enviado a {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar código de verificación a {email}");
                throw;
            }
        }
    }

    public class EmailSettings
    {
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string WebsiteBaseUrl { get; set; }
    }
}
