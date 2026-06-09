using Harfi.DTOs.Review;
using Harfi.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Repositories.Interfaces
{
    public interface IReviewRepository
    {

        Task<Job?> GetJobByIdAsync(int jobId);

    
        Task<bool> ReviewExistsAsync(int jobId, int customerId);

  
        Task<Review> CreateReviewAsync(Review review);

  
        Task<List<ReviewResponseDto>> GetReviewsByCraftsmanIdAsync(int craftsmanId);

        Task UpdateCraftsmanRatingAsync(int craftsmanId);
    }
}
