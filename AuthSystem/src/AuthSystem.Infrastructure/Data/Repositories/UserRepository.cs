using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Data/Repositories/UserRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Core.Constants;

namespace AuthSystem.Infrastructure.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> FindByUsernameAsync(string username)
        {
            return await _dbSet.Where(u => u.Username == username && !u.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            return await _dbSet.Where(u => u.Email == email && !u.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<User> FindByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _dbSet.Where(u =>
                    (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && !u.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<Role>> GetUserRolesAsync(Guid userId)
        {
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Where(ur => ur.ExpirationDate == null || ur.ExpirationDate > DateTime.UtcNow)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            return await _context.Roles
                .Where(r => roleIds.Contains(r.Id) && r.IsActive)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Permission>> GetUserPermissionsAsync(Guid userId)
        {
            // Obtener permisos directos
            var directPermissionIds = await _context.UserPermissions
                .Where(up => up.UserId == userId && up.IsGranted)
                .Where(up => up.ExpirationDate == null || up.ExpirationDate > DateTime.UtcNow)
                .Select(up => up.PermissionId)
                .ToListAsync();

            // Obtener roles activos del usuario
            var roleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive)
                .Where(ur => ur.ExpirationDate == null || ur.ExpirationDate > DateTime.UtcNow)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // Obtener permisos basados en roles
            var rolePermissionIds = await _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            // Combinar todos los IDs de permisos únicos
            var allPermissionIds = directPermissionIds.Union(rolePermissionIds).Distinct().ToList();

            // Obtener permisos
            return await _context.Permissions
                .Where(p => allPermissionIds.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<bool> AddToRoleAsync(Guid userId, Guid roleId, Guid? assignedBy = null)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole != null)
            {
                userRole.IsActive = true;
                userRole.AssignedBy = assignedBy;
                userRole.AssignedAt = DateTime.UtcNow;
                userRole.ExpirationDate = null;
            }
            else
            {
                userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedBy = assignedBy,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _context.UserRoles.AddAsync(userRole);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveFromRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null)
            {
                return false;
            }

            userRole.IsActive = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddPermissionAsync(Guid userId, Guid permissionId, bool isGranted = true, Guid? assignedBy = null)
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (userPermission != null)
            {
                userPermission.IsGranted = isGranted;
                userPermission.AssignedBy = assignedBy;
                userPermission.AssignedAt = DateTime.UtcNow;
                userPermission.ExpirationDate = null;
            }
            else
            {
                userPermission = new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    IsGranted = isGranted,
                    AssignedBy = assignedBy,
                    AssignedAt = DateTime.UtcNow
                };
                await _context.UserPermissions.AddAsync(userPermission);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemovePermissionAsync(Guid userId, Guid permissionId)
        {
            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId);

            if (userPermission == null)
            {
                return false;
            }

            _context.UserPermissions.Remove(userPermission);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsInRoleAsync(Guid userId, Guid roleId)
        {
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId &&
                                ur.IsActive && (ur.ExpirationDate == null || ur.ExpirationDate > DateTime.UtcNow));
        }

        public async Task<bool> HasPermissionAsync(Guid userId, Guid permissionId)
        {
            // Verificar permiso directo
            var hasDirectPermission = await _context.UserPermissions
                .AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId &&
                                up.IsGranted && (up.ExpirationDate == null || up.ExpirationDate > DateTime.UtcNow));

            if (hasDirectPermission)
            {
                return true;
            }

            // Verificar permisos basados en roles
            var userRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == userId && ur.IsActive &&
                           (ur.ExpirationDate == null || ur.ExpirationDate > DateTime.UtcNow))
                .Select(ur => ur.RoleId)
                .ToListAsync();

            return await _context.RolePermissions
                .AnyAsync(rp => userRoleIds.Contains(rp.RoleId) && rp.PermissionId == permissionId);
        }

        public async Task AddSessionAsync(UserSession session)
        {
            await _context.UserSessions.AddAsync(session);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> RevokeSessionAsync(Guid sessionId)
        {
            var session = await _context.UserSessions.FindAsync(sessionId);
            if (session == null)
            {
                return false;
            }

            session.RevokedAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RevokeAllUserSessionsAsync(Guid userId)
        {
            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();

            if (!sessions.Any())
            {
                return false;
            }

            var now = DateTime.UtcNow;
            foreach (var session in sessions)
            {
                session.RevokedAt = now;
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsTokenRevokedAsync(Guid userId, string jti)
        {
            var session = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Token.Contains(jti));

            return session == null || session.RevokedAt != null;
        }

        public async Task<UserTwoFactorSettings> GetTwoFactorSettingsAsync(Guid userId)
        {
            return await _context.UserTwoFactorSettings
                .FirstOrDefaultAsync(tf => tf.UserId == userId);
        }

        public async Task SaveTwoFactorSettingsAsync(UserTwoFactorSettings settings)
        {
            var existingSettings = await _context.UserTwoFactorSettings
                .FirstOrDefaultAsync(tf => tf.UserId == settings.UserId);

            if (existingSettings != null)
            {
                existingSettings.IsEnabled = settings.IsEnabled;
                existingSettings.Method = settings.Method;
                existingSettings.SecretKey = settings.SecretKey;
                existingSettings.RecoveryCodesJson = settings.RecoveryCodesJson;
                existingSettings.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                settings.UpdatedAt = DateTime.UtcNow;
                await _context.UserTwoFactorSettings.AddAsync(settings);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveTwoFactorSettingsAsync(Guid userId)
        {
            var settings = await _context.UserTwoFactorSettings
                .FirstOrDefaultAsync(tf => tf.UserId == userId);

            if (settings != null)
            {
                _context.UserTwoFactorSettings.Remove(settings);
                await _context.SaveChangesAsync();
            }
        }

        // Agregar a AuthSystem.Infrastructure/Data/Repositories/UserRepository.cs
        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName || r.NormalizedName == roleName.ToUpperInvariant());
        }

        public async Task StoreEmailConfirmationTokenAsync(Guid userId, string token)
        {
            // En una implementación real, deberías almacenar este token con una fecha de expiración
            // Aquí hay algunas opciones:

            // 1. Usando una tabla específica para tokens
            var tokenEntity = new EmailConfirmationToken
            {
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(3) // 3 días de expiración
            };

            await _context.EmailConfirmationTokens.AddAsync(tokenEntity);
            await _context.SaveChangesAsync();

            // 2. Alternativa: si no tienes una tabla específica, podrías almacenarlo en caché usando Redis
            // o podrías usar un campo en la tabla de usuarios para almacenar temporalmente este token

            /*
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                // Asumiendo que tienes estas propiedades en tu entidad User
                user.EmailConfirmationToken = token;
                user.EmailConfirmationTokenExpiry = DateTime.UtcNow.AddDays(3);
                await _context.SaveChangesAsync();
            }
            */
        }

        public async Task<(bool IsValid, User User)> ValidateEmailConfirmationTokenAsync(Guid userId, string token)
        {
            // Buscar el token en la base de datos
            var tokenEntity = await _context.EmailConfirmationTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token && t.ExpiresAt > DateTime.UtcNow);

            if (tokenEntity == null)
            {
                // Token no válido o expirado
                throw new InvalidOperationException("El token de confirmación de email no es válido o ha expirado.");
            }

            var user = await _dbSet.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("No se encontró el usuario asociado al token.");
            }

            return (true, user);

            // Alternativa si usas la segunda opción en StoreEmailConfirmationTokenAsync
            /*
            var user = await _dbSet
                .FirstOrDefaultAsync(u => u.Id == userId 
                                      && u.EmailConfirmationToken == token 
                                      && u.EmailConfirmationTokenExpiry > DateTime.UtcNow);

            return (user != null, user);
            */
        }

        public async Task<bool> ConfirmEmailAsync(Guid userId)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.ConfirmEmail();

            // Eliminar los tokens de confirmación asociados a este usuario
            var tokens = await _context.EmailConfirmationTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            _context.EmailConfirmationTokens.RemoveRange(tokens);

            await _context.SaveChangesAsync();
            return true;

            // Alternativa para la segunda opción
            /*
            user.EmailConfirmationToken = null;
            user.EmailConfirmationTokenExpiry = null;
            await _context.SaveChangesAsync();
            return true;
            */
        }

        // Nuevos métodos para la gestión del historial de contraseñas
        public async Task<IReadOnlyList<PasswordHistory>> GetPasswordHistoryAsync(Guid userId, int limit)
        {
            return await _context.PasswordHistory
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.ChangedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task AddPasswordToHistoryAsync(PasswordHistory passwordHistory)
        {
            // Obtener número total de entradas de historial para este usuario
            var count = await _context.PasswordHistory
                .CountAsync(ph => ph.UserId == passwordHistory.UserId);

            // Si superamos el límite de historial, eliminar las entradas más antiguas
            if (count >= SecurityConstants.PasswordHistoryLimit)
            {
                var oldestEntries = await _context.PasswordHistory
                    .Where(ph => ph.UserId == passwordHistory.UserId)
                    .OrderBy(ph => ph.ChangedAt)
                    .Take(count - SecurityConstants.PasswordHistoryLimit + 1)
                    .ToListAsync();

                _context.PasswordHistory.RemoveRange(oldestEntries);
            }

            // Añadir la nueva entrada
            await _context.PasswordHistory.AddAsync(passwordHistory);
            await _context.SaveChangesAsync();
        }

        // Implementación de los métodos de la interfaz IUserRepository
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet.Where(u => !u.IsDeleted).ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            await _dbSet.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await FindByUsernameAsync(username);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await FindByEmailAsync(email);
        }

        // Implementación de los métodos para tokens de confirmación de email
        public async Task SaveEmailConfirmationTokenAsync(EmailConfirmationToken token)
        {
            await _context.EmailConfirmationTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }

        public async Task<EmailConfirmationToken> GetEmailConfirmationTokenAsync(Guid userId, string token)
        {
            return await _context.EmailConfirmationTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token && t.ExpiresAt > DateTime.UtcNow);
        }

        public async Task DeleteEmailConfirmationTokenAsync(Guid tokenId)
        {
            var token = await _context.EmailConfirmationTokens.FindAsync(tokenId);
            if (token != null)
            {
                _context.EmailConfirmationTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAllEmailConfirmationTokensForUserAsync(Guid userId)
        {
            var tokens = await _context.EmailConfirmationTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();

            if (tokens.Any())
            {
                _context.EmailConfirmationTokens.RemoveRange(tokens);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(bool IsValid, User User)> ValidateCredentialsByEmailAsync(string email, string password)
        {
            var user = await FindByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el email '{email}'.");
            }

            // Verificar la contraseña usando BCrypt
            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return (isValid, user);
        }

        public async Task<(bool IsValid, User User)> ValidateCredentialsAsync(string username, string password)
        {
            var user = await FindByUsernameAsync(username);
            if (user == null)
            {
                throw new InvalidOperationException($"No se encontró ningún usuario con el nombre de usuario '{username}'.");
            }

            // Verificar la contraseña usando BCrypt
            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return (isValid, user);
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _dbSet.FindAsync(id);
            if (user != null)
            {
                user.Delete(); // Usar el método Delete() en lugar de asignar directamente a IsDeleted
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.Username == username && !u.IsDeleted);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted);
        }
    }
}
