using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harfi.Services.Implementations;

public class EmbeddingService
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        IHttpClientFactory factory,
        IConfiguration config,
        ILogger<EmbeddingService> logger)
    {
        _http = factory.CreateClient("Voyage");
        _model = config["Voyage:EmbeddingModel"] ?? "voyage-3";
        _logger = logger;
    }

    public async Task<List<float[]>> EmbedBatchAsync(List<string> texts)
    {
        var payload = new { input = texts, model = _model, input_type = "document" };
        return await CallAsync(payload);
    }

    public async Task<float[]> EmbedQueryAsync(string text)
    {
        var payload = new { input = new[] { text }, model = _model, input_type = "query" };
        return (await CallAsync(payload))[0];
    }

    private async Task<List<float[]>> CallAsync(object payload)
    {
        var resp = await _http.PostAsJsonAsync("v1/embeddings", payload);

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException(
                $"Voyage error {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");

        var result = await resp.Content.ReadFromJsonAsync<VoyageResponse>();
        if (result?.Data is null || result.Data.Count == 0)
            throw new InvalidOperationException("Voyage: empty response");

        return result.Data.OrderBy(d => d.Index).Select(d => d.Embedding).ToList();
    }

    private class VoyageResponse
    {
        [JsonPropertyName("data")] public List<VoyageItem> Data { get; set; } = [];
    }
    private class VoyageItem
    {
        [JsonPropertyName("index")] public int Index { get; set; }
        [JsonPropertyName("embedding")] public float[] Embedding { get; set; } = [];
    }
}