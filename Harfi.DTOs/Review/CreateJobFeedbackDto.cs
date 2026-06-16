using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Review
{
    /// <summary>
    /// Data sent when user clicks feedback button on the AI self-fix guide screen.
    /// 
    /// POST /api/reviews/rag-feedback body:
    /// {
    ///   "ragDocumentId": 7,
    ///   "feedbackType": "helpful"
    /// }
    /// </summary>
    public class CreateJobFeedbackDto
    {
       
        [Required(ErrorMessage = "معرف الوثيقة مطلوب")]
        public int RAGDocumentId { get; set; }

        
        [Required(ErrorMessage = "نوع الرأي مطلوب")]
        public string FeedbackType { get; set; } = string.Empty;
    }
}
