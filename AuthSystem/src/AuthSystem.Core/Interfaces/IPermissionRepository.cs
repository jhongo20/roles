using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.Core.Entities;

namespace AuthSystem.Core.Interfaces
{
    public interface IPermissionRepository : IRepository<Permission>
    {
        Task<Permission> FindByCodeAsync(string code);
        Task<Permission> GetByCodeAsync(string code);
        Task<IReadOnlyList<Permission>> GetByCategoryAsync(string category);
        Task<IReadOnlyList<Permission>> GetAllAsync(bool includeInactive = false);
        Task<Permission> GetByIdAsync(Guid id);
        Task<Permission> CreateAsync(Permission permission);
        Task UpdateAsync(Permission permission);
        Task DeleteAsync(Guid id);
    }
}
