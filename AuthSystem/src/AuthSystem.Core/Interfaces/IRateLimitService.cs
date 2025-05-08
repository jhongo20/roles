using System;
using System.Threading.Tasks;

namespace AuthSystem.Core.Interfaces
{
    public interface IRateLimitService
    {
        /// <summary>
        /// Verifica si una operación está limitada por tasa de intentos
        /// </summary>
        /// <param name="key">Clave única para identificar la operación</param>
        /// <param name="maxAttempts">Número máximo de intentos permitidos</param>
        /// <param name="timeWindow">Ventana de tiempo para los intentos</param>
        /// <returns>True si se ha excedido el límite, false en caso contrario</returns>
        Task<bool> IsRateLimitedAsync(string key, int maxAttempts, TimeSpan timeWindow);
    }
}
