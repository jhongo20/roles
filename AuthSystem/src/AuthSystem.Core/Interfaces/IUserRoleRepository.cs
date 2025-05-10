using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Core.Entities;

namespace AuthSystem.Core.Interfaces
{
    public interface IUserRoleRepository
    {
        /// <summary>
        /// Verifica si un rol está asignado a algún usuario
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <returns>True si el rol está asignado a al menos un usuario, False en caso contrario</returns>
        Task<bool> IsRoleAssignedToAnyUserAsync(Guid roleId);

        /// <summary>
        /// Obtiene todos los usuarios que tienen un rol específico
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <returns>Lista de usuarios con el rol especificado</returns>
        Task<IEnumerable<User>> GetUsersByRoleAsync(Guid roleId);

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol</param>
        /// <param name="assignedBy">ID del usuario que asigna el rol (opcional)</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario</returns>
        Task<bool> AssignRoleToUserAsync(Guid userId, Guid roleId, Guid? assignedBy = null);

        /// <summary>
        /// Elimina un rol de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol</param>
        /// <returns>True si la operación fue exitosa, False en caso contrario</returns>
        Task<bool> RemoveRoleFromUserAsync(Guid userId, Guid roleId);
    }
}
