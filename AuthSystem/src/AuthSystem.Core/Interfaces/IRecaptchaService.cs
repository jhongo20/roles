using System.Threading.Tasks;

namespace AuthSystem.Core.Interfaces
{
    public interface IRecaptchaService
    {
        Task<bool> ValidateTokenAsync(string token, string ipAddress);
        
        /// <summary>
        /// Obtiene la configuración pública de reCAPTCHA (solo la clave del sitio)
        /// </summary>
        /// <returns>Configuración pública de reCAPTCHA</returns>
        RecaptchaPublicConfig GetPublicConfig();
    }
    
    /// <summary>
    /// Configuración pública de reCAPTCHA que puede ser compartida con el cliente
    /// </summary>
    public class RecaptchaPublicConfig
    {
        /// <summary>
        /// Clave del sitio para el cliente
        /// </summary>
        public string SiteKey { get; set; }
    }
}
