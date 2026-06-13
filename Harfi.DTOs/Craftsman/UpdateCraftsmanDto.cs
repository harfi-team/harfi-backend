using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Craftsman
{
    public class UpdateCraftsmanDto
    {

        public string Fullname { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;

        public int ServiceTypeId { get; set; }
        public int CityId { get; set; }
        public string? Neighborhood { get; set; }
        public decimal? PriceRangeMin { get; set; }
        public decimal? PriceRangeMax { get; set; }
        public int Experience { get; set; }
        public string? Bio { get; set; }
    }
}
