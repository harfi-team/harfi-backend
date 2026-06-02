using Harfi.DTOs.Review;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Services.Implementations
{
    public class JobFeedbackService : IJobFeedbackService
    {
        private readonly IJobFeedbackRepository _feedbackRepository;

       
        private static readonly string[] ValidFeedbackTypes =
            ["helpful", "need_craftsman"];

        public JobFeedbackService(IJobFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }
        public async Task<ServiceResult<string>> SubmitFeedbackAsync(CreateJobFeedbackDto dto, int userId)
        {
            var ragDocument = await _feedbackRepository
    .GetRAGDocumentByIdAsync(dto.RAGDocumentId);

            if (ragDocument == null)
                return ServiceResult<string>.Fail(
                    "وثيقة الذكاء الاصطناعي غير موجودة");

           
            if (!ValidFeedbackTypes.Contains(dto.FeedbackType))
                return ServiceResult<string>.Fail(
                    "نوع الرأي غير صحيح. " +
                    "القيم المسموح بها: helpful, need_craftsman");

    
            var alreadySubmitted = await _feedbackRepository
                .FeedbackExistsAsync(dto.RAGDocumentId, userId);

            if (alreadySubmitted)
                return ServiceResult<string>.Fail(
                    "لقد قدمت رأيك على هذا المحتوى من قبل");


            // ── ALL RULES PASSED — Save feedback ──────────────────────────────
            var feedback = new JobFeedback
            {
                UserId = userId,
                RAGDocumentId = dto.RAGDocumentId,
                FeedbackType = dto.FeedbackType,
                CreatedAt = DateTime.UtcNow
            };

            await _feedbackRepository.CreateFeedbackAsync(feedback);

            var confirmationMessage = dto.FeedbackType == "helpful"
                ? "شكراً! سعداء أن المحتوى أفادك ✓"
                : "شكراً على رأيك! سنساعدك في العثور على حرفي مناسب";

            return ServiceResult<string>.Ok(confirmationMessage);
        }
    }
}
