using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AuthSystem.Infrastructure.Email;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthSystem.IntegrationTests
{
    public class EmailTemplateServiceTests : IDisposable
    {
        private readonly Mock<ILogger<EmailTemplateService>> _loggerMock;
        private readonly string _templateDirectory;
        private readonly EmailTemplateService _service;

        public EmailTemplateServiceTests()
        {
            _loggerMock = new Mock<ILogger<EmailTemplateService>>();
            
            // Crear directorio temporal para las pruebas
            _templateDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_templateDirectory);
            
            // Crear una plantilla de prueba
            var templateContent = "<html><body>Hello {{Name}}!</body></html>";
            File.WriteAllText(Path.Combine(_templateDirectory, "TestTemplate.html"), templateContent);
            
            _service = new EmailTemplateService(_loggerMock.Object);
            
            // Usar reflection para establecer el directorio de plantillas
            var field = typeof(EmailTemplateService).GetField("_templateDirectory", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(_service, _templateDirectory);
        }

        [Fact]
        public async Task RenderTemplateAsync_WithValidTemplate_ReplacesVariables()
        {
            // Arrange
            var templateData = new Dictionary<string, string>
            {
                { "Name", "John" }
            };

            // Act
            var result = await _service.RenderTemplateAsync("TestTemplate", templateData);

            // Assert
            Assert.Equal("<html><body>Hello John!</body></html>", result);
        }

        [Fact]
        public async Task RenderTemplateAsync_WithMissingTemplate_ThrowsFileNotFoundException()
        {
            // Arrange
            var templateData = new Dictionary<string, string>
            {
                { "Name", "John" }
            };

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _service.RenderTemplateAsync("NonExistentTemplate", templateData));
        }

        [Fact]
        public async Task RenderTemplateAsync_WithMissingVariable_LogsWarning()
        {
            // Arrange
            var templateData = new Dictionary<string, string>
            {
                // No variables provided
            };

            // Act
            var result = await _service.RenderTemplateAsync("TestTemplate", templateData);

            // Assert
            Assert.Equal("<html><body>Hello {{Name}}!</body></html>", result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Variable no reemplazada")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        public void Dispose()
        {
            // Limpiar directorio temporal
            if (Directory.Exists(_templateDirectory))
            {
                Directory.Delete(_templateDirectory, true);
            }
        }
    }
}
