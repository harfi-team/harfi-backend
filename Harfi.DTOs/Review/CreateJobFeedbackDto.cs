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
        /// <summary>
        /// Which RAG document (AI guide chunk) the user is rating.
        /// This ID comes from the RAGDocuments table.
        /// Frontend receives this when the AI guide is displayed.
        /// </summary>
        [Required(ErrorMessage = "معرف الوثيقة مطلوب")]
        public int RAGDocumentId { get; set; }

        /// <summary>
        /// What the user thought of the AI guide.
        /// Only two valid values (validated in Service):
        ///   "helpful"        → "المحتوى ساعدني ✓"
        ///   "need_craftsman" → "ما زلت بحاجة لحرفي"
        /// </summary>
        [Required(ErrorMessage = "نوع الرأي مطلوب")]
        public string FeedbackType { get; set; } = string.Empty;
    }
}
