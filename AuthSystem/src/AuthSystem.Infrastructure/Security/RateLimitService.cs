using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AuthSystem.Infrastructure.Security
{
    public class RateLimitService : IRateLimitService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RateLimitService> _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();

        public RateLimitService(IDistributedCache cache, ILogger<RateLimitService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> IsRateLimitedAsync(string key, int maxAttempts, TimeSpan timeWindow)
        {
            var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            
            try
            {
                await lockObj.WaitAsync();
                
                var cacheKey = $"ratelimit:{key}";
                var cacheData = await _cache.GetStringAsync(cacheKey);
                
                RateLimitInfo rateLimitInfo;
                
                if (string.IsNullOrEmpty(cacheData))
                {
                    rateLimitInfo = new RateLimitInfo
                    {
                        Count = 1,
                        FirstAttemptTime = DateTime.UtcNow
                    };
                }
                else
                {
                    rateLimitInfo = JsonConvert.DeserializeObject<RateLimitInfo>(cacheData) ?? new RateLimitInfo
                    {
                        Count = 1,
                        FirstAttemptTime = DateTime.UtcNow
                    };
                    
                    // Si ha pasado el tiempo de ventana, reiniciar el contador
                    if (DateTime.UtcNow - rateLimitInfo.FirstAttemptTime > timeWindow)
                    {
                        rateLimitInfo = new RateLimitInfo
                        {
                            Count = 1,
                            FirstAttemptTime = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        rateLimitInfo.Count++;
                    }
                }
                
                // Guardar la información actualizada en la caché
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonConvert.SerializeObject(rateLimitInfo),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = timeWindow
                    });
                
                return rateLimitInfo.Count > maxAttempts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar límite de intentos para {Key}", key);
                return false; // En caso de error, permitir la operación
            }
            finally
            {
                lockObj.Release();
            }
        }

        private class RateLimitInfo
        {
            public int Count { get; set; }
            public DateTime FirstAttemptTime { get; set; }
        }
    }
}
