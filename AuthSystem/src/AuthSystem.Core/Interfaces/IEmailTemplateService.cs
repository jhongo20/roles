using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthSystem.Core.Interfaces
{
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Renderiza una plantilla de correo electrónico con los datos proporcionados
        /// </summary>
        /// <param name="templateName">Nombre de la plantilla (sin extensión)</param>
        /// <param name="templateData">Diccionario con los datos para reemplazar en la plantilla</param>
        /// <returns>Contenido HTML renderizado</returns>
        Task<string> RenderTemplateAsync(string templateName, Dictionary<string, string> templateData);
    }
}
