using System.ComponentModel.DataAnnotations;

namespace Harfi.DTOs.User
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }
    }
}

