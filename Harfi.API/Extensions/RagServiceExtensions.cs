using Harfi.Repositories.Implementations;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Implementations;
using Harfi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Harfi.API.Extensions;

public static class RagServiceExtensions
{
    public static IServiceCollection AddRagHttpClients(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<ICraftsmanRepository, CraftsmanRepository>();

        services.AddHttpClient("Voyage", client =>
        {
            client.BaseAddress = new Uri("https://api.voyageai.com/");
            client.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {config["Voyage:ApiKey"]}");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddHttpClient("Groq", client =>
        {
            client.BaseAddress = new Uri("https://api.groq.com/");
            client.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {config["Groq:ApiKey"]}");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddHttpClient("GroqBase", client =>
        {
            client.BaseAddress = new Uri("https://api.groq.com/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddHttpClient("Qdrant", client =>
        {
            client.BaseAddress = new Uri(
                config["Qdrant:BaseUrl"] ?? "http://localhost:6400/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<EmbeddingService>();
        services.AddScoped<VectorDbService>();
        services.AddScoped<ChunkingService>();
        services.AddScoped<RAGService>();
        services.AddScoped<IntentService>();
        services.AddScoped<ISolutionService, SolutionService>();
        services.AddSingleton<GroqRotatingClient>();

        return services;
    }
}