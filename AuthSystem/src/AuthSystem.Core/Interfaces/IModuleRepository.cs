using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Core.Entities;

namespace AuthSystem.Core.Interfaces
{
    public interface IModuleRepository : IRepository<Module>
    {
        Task<IReadOnlyList<Module>> GetAllAsync(bool includeInactive = false);
        Task<Module> GetByIdAsync(Guid id);
        Task<Module> GetByCodeAsync(string code);
        Task<Module> CreateAsync(Module module);
        Task UpdateAsync(Module module);
        Task DeleteAsync(Guid id);
        Task<IReadOnlyList<Module>> GetRootModulesAsync();
        Task<IReadOnlyList<Module>> GetChildModulesAsync(Guid parentId);
        Task<bool> HasChildModulesAsync(Guid moduleId);
        Task<IReadOnlyList<Module>> GetUserModulesAsync(Guid userId);
        Task<bool> AddPermissionAsync(ModulePermission modulePermission);
        Task<bool> RemovePermissionAsync(Guid moduleId, Guid permissionId);
        Task<bool> HasPermissionsAsync(Guid moduleId);
        Task<bool> HasPermissionAsync(Guid moduleId, Guid permissionId);
        Task<IReadOnlyList<Permission>> GetModulePermissionsAsync(Guid moduleId);
    }
}
