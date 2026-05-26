using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.User
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        [Phone]
        public string Phone { get; set; }

        [StringLength(500)]
        public string ProfileImageUrl { get; set; } // رابط الصورة الجديدة بعد رفعها على Cloudinary
    }
}

