using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Review
{

    /// Data sent by the CUSTOMER when submitting a review.
    /// 
    /// What the client sends in POST /api/reviews body:
    /// {
    ///   "jobId": 5,
    ///   "stars": 4,
    ///   "comment": "حرفي محترم وشغله نضيف"
    /// }
    public class CreateReviewDto
    {

        [Required(ErrorMessage = "رقم الطلب مطلوب")]
        public int JobId { get; set; }


        [Required(ErrorMessage = "التقييم بالنجوم مطلوب")]
        [Range(1, 5, ErrorMessage = "التقييم يجب أن يكون بين 1 و 5 نجوم")]
        public int Stars { get; set; }


        [MaxLength(1000, ErrorMessage = "التعليق لا يجب أن يتجاوز 1000 حرف")]
        public string? Comment { get; set; }
    }
}
