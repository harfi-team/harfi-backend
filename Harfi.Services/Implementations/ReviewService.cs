using Harfi.DTOs.Review;
using Harfi.Models.Constants;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;


        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<CraftsmanReviewsResponseDto> GetCraftsmanReviewsAsync(int craftsmanId)
        {
            // Fetch all reviews from Repository (already projected to DTO)
            var reviews = await _reviewRepository
                .GetReviewsByCraftsmanIdAsync(craftsmanId);

            // Calculate average stars in memory (not in SQL)
            // because the list is already fetched and typically small
            var averageStars = reviews.Count > 0
                ? Math.Round(reviews.Average(r => r.Stars), 1)
                : 0.0;

            // Wrap in summary DTO — saves frontend from calculating stats
            return new CraftsmanReviewsResponseDto
            {
                CraftsmanId = craftsmanId,
                TotalReviews = reviews.Count,
                AverageStars = averageStars,
                Reviews = reviews
            };
        }

        public async Task<ServiceResult<ReviewResponseDto>> SubmitReviewAsync(CreateReviewDto dto, int customerId)
        {
            var job = await _reviewRepository.GetJobByIdAsync(dto.JobId);

            if (job == null)
                return ServiceResult<ReviewResponseDto>.Fail(
                    "الطلب غير موجود");

            if (job.Status != JobStatusConstants.Done)
                return ServiceResult<ReviewResponseDto>.Fail(
                    $"لا يمكن تقديم تقييم إلا بعد اكتمال الشغل. " +
                    $"حالة الطلب الحالية: {job.Status}");

            if (job.CustomerId != customerId)
                return ServiceResult<ReviewResponseDto>.Fail(
                    "أنت لست العميل صاحب هذا الطلب");

            var alreadyReviewed = await _reviewRepository
            .ReviewExistsAsync(dto.JobId, customerId);

            if (alreadyReviewed)
                return ServiceResult<ReviewResponseDto>.Fail(
                    "لقد قدمت تقييماً لهذا الطلب من قبل");


            if (dto.Stars < 1 || dto.Stars > 5)
                return ServiceResult<ReviewResponseDto>.Fail(
                    "التقييم يجب أن يكون بين 1 و 5 نجوم");

            if (dto.Comment != null && dto.Comment.Length > 1000)
                return ServiceResult<ReviewResponseDto>.Fail(
                    "التعليق لا يجب أن يتجاوز 1000 حرف");


            // ── ALL RULES PASSED — Build and save the review ──────────────────

            var review = new Review
            {
                JobId = dto.JobId,

                CustomerId = customerId,

             
                CraftsmanId = job.CraftsmanId ?? 0,

                Stars = dto.Stars,

                Comment = dto.Comment?.Trim(),

                CreatedAt = DateTime.UtcNow
            };


            var savedReview = await _reviewRepository.CreateReviewAsync(review);

            var response = new ReviewResponseDto
            {
                Id = savedReview.Id,
                JobId = savedReview.JobId,
                Stars = savedReview.Stars,
                Comment = savedReview.Comment,
                CustomerName = job.Customer?.Name ?? string.Empty,
                CreatedAt = savedReview.CreatedAt
            };

            return ServiceResult<ReviewResponseDto>.Ok(response);
        }
    }
}
