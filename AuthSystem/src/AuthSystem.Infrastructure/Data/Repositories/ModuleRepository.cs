using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Data.Repositories
{
    public class ModuleRepository : Repository<Module>, IModuleRepository
    {
        private readonly ILogger<ModuleRepository> _logger;

        public ModuleRepository(
            ApplicationDbContext context,
            ILogger<ModuleRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<IReadOnlyList<Module>> GetAllAsync(bool includeInactive = false)
        {
            try
            {
                IQueryable<Module> query = _dbSet;

                if (!includeInactive)
                {
                    query = query.Where(m => m.IsActive);
                }

                return await query
                    .OrderBy(m => m.ParentId)
                    .ThenBy(m => m.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los módulos");
                throw;
            }
        }

        public async Task<Module> GetByIdAsync(Guid id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el módulo con ID {ModuleId}", id);
                throw;
            }
        }

        // Nota: La entidad Module no tiene una propiedad Code, usamos Name como identificador alternativo
        public async Task<Module> GetByCodeAsync(string code)
        {
            try
            {
                // Usamos el Name como identificador ya que Code no existe
                return await _dbSet.FirstOrDefaultAsync(m => m.Name == code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el módulo con nombre {ModuleName}", code);
                throw;
            }
        }

        public async Task<Module> CreateAsync(Module module)
        {
            try
            {
                await _dbSet.AddAsync(module);
                await _context.SaveChangesAsync();
                return module;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el módulo {ModuleName}", module.Name);
                throw;
            }
        }

        public async Task UpdateAsync(Module module)
        {
            try
            {
                _dbSet.Update(module);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el módulo con ID {ModuleId}", module.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var module = await GetByIdAsync(id);
                if (module != null)
                {
                    _dbSet.Remove(module);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el módulo con ID {ModuleId}", id);
                throw;
            }
        }

        public async Task<IReadOnlyList<Module>> GetRootModulesAsync()
        {
            try
            {
                return await _dbSet.Where(m => m.ParentId == null && m.IsActive)
                    .OrderBy(m => m.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los módulos raíz");
                throw;
            }
        }

        public async Task<IReadOnlyList<Module>> GetChildModulesAsync(Guid parentId)
        {
            try
            {
                return await _dbSet.Where(m => m.ParentId == parentId && m.IsActive)
                    .OrderBy(m => m.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los submódulos del módulo con ID {ModuleId}", parentId);
                throw;
            }
        }

        public async Task<bool> HasChildModulesAsync(Guid moduleId)
        {
            try
            {
                return await _dbSet.AnyAsync(m => m.ParentId == moduleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el módulo con ID {ModuleId} tiene submódulos", moduleId);
                throw;
            }
        }

        public async Task<IReadOnlyList<Module>> GetUserModulesAsync(Guid userId)
        {
            try
            {
                // Obtener todos los permisos del usuario
                var userPermissions = new List<Guid>();

                // Permisos directos
                var directPermissions = await _context.UserPermissions
                    .Where(up => up.UserId == userId && up.IsGranted)
                    .Where(up => up.ExpirationDate == null || up.ExpirationDate > DateTime.UtcNow)
                    .Select(up => up.PermissionId)
                    .ToListAsync();

                userPermissions.AddRange(directPermissions);

                // Permisos basados en roles
                var rolePermissions = await _context.UserRoles
                    .Where(ur => ur.UserId == userId && ur.IsActive)
                    .Where(ur => ur.ExpirationDate == null || ur.ExpirationDate > DateTime.UtcNow)
                    .Join(_context.RolePermissions,
                        ur => ur.RoleId,
                        rp => rp.RoleId,
                        (ur, rp) => rp.PermissionId)
                    .Distinct()
                    .ToListAsync();

                userPermissions.AddRange(rolePermissions);
                userPermissions = userPermissions.Distinct().ToList();

                // Obtener módulos accesibles
                var moduleIds = await _context.ModulePermissions
                    .Where(mp => userPermissions.Contains(mp.PermissionId))
                    .Select(mp => mp.ModuleId)
                    .Distinct()
                    .ToListAsync();

                return await _dbSet
                    .Where(m => moduleIds.Contains(m.Id) && m.IsActive)
                    .OrderBy(m => m.ParentId)
                    .ThenBy(m => m.DisplayOrder)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los módulos del usuario con ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> AddPermissionAsync(ModulePermission modulePermission)
        {
            try
            {
                var existingPermission = await _context.ModulePermissions
                    .FirstOrDefaultAsync(mp => mp.ModuleId == modulePermission.ModuleId && 
                                         mp.PermissionId == modulePermission.PermissionId);

                if (existingPermission != null)
                {
                    return true; // Ya existe, no es necesario hacer nada
                }

                await _context.ModulePermissions.AddAsync(modulePermission);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asociar el permiso {PermissionId} al módulo {ModuleId}", 
                    modulePermission.PermissionId, modulePermission.ModuleId);
                throw;
            }
        }

        public async Task<bool> RemovePermissionAsync(Guid moduleId, Guid permissionId)
        {
            try
            {
                var modulePermission = await _context.ModulePermissions
                    .FirstOrDefaultAsync(mp => mp.ModuleId == moduleId && mp.PermissionId == permissionId);

                if (modulePermission == null)
                {
                    return false;
                }

                _context.ModulePermissions.Remove(modulePermission);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar el permiso {PermissionId} del módulo {ModuleId}", 
                    permissionId, moduleId);
                throw;
            }
        }

        public async Task<bool> HasPermissionsAsync(Guid moduleId)
        {
            try
            {
                return await _context.ModulePermissions.AnyAsync(mp => mp.ModuleId == moduleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el módulo con ID {ModuleId} tiene permisos asociados", moduleId);
                throw;
            }
        }

        public async Task<bool> HasPermissionAsync(Guid moduleId, Guid permissionId)
        {
            try
            {
                return await _context.ModulePermissions
                    .AnyAsync(mp => mp.ModuleId == moduleId && mp.PermissionId == permissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el módulo {ModuleId} tiene el permiso {PermissionId}", 
                    moduleId, permissionId);
                throw;
            }
        }

        public async Task<IReadOnlyList<Permission>> GetModulePermissionsAsync(Guid moduleId)
        {
            try
            {
                var permissionIds = await _context.ModulePermissions
                    .Where(mp => mp.ModuleId == moduleId)
                    .Select(mp => mp.PermissionId)
                    .ToListAsync();

                return await _context.Permissions
                    .Where(p => permissionIds.Contains(p.Id))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los permisos del módulo con ID {ModuleId}", moduleId);
                throw;
            }
        }
    }
}
