using Harfi.DTOs.Dispute;

namespace Harfi.Services.Interfaces;

public interface IDisputeService
{
    // Customer / Craftsman
    Task<DisputeDetailDto> OpenDisputeAsync(int jobId, int userId, string role, CreateDisputeRequest dto);
    Task<DisputeDetailDto?> GetDisputeForJobAsync(int jobId, int userId, string role);
    Task<IEnumerable<DisputeSummaryDto>> GetMyDisputesAsync(int userId);
    Task<DisputeDetailDto> RespondToDisputeAsync(int disputeId, int userId, string role, DisputeResponseRequest dto);
}
