using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Review
{
    public class CraftsmanReviewsResponseDto
    {
        /// <summary>
        /// The craftsman whose reviews are being shown.
        /// </summary>
        public int CraftsmanId { get; set; }

        /// <summary>
        /// Total number of reviews this craftsman has received.
        /// Displayed on profile: "12 تقييم"
        /// </summary>
        public int TotalReviews { get; set; }

        /// <summary>
        /// Average of all star ratings rounded to 1 decimal.
        /// Example: 4.6 stars
        /// Displayed prominently on craftsman profile and search cards.
        /// 0.0 if no reviews yet.
        /// </summary>
        public double AverageStars { get; set; }

        /// <summary>
        /// The list of individual reviews, newest first.
        /// </summary>
        public List<ReviewResponseDto> Reviews { get; set; } = new();
    }
}
