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
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRoleRepository> _logger;

        public UserRoleRepository(
            ApplicationDbContext context,
            ILogger<UserRoleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> IsRoleAssignedToAnyUserAsync(Guid roleId)
        {
            try
            {
                return await _context.UserRoles
                    .AnyAsync(ur => ur.RoleId == roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el rol {RoleId} está asignado a algún usuario", roleId);
                return false;
            }
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(Guid roleId)
        {
            try
            {
                return await _context.UserRoles
                    .Where(ur => ur.RoleId == roleId)
                    .Select(ur => ur.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios con el rol {RoleId}", roleId);
                return Enumerable.Empty<User>();
            }
        }

        public async Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId, Guid? assignedBy = null)
        {
            try
            {
                // Verificar si ya existe la asignación
                var existingAssignment = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

                if (existingAssignment != null)
                {
                    return true; // Ya existe la asignación
                }

                // Crear nueva asignación
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedBy = assignedBy,
                    AssignedAt = DateTime.UtcNow
                };

                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar el rol {RoleId} al usuario {UserId}", roleId, userId);
                return false;
            }
        }

        public async Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            try
            {
                var userRole = await _context.UserRoles
                    .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

                if (userRole == null)
                {
                    return false; // No existe la asignación
                }

                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol {RoleId} del usuario {UserId}", roleId, userId);
                return false;
            }
        }
    }
}
