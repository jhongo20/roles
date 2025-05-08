using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Email
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly ILogger<EmailTemplateService> _logger;
        private readonly string _templateDirectory;

        public EmailTemplateService(ILogger<EmailTemplateService> logger)
        {
            _logger = logger;
            // Ruta a la carpeta de plantillas
            _templateDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email", "Templates");
        }

        public async Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> templateData)
        {
            try
            {
                // Asegurarse de que el nombre de la plantilla no contenga caracteres inválidos
                templateName = Path.GetFileNameWithoutExtension(templateName);
                string templatePath = Path.Combine(_templateDirectory, $"{templateName}.html");

                // Verificar si la plantilla existe
                if (!File.Exists(templatePath))
                {
                    _logger.LogError($"La plantilla '{templateName}.html' no existe en {_templateDirectory}");
                    throw new FileNotFoundException($"La plantilla '{templateName}.html' no existe");
                }

                // Leer el contenido de la plantilla
                string templateContent = await File.ReadAllTextAsync(templatePath);

                // Reemplazar todas las variables en la plantilla
                foreach (var kvp in templateData)
                {
                    templateContent = templateContent.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
                }

                // Verificar si hay variables no reemplazadas
                var unreplacedVariables = Regex.Matches(templateContent, @"\{\{([^{}]+)\}\}");
                if (unreplacedVariables.Count > 0)
                {
                    foreach (Match match in unreplacedVariables)
                    {
                        _logger.LogWarning($"Variable no reemplazada en la plantilla '{templateName}': {match.Groups[1].Value}");
                    }
                }

                return templateContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al renderizar la plantilla '{templateName}'");
                throw;
            }
        }
    }
}
