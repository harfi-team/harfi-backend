using System.Net.Http.Json;
using System.Text.Json;
using Harfi.DTOs.RAG;
using Microsoft.Extensions.Logging;

namespace Harfi.Services.Implementations;

public class VectorDbService
{
    private readonly HttpClient _http;
    private readonly ILogger<VectorDbService> _logger;
    //private const string Col = "craftsmen";
    private const string Col = "harfi_craftsmen";

    private const string ProblemsCol = "job_solutions";
    private const int VecDim = 1024;

    private static readonly JsonSerializerOptions Opts =
        new() { PropertyNameCaseInsensitive = true };

    private static readonly JsonSerializerOptions WriteOpts = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public VectorDbService(IHttpClientFactory factory, ILogger<VectorDbService> logger)
    {
        _http = factory.CreateClient("Qdrant");
        _logger = logger;
    }

    public async Task EnsureCollectionAsync()
    {
        var check = await _http.GetAsync($"collections/{Col}");
        if (check.IsSuccessStatusCode) { _logger.LogInformation("Collection '{C}' exists", Col); return; }

        var resp = await _http.PutAsJsonAsync($"collections/{Col}", new
        {
            vectors = new { size = VecDim, distance = "Cosine" }
        });

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Qdrant create error: {await resp.Content.ReadAsStringAsync()}");

        _logger.LogInformation("Collection '{C}' created (dim={D})", Col, VecDim);
    }

    public async Task AddChunksAsync(List<CraftsmanChunk> chunks)
    {
        var points = chunks.Select(c => new
        {
            id = ToUuid(c.ChromaId),
            vector = c.Embedding,
            payload = c.Metadata.ToDictionary(kv => kv.Key, kv => (object)kv.Value)
        }).ToList();

        var resp = await _http.PutAsJsonAsync($"collections/{Col}/points", new { points });

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Qdrant upsert error: {await resp.Content.ReadAsStringAsync()}");

        _logger.LogInformation("Upserted {N} points", chunks.Count);
    }

    public async Task<QdrantSearchResponse> SearchAsync(
        float[] embedding, int topK, string? serviceType, string? city)
    {
        var must = new List<object>();
        if (!string.IsNullOrEmpty(serviceType))
            must.Add(new { key = "service_type", match = new { value = serviceType } });
        if (!string.IsNullOrEmpty(city))
            must.Add(new { key = "city", match = new { value = city } });

        object body = must.Count > 0
            ? new { vector = embedding, limit = topK, with_payload = true, filter = new { must } }
            : new { vector = embedding, limit = topK, with_payload = true };

        var resp = await _http.PostAsJsonAsync($"collections/{Col}/points/search", body);

        if (!resp.IsSuccessStatusCode)
            resp = await _http.PostAsJsonAsync($"collections/{Col}/points/search",
                new { vector = embedding, limit = topK, with_payload = true });

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Qdrant search error: {await resp.Content.ReadAsStringAsync()}");

        return await resp.Content.ReadFromJsonAsync<QdrantSearchResponse>(Opts)
               ?? new QdrantSearchResponse();
    }

    public async Task<int> CountAsync()
    {
        var resp = await _http.PostAsJsonAsync($"collections/{Col}/points/count", new { exact = true });
        if (!resp.IsSuccessStatusCode) return 0;
        var r = await resp.Content.ReadFromJsonAsync<QdrantCountResponse>(Opts);
        return r?.result?.count ?? 0;
    }

    public async Task EnsureProblemsCollectionAsync()
    {
        var check = await _http.GetAsync($"collections/{ProblemsCol}");
        if (check.IsSuccessStatusCode) return;

        var resp = await _http.PutAsJsonAsync($"collections/{ProblemsCol}", new
        {
            vectors = new { size = VecDim, distance = "Cosine" }
        });

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Qdrant create job_solutions error: {await resp.Content.ReadAsStringAsync()}");

        _logger.LogInformation("Collection '{C}' created", ProblemsCol);
    }

    public async Task AddProblemsAsync(List<(int Id, string Text, float[] Embedding)> problems)
    {
        var points = problems.Select(p => new
        {
            id = ToUuid($"job-{p.Id}"),
            vector = p.Embedding,
            payload = new Dictionary<string, object>
            {
                ["problem_id"] = p.Id.ToString(),
                ["text"] = p.Text
            }
        }).ToList();

        var json = JsonSerializer.Serialize(new { points }, WriteOpts);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var resp = await _http.PutAsync($"collections/{ProblemsCol}/points", content);

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Qdrant upsert job_solutions error: {await resp.Content.ReadAsStringAsync()}");

        _logger.LogInformation("[JobSolutions] Upserted {N} points", problems.Count);
    }

    public async Task<QdrantSearchResponse> SearchProblemsAsync(
        float[] embedding, int topK = 3, double minScore = 0.75)
    {
        var body = new { vector = embedding, limit = topK, with_payload = true };
        var resp = await _http.PostAsJsonAsync(
            $"collections/{ProblemsCol}/points/search", body);

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Qdrant job_solutions search error: {await resp.Content.ReadAsStringAsync()}");

        var result = await resp.Content.ReadFromJsonAsync<QdrantSearchResponse>(Opts)
                     ?? new QdrantSearchResponse();

        if (result.result is not null && minScore > 0)
            result.result = result.result.Where(x => x.score >= minScore).ToList();

        return result;
    }

    public async Task<List<QdrantScoredPoint>> ScrollAsync(int limit = 10)
    {
        var resp = await _http.PostAsJsonAsync(
            $"collections/{Col}/points/scroll",
            new { limit, with_payload = true });
        if (!resp.IsSuccessStatusCode) return [];
        var r = await resp.Content.ReadFromJsonAsync<QdrantScrollResponse>(Opts);
        return r?.result?.points ?? [];
    }

    private static string ToUuid(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash).ToString();
    }
    public async Task DeletePointAsync(string pointId)
    {
        var payload = new { ids = new[] { pointId } };
        var response = await _http.PostAsJsonAsync($"collections/{Col}/points/delete", payload);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Qdrant delete failed: {error}");
        }
    }
}