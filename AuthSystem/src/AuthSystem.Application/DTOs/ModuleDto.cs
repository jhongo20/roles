using System;
using System.Collections.Generic;

namespace AuthSystem.Application.DTOs
{
    public class ModuleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public Guid? ParentId { get; set; }
        public string ParentModuleName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ModuleDto> ChildModules { get; set; } = new List<ModuleDto>();
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }

    public class ModuleResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public Guid? ParentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ModulePermissionResponseDto
    {
        public Guid ModuleId { get; set; }
        public string ModuleName { get; set; }
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string PermissionCode { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}