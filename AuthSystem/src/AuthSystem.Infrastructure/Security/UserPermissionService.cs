using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Security
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserPermissionService> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

        public UserPermissionService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMemoryCache cache,
            ILogger<UserPermissionService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode)
        {
            try
            {
                // Intentar obtener los permisos del caché
                var cacheKey = $"user_permissions_{userId}";
                if (!_cache.TryGetValue(cacheKey, out IEnumerable<string> permissions))
                {
                    // Si no están en caché, obtenerlos de la base de datos
                    permissions = await GetUserPermissionsAsync(userId);
                    
                    // Guardar en caché
                    _cache.Set(cacheKey, permissions, _cacheExpiration);
                }

                // Verificar si el usuario tiene el permiso requerido
                return permissions.Contains(permissionCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar permiso {PermissionCode} para usuario {UserId}", permissionCode, userId);
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
        {
            try
            {
                // Obtener todos los roles del usuario
                var userRoles = await _userRepository.GetUserRolesAsync(userId);
                
                // Inicializar conjunto de permisos
                var permissions = new HashSet<string>();

                // Para cada rol, obtener sus permisos
                foreach (var role in userRoles)
                {
                    // Asegurarse de que estamos pasando el ID del rol, no el objeto Role completo
                    var rolePermissions = await _roleRepository.GetRolePermissionsAsync(role.Id);
                    foreach (var permission in rolePermissions)
                    {
                        permissions.Add(permission.Code);
                    }
                }

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener permisos para usuario {UserId}", userId);
                return Enumerable.Empty<string>();
            }
        }
    }
}
