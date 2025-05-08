using Azure;
using Azure.Communication.Sms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;

namespace AuthSystem.Infrastructure.Services
{
    public class AzureSmsService : ISmsService
    {
        private readonly SmsClient _smsClient;
        private readonly string _fromNumber;
        private readonly ILogger<AzureSmsService> _logger;

        public AzureSmsService(IOptions<AzureCommunicationSettings> settings, ILogger<AzureSmsService> logger)
        {
            _logger = logger;
            
            try
            {
                // Usar Connection String para inicializar el cliente
                _smsClient = new SmsClient(settings.Value.ConnectionString);
                _fromNumber = settings.Value.FromNumber;
                
                _logger.LogInformation("Azure SMS Service inicializado correctamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al inicializar Azure SMS Service");
                throw;
            }
        }

        public async Task SendAsync(string phoneNumber, string message)
        {
            try
            {
                var response = await _smsClient.SendAsync(
                    from: _fromNumber,
                    to: phoneNumber,
                    message: message);

                var messageId = response.Value.MessageId;
                
                // En la versión 1.0.1 de Azure.Communication.Sms, no hay una propiedad Successful o Error
                // Si la operación fue exitosa, no se lanzará una excepción
                _logger.LogInformation("SMS enviado exitosamente a {PhoneNumber}, ID: {MessageId}", phoneNumber, messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al enviar SMS a {PhoneNumber}", phoneNumber);
                throw;
            }
        }

        public async Task SendVerificationCodeAsync(string phoneNumber, string code)
        {
            var message = $"Tu código de verificación para AuthSystem es: {code}";
            await SendAsync(phoneNumber, message);
        }
    }

    public class AzureCommunicationSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string FromNumber { get; set; } = string.Empty;
    }
}
