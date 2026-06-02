using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harfi.Services.Implementations;

public class GroqRotatingClient
{
    private readonly List<string> _keys;
    private int _currentIndex = 0;
    private readonly IHttpClientFactory _factory;
    private readonly ILogger<GroqRotatingClient> _logger;
    private readonly object _lock = new();

    public GroqRotatingClient(
        IConfiguration config,
        IHttpClientFactory factory,
        ILogger<GroqRotatingClient> logger)
    {
        _factory = factory;
        _logger = logger;

        var keys = config.GetSection("Groq:ApiKeys").Get<List<string>>();
        if (keys is { Count: > 0 })
            _keys = keys;
        else
        {
            var single = config["Groq:ApiKey"];
            _keys = string.IsNullOrEmpty(single) ? [] : [single];
        }

        if (_keys.Count == 0)
            throw new InvalidOperationException("No Groq API keys configured.");

        _logger.LogInformation("[GroqRotation] Loaded {N} API key(s)", _keys.Count);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, object payload)
    {
        int attempts = _keys.Count;

        for (int i = 0; i < attempts; i++)
        {
            string key;
            int idx;

            lock (_lock)
            {
                idx = _currentIndex % _keys.Count;
                key = _keys[idx];
            }

            var client = _factory.CreateClient("GroqBase");
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
            request.Content = JsonContent.Create(payload);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return response;

            if (response.StatusCode is HttpStatusCode.TooManyRequests
                                    or HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning(
                    "[GroqRotation] Key #{I} returned {S} → rotating", idx + 1,
                    (int)response.StatusCode);

                lock (_lock)
                {
                    if (_keys.Count == 1) return response;
                    _currentIndex = (idx + 1) % _keys.Count;
                }
                continue;
            }

            return response;
        }

        return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
    }
}