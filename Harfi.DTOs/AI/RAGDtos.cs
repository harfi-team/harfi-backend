namespace Harfi.DTOs.RAG;

// ── RAG DTOs ──────────────────────────────────────────────────────────────────

public class QueryRequest
{
    public string Question { get; set; } = string.Empty;
    public int TopK { get; set; } = 5;
    public string? ExtractedService { get; set; }
    public string? ExtractedCity { get; set; }
}

public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<RetrievedCraftsmanDto> RetrievedCraftsmen { get; set; } = new();
    public double LatencyMs { get; set; }
}

public class RetrievedCraftsmanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Neighborhood { get; set; }
    public double Rating { get; set; }
    public int ExperienceYears { get; set; }
    public decimal? PriceRangeMin { get; set; }
    public decimal? PriceRangeMax { get; set; }
    public string RelevantText { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public bool IsNearby { get; set; }
    public string? NearbyFromCity { get; set; }
}

public class IngestResponse
{
    public int TotalCraftsmen { get; set; }
    public int TotalChunksIndexed { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CraftsmanChunk
{
    public string ChromaId { get; set; } = string.Empty;
    public int CraftsmanId { get; set; }
    public string Text { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public Dictionary<string, string> Metadata { get; set; } = new();
}

// ── Chat DTOs ─────────────────────────────────────────────────────────────────

public enum UserIntent
{
    NotAskedYet = 0,
    WantCraftsman = 1,
    WantSteps = 2
}

public enum SolutionFollowUpState
{
    None = 0,
    WaitingAnswer = 1,
    WaitingDetail = 2
}

public class Chat3Request
{
    public List<ChatMsg> Messages { get; set; } = new();
    public string? ExtractedService { get; set; }
    public string? ExtractedCity { get; set; }
    public int? ExtractedCount { get; set; }
    public int FailedServiceAttempts { get; set; } = 0;
    public int FailedCityAttempts { get; set; } = 0;
    public int FailedCountAttempts { get; set; } = 0;
    public UserIntent Intent { get; set; } = UserIntent.NotAskedYet;
    public int ProblemClarificationAttempts { get; set; } = 0;
    public SolutionFollowUpState FollowUpState { get; set; } = SolutionFollowUpState.None;
    public string? LastProblemDescription { get; set; }
}

public class ChatMsg
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class Chat3Response
{
    public bool IsComplete { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool ShowServicesList { get; set; }
    public List<string> ServicesList { get; set; } = new();
    public bool ShowCitiesList { get; set; }
    public List<string> CitiesList { get; set; } = new();
    public bool ShowIntentChoice { get; set; }
    public List<string> SolutionSteps { get; set; } = new();
    public bool ShowSolvedQuestion { get; set; }
    public string? ExtractedService { get; set; }
    public string? ExtractedCity { get; set; }
    public int? ExtractedCount { get; set; }
    public int ProblemClarificationAttempts { get; set; } = 0;
    public SolutionFollowUpState FollowUpState { get; set; } = SolutionFollowUpState.None;
    public string? LastProblemDescription { get; set; }
    public QueryResponse? Result { get; set; }
    public double LatencyMs { get; set; }
}

public class LlmExtractionResult
{
    public string? ServiceType { get; set; }
    public string? City { get; set; }
    public int? Count { get; set; }
    public string Missing { get; set; } = "all";
    public string? QuestionToAsk { get; set; }
    public bool ShowServicesList { get; set; }
    public bool ShowCitiesList { get; set; }
}

public class ConversationRequest
{
    public List<ConversationMessage> Messages { get; set; } = new();
    public int TopK { get; set; } = 5;
}

public class ConversationMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ConversationResponse
{
    public bool NeedsMoreInfo { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DetectedService { get; set; }
    public string? DetectedCity { get; set; }
    public QueryResponse? Result { get; set; }
    public double LatencyMs { get; set; }
}

public class IntentAnalysis
{
    public bool IsComplete { get; set; }
    public string? ServiceType { get; set; }
    public string? City { get; set; }
    public string? MissingInfo { get; set; }
    public string? QuestionToAsk { get; set; }
    public string? CleanProblem { get; set; }
}

// ── Qdrant DTOs ───────────────────────────────────────────────────────────────

public class QdrantSearchResponse
{
    public List<QdrantScoredPoint> result { get; set; } = new();
    public string? status { get; set; }
}

public class QdrantScoredPoint
{
    public string id { get; set; } = string.Empty;
    public double score { get; set; }
    public Dictionary<string, object>? payload { get; set; }
}

public class QdrantCountResponse
{
    public QdrantCountResult? result { get; set; }
}

public class QdrantCountResult
{
    public int count { get; set; }
}

public class QdrantScrollResponse
{
    public QdrantScrollResult? result { get; set; }
}

public class QdrantScrollResult
{
    public List<QdrantScoredPoint>? points { get; set; }
}