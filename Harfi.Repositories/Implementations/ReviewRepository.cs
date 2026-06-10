using Harfi.DTOs.Review;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Implementations
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _db;

        public ReviewRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<Review> CreateReviewAsync(Review review)
        {
            _db.Reviews.Add(review);

            await _db.SaveChangesAsync();

            return review;
        }

        public async Task<Job?> GetJobByIdAsync(int jobId)
        {
            return await _db.Jobs
                 .Include(j => j.Craftsman)
                 .Include(j => j.Customer)
             .FirstOrDefaultAsync(j => j.Id == jobId);
        }

        public async Task<List<ReviewResponseDto>> GetReviewsByCraftsmanIdAsync(int craftsmanId)
        {
            return await _db.Reviews
               .Where(r => r.CraftsmanId == craftsmanId)

            .OrderByDescending(r => r.CreatedAt)


                .Select(r => new ReviewResponseDto
                {
                    Id = r.Id,
                    JobId = r.JobId,
                    Stars = r.Stars,
                    Comment = r.Comment,
                    CustomerName = r.Customer.Name,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> ReviewExistsAsync(int jobId, int customerId)
        {
            return await _db.Reviews.AnyAsync(r =>
                r.JobId == jobId &&
                r.CustomerId == customerId);
        }

        public async Task UpdateCraftsmanRatingAsync(int craftsmanId)
        {
            var avgStars = await _db.Reviews
                .Where(r => r.CraftsmanId == craftsmanId && !r.IsDeleted)
                .AverageAsync(r => (double?)r.Stars) ?? 0.0;

            var craftsman = await _db.Craftsmen.FindAsync(craftsmanId);
            if (craftsman is not null)
            {
                craftsman.Rating = Math.Round((decimal)avgStars, 2);
                await _db.SaveChangesAsync();
            }
        }
    }
}
