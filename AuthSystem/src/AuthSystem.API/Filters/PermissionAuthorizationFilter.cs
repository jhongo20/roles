using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string PermissionCode { get; }

        public RequirePermissionAttribute(string permissionCode)
        {
            PermissionCode = permissionCode;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Obtener el ID del usuario del token JWT
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            try
            {
                var userPermissionService = context.HttpContext.RequestServices.GetRequiredService<IUserPermissionService>();
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequirePermissionAttribute>>();

                // Verificar si el usuario tiene el permiso requerido
                var hasPermission = await userPermissionService.UserHasPermissionAsync(
                    Guid.Parse(userId), 
                    PermissionCode
                );

                if (!hasPermission)
                {
                    logger.LogWarning("Usuario {UserId} intentó acceder a un recurso que requiere el permiso {PermissionCode} pero no lo tiene", userId, PermissionCode);
                    context.Result = new ForbidResult();
                    return;
                }

                logger.LogDebug("Usuario {UserId} autorizado con el permiso {PermissionCode}", userId, PermissionCode);
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequirePermissionAttribute>>();
                logger.LogError(ex, "Error al verificar permiso {PermissionCode} para usuario {UserId}", PermissionCode, userId);
                context.Result = new StatusCodeResult(500);
            }
        }
    }
}
