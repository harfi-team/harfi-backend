namespace Harfi.Services.Interfaces;

public interface ISolutionService
{
    Task<List<string>> GetSolutionStepsAsync(string serviceType, string problemDescription);
    Task<int> IngestJobSolutionsAsync();
}