using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Security/RecaptchaService.cs
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace AuthSystem.Infrastructure.Security
{
    public class RecaptchaService : IRecaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly RecaptchaSettings _settings;

        public RecaptchaService(HttpClient httpClient, IOptions<RecaptchaSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<bool> ValidateTokenAsync(string token, string ipAddress)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var response = await _httpClient.GetStringAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={_settings.SecretKey}&response={token}&remoteip={ipAddress}");

            var recaptchaResponse = JsonSerializer.Deserialize<RecaptchaResponse>(response);

            return recaptchaResponse?.Success == true && recaptchaResponse.Score >= _settings.MinimumScore;
        }
        
        /// <summary>
        /// Obtiene la configuración pública de reCAPTCHA (solo la clave del sitio)
        /// </summary>
        /// <returns>Configuración pública de reCAPTCHA</returns>
        public RecaptchaPublicConfig GetPublicConfig()
        {
            return new RecaptchaPublicConfig
            {
                SiteKey = _settings.SiteKey
            };
        }

        private class RecaptchaResponse
        {
            public bool Success { get; set; }
            public float Score { get; set; }
            public string Action { get; set; } = string.Empty;
            public string Hostname { get; set; } = string.Empty;
        }
    }

    public class RecaptchaSettings
    {
        public string SiteKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public float MinimumScore { get; set; } = 0.5f;
    }
}
