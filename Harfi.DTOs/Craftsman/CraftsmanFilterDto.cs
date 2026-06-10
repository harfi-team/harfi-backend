using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Craftsman
{
    public class CraftsmanFilterDto
    {
        public string? ServiceType { get; set; }   // الفلترة حسب المهنة
        public string? City { get; set; }           // الفلترة حسب المدينة
        public decimal? MinRating { get; set; }     // الفلترة بالحرفيين الحاصلين على تقييم أعلى من رقم معين
        public int? MinExperience { get; set; }     // الفلترة بالحد الأدنى لسنوات الخبرة
    }
}
