using Harfi.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Repositories.Interfaces
{
    public interface IJobFeedbackRepository
    {
      
        Task<RAGDocument?> GetRAGDocumentByIdAsync(int ragDocumentId);

      
        Task<bool> FeedbackExistsAsync(int ragDocumentId, int userId);

       
        Task<JobFeedback> CreateFeedbackAsync(JobFeedback feedback);
    }
}
