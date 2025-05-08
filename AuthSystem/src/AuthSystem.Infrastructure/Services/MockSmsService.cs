using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Services
{
    public class MockSmsService : ISmsService
    {
        private readonly ILogger<MockSmsService> _logger;

        public MockSmsService(ILogger<MockSmsService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string phoneNumber, string message)
        {
            _logger.LogInformation("MOCK SMS a {PhoneNumber}: {Message}", phoneNumber, message);
            return Task.CompletedTask;
        }

        public Task SendVerificationCodeAsync(string phoneNumber, string code)
        {
            _logger.LogInformation("MOCK Código de verificación a {PhoneNumber}: {Code}", phoneNumber, code);
            return Task.CompletedTask;
        }
    }
}
