using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Harfi.Repositories.Data;
using Harfi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harfi.Services.Implementations;

public class SolutionService : ISolutionService
{
    private readonly GroqRotatingClient _groqRotating;
    private readonly IConfiguration _config;
    private readonly ILogger<SolutionService> _logger;
    private readonly EmbeddingService _embedder;
    private readonly VectorDbService _vectorDb;
    private readonly AppDbContext _db;

    public SolutionService(
        IConfiguration config,
        ILogger<SolutionService> logger,
        EmbeddingService embedder,
        VectorDbService vectorDb,
        AppDbContext db,
        GroqRotatingClient groqRotating)
    {
        _groqRotating = groqRotating;
        _config = config;
        _logger = logger;
        _embedder = embedder;
        _vectorDb = vectorDb;
        _db = db;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  الـ Method الرئيسي
    // ════════════════════════════════════════════════════════════════════════

    public async Task<List<string>> GetSolutionStepsAsync(
        string serviceType, string problemDescription)
    {
        float[] qEmbed;
        try
        {
            qEmbed = await _embedder.EmbedQueryAsync(problemDescription);
            _logger.LogInformation("[Solution] Embedded problem description");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Solution] Embed failed: {M} → LLM fallback", ex.Message);
            return await GenerateStepsWithLlmAsync(serviceType, problemDescription);
        }

        try
        {
            var qdrantResult = await _vectorDb.SearchProblemsAsync(qEmbed, topK: 3, minScore: 0.0);
            var hits = qdrantResult.result ?? [];

            _logger.LogInformation("[Solution] Qdrant returned {C} candidates", hits.Count);

            foreach (var hit in hits.OrderByDescending(h => h.score))
            {
                int jobId = GetInt(hit.payload, "problem_id");

                var job = await _db.Jobs
                    .Include(j => j.Review)
                    .Include(j => j.Craftsman)
                    .FirstOrDefaultAsync(j => j.Id == jobId && j.Status == "done");

                if (job is null) continue;

                double boostedScore = ComputeBoostedScore(
                    qdrantScore: hit.score,
                    reviewStars: job.Review?.Stars,
                    craftsmanRating: (double?)job.Craftsman?.Rating,
                    completedAt: job.CompletedAt);

                _logger.LogInformation(
                    "[Solution] Checking job #{Id} | qdrant={Q:F3} boosted={B:F3}",
                    job.Id, hit.score, boostedScore);

                bool suitable = await IsMatchSuitableAsync(
                    serviceType,
                    problemDescription,
                    job.Description,
                    job.ProblemDescription ?? job.Description);

                if (suitable)
                {
                    _logger.LogInformation(
                        "[Solution] ✓ Match accepted: job #{Id} (boosted={B:F3})",
                        job.Id, boostedScore);

                    var rawSteps = ParseSteps(job.SolutionDescription ?? "");
                    return await PolishStepsWithLlmAsync(
                        serviceType, problemDescription, rawSteps, boostedScore);
                }

                _logger.LogInformation(
                    "[Solution] ✗ Not suitable: job #{Id} → trying next", job.Id);
            }

            _logger.LogInformation("[Solution] No suitable match → LLM generates");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Solution] Qdrant failed: {M} → LLM fallback", ex.Message);
        }

        return await GenerateStepsWithLlmAsync(serviceType, problemDescription);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Boosted Score
    //  60% Qdrant similarity + 25% Review.Stars + 10% Craftsman.Rating + 5% حداثة
    // ════════════════════════════════════════════════════════════════════════

    private static double ComputeBoostedScore(
        double qdrantScore,
        int? reviewStars,
        double? craftsmanRating,
        DateTime? completedAt)
    {
        double score = 0.60 * qdrantScore;

        if (reviewStars.HasValue)
            score += 0.25 * ((reviewStars.Value - 1) / 4.0);

        if (craftsmanRating.HasValue)
            score += 0.10 * (craftsmanRating.Value / 5.0);

        if (completedAt.HasValue)
        {
            double monthsAgo = (DateTime.UtcNow - completedAt.Value).TotalDays / 30.0;
            if (monthsAgo <= 12) score += 0.05;
            else if (monthsAgo <= 24) score += 0.025;
        }

        return Math.Round(score, 4);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  LLM يتحقق إن الحل مناسب للمشكلة
    // ════════════════════════════════════════════════════════════════════════

    private async Task<bool> IsMatchSuitableAsync(
        string serviceType, string problemDescription,
        string candidateTitle, string candidateText)
    {
        string prompt =
            "أنت مساعد متخصص في الصيانة المنزلية.\n\n" +
            "مهمتك: حدد إذا كانت الخطوات المقترحة مناسبة لمشكلة العميل.\n\n" +
            $"مشكلة العميل: {problemDescription}\n" +
            $"التخصص: {serviceType}\n\n" +
            $"الحل المقترح: {candidateTitle}\n" +
            $"وصف الحل: {candidateText}\n\n" +
            "هل هذا الحل مناسب لمشكلة العميل؟\n" +
            "رد بـ JSON فقط: {\"suitable\": true} أو {\"suitable\": false}";

        try
        {
            var payload = new
            {
                model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
                temperature = 0.0,
                max_tokens = 20,
                messages = new[]
                {
                    new { role = "system", content = prompt },
                    new { role = "user",   content = "هل الحل مناسب؟" }
                }
            };

            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
            if (!resp.IsSuccessStatusCode) return false;

            var result = await resp.Content.ReadFromJsonAsync<GroqResp>();
            string raw = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            string json = raw.Replace("```json", "").Replace("```", "").Trim();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("suitable").GetBoolean();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Solution] IsMatchSuitable failed: {M}", ex.Message);
            return false;
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  LLM يظبط صيغة الخطوات (لما Qdrant يلاقي match)
    // ════════════════════════════════════════════════════════════════════════

    private async Task<List<string>> PolishStepsWithLlmAsync(
        string serviceType, string problemDescription,
        List<string> rawSteps, double score)
    {
        string stepsText = string.Join("\n", rawSteps.Select((s, i) => $"{i + 1}. {s}"));

        string prompt =
            "أنت خبير في الصيانة المنزلية في مصر.\n\n" +
            "لديك خطوات حل جاهزة لمشكلة مشابهة، مهمتك:\n" +
            "- اعرض الخطوات بلغة عربية واضحة وسهلة\n" +
            "- خليها مناسبة للمشكلة المحددة اللي وصفها العميل\n" +
            "- ابدأ كل خطوة برقم متبوع بنقطة (1. 2. 3.)\n" +
            "- لا تضيف مقدمة أو خاتمة، الخطوات فقط\n\n" +
            "الخطوات المرجعية:\n" + stepsText;

        var payload = new
        {
            model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
            temperature = 0.2,
            max_tokens = 400,
            messages = new[]
            {
                new { role = "system", content = prompt },
                new { role = "user",
                      content = $"التخصص: {serviceType}\nمشكلة العميل: {problemDescription}\n\nاعرض الخطوات:" }
            }
        };

        try
        {
            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
            if (!resp.IsSuccessStatusCode) return rawSteps;

            var result = await resp.Content.ReadFromJsonAsync<GroqResp>();
            string raw = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            var steps = ParseSteps(raw);
            return steps.Count > 0 ? steps : rawSteps;
        }
        catch { return rawSteps; }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  LLM يكتب الخطوات من عنده (لما Qdrant ما يلاقيش)
    // ════════════════════════════════════════════════════════════════════════

    private async Task<List<string>> GenerateStepsWithLlmAsync(
        string serviceType, string problemDescription)
    {
        string prompt =
            "أنت خبير في الصيانة المنزلية في مصر.\n\n" +
            "لم يتم العثور على حل جاهز لهذه المشكلة في قاعدة البيانات.\n" +
            "مهمتك: إعطاء خطوات بسيطة وسريعة يمكن للشخص العادي تجربتها.\n\n" +
            "قواعد الإجابة:\n" +
            "- ابدأ بجملة: 'لم أجد في السجل حلاً محدداً لهذه المشكلة، لكن يمكنك تجربة:'\n" +
            "- اكتب من 3 إلى 5 خطوات فقط\n" +
            "- ابدأ كل خطوة برقم متبوع بنقطة (1. 2. 3.)\n" +
            "- لا تضيف أي مقدمة أو خاتمة غير المطلوبة";

        var payload = new
        {
            model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
            temperature = 0.3,
            max_tokens = 400,
            messages = new[]
            {
                new { role = "system", content = prompt },
                new { role = "user",
                      content = $"التخصص: {serviceType}\nالمشكلة: {problemDescription}\n\nاكتب الخطوات:" }
            }
        };

        try
        {
            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
            if (!resp.IsSuccessStatusCode) return DefaultSteps(serviceType);

            var result = await resp.Content.ReadFromJsonAsync<GroqResp>();
            string raw = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            var steps = ParseSteps(raw);
            return steps.Count > 0 ? steps : DefaultSteps(serviceType);
        }
        catch { return DefaultSteps(serviceType); }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Ingest — يرفع Jobs المكتملة على Qdrant (collection: job_solutions)
    // ════════════════════════════════════════════════════════════════════════

    public async Task<int> IngestJobSolutionsAsync()
    {
        await _vectorDb.EnsureProblemsCollectionAsync();

        var jobs = await _db.Jobs
            .Include(j => j.Review)
            .Include(j => j.Craftsman)
            .Where(j => j.Status == "done" && j.SolutionDescription != null)
            .AsNoTracking()
            .ToListAsync();

        _logger.LogInformation("[Jobs.Ingest] {N} completed jobs to index", jobs.Count);

        int done = 0;
        const int BatchSize = 5;

        for (int i = 0; i < jobs.Count; i += BatchSize)
        {
            var batch = jobs.Skip(i).Take(BatchSize).ToList();

            var texts = batch.Select(j =>
                $"{j.ServiceType} — {j.Description}: " +
                $"{j.ProblemDescription ?? j.Description} " +
                $"الحل: {j.SolutionDescription} " +
                $"التقييم: {j.Review?.Stars ?? 0}/5 " +
                $"تقييم الحرفي: {j.Craftsman?.Rating ?? 0}/5"
            ).ToList();

            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var embeddings = await _embedder.EmbedBatchAsync(texts);
                    var toUpsert = batch
                        .Select((j, idx) => (j.Id, texts[idx], embeddings[idx]))
                        .ToList();

                    await _vectorDb.AddProblemsAsync(toUpsert);
                    done += batch.Count;
                    _logger.LogInformation("[Jobs.Ingest] ✓ {D}/{T}", done, jobs.Count);
                    break;
                }
                catch (Exception) when (attempt < 3)
                {
                    _logger.LogWarning("[Jobs.Ingest] Attempt {A} failed — wait 65s", attempt);
                    await Task.Delay(65_000);
                }
                catch (Exception ex)
                {
                    _logger.LogError("[Jobs.Ingest] Failed: {M}", ex.Message);
                    return done;
                }
            }

            if (i + BatchSize < jobs.Count)
                await Task.Delay(25_000);
        }

        return done;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ════════════════════════════════════════════════════════════════════════

    private static List<string> ParseSteps(string raw)
    {
        var steps = new List<string>();
        foreach (var line in raw.Split('\n'))
        {
            string t = line.Trim();
            if (string.IsNullOrEmpty(t)) continue;
            if (t.Length > 2 && char.IsDigit(t[0]))
            {
                int dot = t.IndexOf('.');
                string s = dot >= 0 && dot < 3 ? t[(dot + 1)..].Trim() : t;
                if (!string.IsNullOrEmpty(s)) steps.Add(s);
            }
        }
        return steps;
    }

    private static int GetInt(Dictionary<string, object>? p, string key)
    {
        if (p is null || !p.TryGetValue(key, out var v)) return 0;
        return v switch
        {
            JsonElement je when je.ValueKind == JsonValueKind.Number => je.GetInt32(),
            JsonElement je when je.ValueKind == JsonValueKind.String =>
                int.TryParse(je.GetString(), out int n) ? n : 0,
            string s => int.TryParse(s, out int n2) ? n2 : 0,
            _ => 0
        };
    }

    private static List<string> DefaultSteps(string serviceType) =>
        serviceType switch
        {
            "كهربائي" => ["تحقق من لوحة الكهرباء وشوف لو في قاطع طلع لفوق",
                          "ارجع القاطع لتحت وشوف لو المشكلة اتحلت",
                          "تأكد إن الأجهزة مش بتاكل تيار زيادة",
                          "لو مفيش تحسن اتصل بكهربائي فوراً"],
            "سباك" => ["اقفل الماء من المحبس الرئيسي",
                          "تحقق من الوصلات وشوف لو في تسريب واضح",
                          "لو التسريب بسيط حاول تشد الوصلة",
                          "لو التسريب كبير اتصل بسباك فوراً"],
            "تكييف وتبريد" => ["تأكد إن التكييف فيه تيار كهربائي",
                                "نظف فلتر التكييف لو متسخ",
                                "شغّل التكييف على Cool وخفّض الدرجة",
                                "لو مرجعتش ابعت فني تكييف"],
            "نجار" => ["شوف لو المفصلة محتاجة ربط أو تشحيم",
                          "حاول تحرك الباب وحدد مكان المشكلة",
                          "لو الخشب متورم من الرطوبة انتظر يجف",
                          "لو مش قادر تحله اتصل بنجار"],
            _ => ["حاول تحدد مكان وسبب المشكلة بدقة",
                          "لو المشكلة بسيطة حاول تحلها بنفسك بحذر",
                          "لو مش متأكد لا تتدخل عشان متزيدش المشكلة",
                          "اتصل بفني متخصص في " + serviceType]
        };

    private class GroqResp
    {
        [JsonPropertyName("choices")] public List<GroqChoice> Choices { get; set; } = [];
    }
    private class GroqChoice
    {
        [JsonPropertyName("message")] public GroqMsg Message { get; set; } = new();
    }
    private class GroqMsg
    {
        [JsonPropertyName("content")] public string Content { get; set; } = "";
    }
}