using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Craftsman
{
    public class CreateCraftsmanDto
    {
        [Required]
        public int UserId { get; set; } // لربطه بحساب المستخدم الأساسي المفتوح حالياً

        [Required]
        [StringLength(50)]
        public string ServiceType { get; set; } // مثال: سباكة، كهرباء، نجارة

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(100)]
        public string Neighborhood { get; set; } // المنطقة أو الحي (nullable)

        public decimal? PriceRangeMin { get; set; }

        public decimal? PriceRangeMax { get; set; }

        [Required]
        public int? Experience { get; set; } // عدد سنوات الخبرة

        [StringLength(1000)]
        public string Bio { get; set; } // نبذة مخصصة في الحرفي عن نفسه

        [Required]
        [StringLength(500)]
        public string NationalIdUrl { get; set; } // رابط الصورة المرفوعة لبطاقة الرقم القومي من أجل موافقة الأدمن
    }

}

