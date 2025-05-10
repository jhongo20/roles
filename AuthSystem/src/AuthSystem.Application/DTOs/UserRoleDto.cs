using System;

namespace AuthSystem.Application.DTOs
{
    public class UserRoleDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; }
    }

    public class UserRoleResponseDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
