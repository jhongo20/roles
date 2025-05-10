using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AuthSystem.Core.Interfaces
{
    public interface IUserPermissionService
    {
        /// <summary>
        /// Verifica si un usuario tiene un permiso específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionCode">Código del permiso</param>
        /// <returns>True si el usuario tiene el permiso, False en caso contrario</returns>
        Task<bool> UserHasPermissionAsync(Guid userId, string permissionCode);

        /// <summary>
        /// Obtiene todos los permisos de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de códigos de permisos</returns>
        Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);
    }
}
