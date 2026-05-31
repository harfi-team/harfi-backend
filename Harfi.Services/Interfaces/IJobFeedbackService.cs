using Harfi.DTOs.Review;
using Harfi.Services.Implementations;

namespace Harfi.Services.Interfaces
{
    public interface IJobFeedbackService
    {

        Task<ServiceResult<string>> SubmitFeedbackAsync(
    CreateJobFeedbackDto dto,
    int userId);
    }
}
