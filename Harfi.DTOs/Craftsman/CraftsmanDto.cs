using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Craftsman
{
    public class CraftsmanDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // بيانات مأخوذة من جدول الـ User المرتبط به
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ProfileImageUrl { get; set; }
        // بيانات الحرفي المهنية
        public string ServiceType { get; set; }
        public string City { get; set; }
        public string Neighborhood { get; set; }
        public decimal? PriceRangeMin { get; set; }
        public decimal? PriceRangeMax { get; set; }
        public int Experience { get; set; }
        public bool IsApproved { get; set; }
        public bool IsAvailable { get; set; }
        public decimal Rating { get; set; }
        public string Bio { get; set; }
        public string NationalIdUrl { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
