using Harfi.DTOs.Review;
using Harfi.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ServiceResult<ReviewResponseDto>> SubmitReviewAsync(
       CreateReviewDto dto,
       int customerId);

        /// Returns all reviews for a craftsman with summary statistics.
        Task<CraftsmanReviewsResponseDto> GetCraftsmanReviewsAsync(
         int craftsmanId);
    }
}
