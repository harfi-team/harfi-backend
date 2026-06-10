using Harfi.DTOs.Review;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Harfi.API.Controllers
{

    [Route("api/reviews")]
    [Authorize] // All endpoints require login by default
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IJobFeedbackService _feedbackService;

     
        public ReviewsController(
            IReviewService reviewService,
            IJobFeedbackService feedbackService)
        {
            _reviewService = reviewService;
            _feedbackService = feedbackService;
        }

        //  POST /api/reviews
        //  Only customers can submit reviews

        /// Submit a star review for a completed job.
        /// Request body: { "jobId": 5, "stars": 4, "comment": "ممتاز" }
        /// 
        /// Returns:
        ///   200 OK       → review saved successfully
        ///   400 BadReq   → business rule failed (job not done, duplicate, etc.)
        ///   401 Unauth   → not logged in
        ///   403 Forbid   → logged in but not a customer

        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> SubmitReview(
            [FromBody] CreateReviewDto dto)
        {
 
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

          
            if (customerIdClaim == null || !int.TryParse(customerIdClaim, out int customerId))
                return Unauthorized(new { message = "رمز المصادقة غير صالح" });

            var result = await _reviewService.SubmitReviewAsync(dto, customerId);

            if (!result.Success)
                return BadRequest(new { message = result.Error });

            return Ok(new
            {
                message = "تم إرسال تقييمك بنجاح",
                data = result.Data
            });
        }

        // ═══════════════════════════════════════════════════════════════════
        //  GET /api/reviews/craftsman/{craftsmanId}
        //  Public — no login needed

        /// Get all reviews for a specific craftsman.
        /// Includes average stars and total review count.
        /// 
        /// Used by:
        ///   - Craftsman profile page (frontend)
        ///   - Search results cards (shows star rating)
        /// 
        /// Returns:
        ///   200 OK  → craftsman reviews with stats
        ///             (empty reviews list if craftsman has no reviews yet)
        [HttpGet("craftsman/{craftsmanId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCraftsmanReviews(int craftsmanId)
        {
            var result = await _reviewService
                .GetCraftsmanReviewsAsync(craftsmanId);

            return Ok(result);
        }

        //  POST /api/reviews/rag-feedback
        //  Any logged-in user can give feedback on AI guides

        /// <summary>
        /// Record user feedback on an AI self-fix guide.
        /// 
        /// Called when user clicks:
        ///   "المحتوى ساعدني ✓"       → feedbackType: "helpful"
        ///   "ما زلت بحاجة لحرفي"    → feedbackType: "need_craftsman"
        /// 
        /// Request body: { "ragDocumentId": 7, "feedbackType": "helpful" }
        /// 
        /// Returns:
        ///   200 OK     → feedback saved, confirmation message returned
        ///   400 BadReq → invalid feedbackType or duplicate
        ///   401 Unauth → not logged in
        [HttpPost("rag-feedback")]
        [Authorize] // Any role can give feedback (customer or craftsman)
        public async Task<IActionResult> SubmitRagFeedback(
            [FromBody] CreateJobFeedbackDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "رمز المصادقة غير صالح" });

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "craftsman")
            {
                return Ok(new
                {
                    redirect = true,
                    url = "/api/craftsmen",
                    message = "يرجى استخدام endpoint الحرفيين"
                });
            }

            var result = await _feedbackService.SubmitFeedbackAsync(dto, userId);
            return Ok(new { message = result });
        }
    }

}
