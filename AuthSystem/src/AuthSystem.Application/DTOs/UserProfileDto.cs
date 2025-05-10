using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Application.DTOs
{
    public class UserProfileDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? ProfilePictureUrl { get; set; }
    }
}