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
    public class PermissionRepository : Repository<Permission>, IPermissionRepository
    {
        private readonly ILogger<PermissionRepository> _logger;

        public PermissionRepository(
            ApplicationDbContext context,
            ILogger<PermissionRepository> logger) : base(context)
        {
            _logger = logger;
        }

        public async Task<Permission> FindByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<Permission> GetByCodeAsync(string code)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<IReadOnlyList<Permission>> GetByCategoryAsync(string category)
        {
            return await _dbSet.Where(p => p.Category == category).ToListAsync();
        }

        public async Task<IReadOnlyList<Permission>> GetAllAsync(bool includeInactive = false)
        {
            try
            {
                IQueryable<Permission> query = _dbSet;

                // La entidad Permission no tiene propiedad IsActive, por lo que no filtramos por estado
                // Si en el futuro se agrega esta propiedad, descomentar el siguiente código:
                /*if (!includeInactive)
                {
                    query = query.Where(p => p.IsActive);
                }*/

                return await query.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los permisos");
                throw;
            }
        }

        public async Task<Permission> GetByIdAsync(Guid id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el permiso con ID {PermissionId}", id);
                throw;
            }
        }

        public async Task<Permission> CreateAsync(Permission permission)
        {
            try
            {
                await _dbSet.AddAsync(permission);
                await _context.SaveChangesAsync();
                return permission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el permiso {PermissionName}", permission.Name);
                throw;
            }
        }

        public async Task UpdateAsync(Permission permission)
        {
            try
            {
                _dbSet.Update(permission);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el permiso con ID {PermissionId}", permission.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var permission = await GetByIdAsync(id);
                if (permission != null)
                {
                    _dbSet.Remove(permission);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el permiso con ID {PermissionId}", id);
                throw;
            }
        }
    }
}
