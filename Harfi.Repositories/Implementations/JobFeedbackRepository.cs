using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Harfi.Repositories.Implementations
{
    public class JobFeedbackRepository : IJobFeedbackRepository
    {
        private readonly AppDbContext _db;

        public JobFeedbackRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<JobFeedback> CreateFeedbackAsync(JobFeedback feedback)
        {
            _db.JobFeedbacks.Add(feedback);
            await _db.SaveChangesAsync();
            return feedback;
        }

        public async Task<bool> FeedbackExistsAsync(int ragDocumentId, int userId)
        {
            return await _db.JobFeedbacks.AnyAsync(f =>
               f.RAGDocumentId == ragDocumentId &&
                f.UserId == userId);
        }

        public async Task<RAGDocument?> GetRAGDocumentByIdAsync(int ragDocumentId)
        {
            return await _db.RAGDocuments.FindAsync(ragDocumentId);

        }
    }
}
