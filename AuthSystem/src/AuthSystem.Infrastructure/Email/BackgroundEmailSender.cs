using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Email
{
    public class BackgroundEmailSender : BackgroundService
    {
        private readonly ILogger<BackgroundEmailSender> _logger;
        private readonly ConcurrentQueue<EmailQueueItem> _emailQueue = new ConcurrentQueue<EmailQueueItem>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private readonly IEmailService _emailService;

        public BackgroundEmailSender(ILogger<BackgroundEmailSender> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public void QueueEmail(EmailQueueItem emailItem)
        {
            if (emailItem == null)
            {
                throw new ArgumentNullException(nameof(emailItem));
            }

            _emailQueue.Enqueue(emailItem);
            _signal.Release();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Email Sender is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await _signal.WaitAsync(stoppingToken);
                
                if (_emailQueue.TryDequeue(out var emailItem))
                {
                    try
                    {
                        _logger.LogInformation("Procesando email para {To}", emailItem.To);
                        
                        switch (emailItem.EmailType)
                        {
                            case EmailType.Confirmation:
                                await _emailService.SendConfirmationEmailAsync(
                                    emailItem.To, 
                                    emailItem.UserId.ToString(), 
                                    emailItem.Token);
                                break;
                            case EmailType.PasswordReset:
                                await _emailService.SendPasswordResetEmailAsync(
                                    emailItem.To, 
                                    emailItem.UserId.ToString(), 
                                    emailItem.Token);
                                break;
                            case EmailType.TwoFactorCode:
                                await _emailService.SendTwoFactorCodeAsync(
                                    emailItem.To, 
                                    emailItem.Token);
                                break;
                            default:
                                await _emailService.SendAsync(
                                    emailItem.To, 
                                    emailItem.Subject, 
                                    emailItem.Body, 
                                    emailItem.IsHtml);
                                break;
                        }
                        
                        _logger.LogInformation("Email enviado exitosamente a {To}", emailItem.To);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al enviar email a {To}", emailItem.To);
                        
                        // Reintentar si no se ha excedido el número máximo de intentos
                        if (emailItem.RetryCount < emailItem.MaxRetries)
                        {
                            emailItem.RetryCount++;
                            _emailQueue.Enqueue(emailItem);
                            _signal.Release();
                            
                            // Esperar antes de reintentar
                            await Task.Delay(TimeSpan.FromSeconds(5 * emailItem.RetryCount), stoppingToken);
                        }
                        else
                        {
                            _logger.LogError("Se ha excedido el número máximo de intentos para enviar email a {To}", emailItem.To);
                        }
                    }
                }
            }

            _logger.LogInformation("Background Email Sender is stopping.");
        }
    }

    public class EmailQueueItem
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; } = true;
        public Guid? UserId { get; set; }
        public string Token { get; set; }
        public EmailType EmailType { get; set; }
        public int RetryCount { get; set; } = 0;
        public int MaxRetries { get; set; } = 3;
    }

    public enum EmailType
    {
        Generic,
        Confirmation,
        PasswordReset,
        TwoFactorCode
    }
}
