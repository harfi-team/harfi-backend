using Harfi.DTOs.RAG;
using Harfi.Services.Implementations;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Harfi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly RAGService _rag;
    private readonly IntentService _intent;
    private readonly ISolutionService _solution;
    private readonly VectorDbService _vectorDb;
    private readonly ILogger<AIController> _logger;

    private const string WelcomeMessage =
        "أهلاً بك! 👋\n" +
        "أنا مساعدك الذكي للعثور على أفضل الحرفيين في مصر.\n" +
        "أخبرني بمشكلتك وسأجد لك الحرفي المناسب فوراً! 🔧";

    public AIController(
        RAGService rag,
        IntentService intent,
        ISolutionService solution,
        VectorDbService vectorDb,
        ILogger<AIController> logger)
    {
        _rag = rag;
        _intent = intent;
        _solution = solution;
        _vectorDb = vectorDb;
        _logger = logger;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/AI/welcome
    // ════════════════════════════════════════════════════════════════════════

    [HttpGet("welcome")]
    public IActionResult Welcome() =>
        Ok(new { message = WelcomeMessage });

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/AI/chat3
    // ════════════════════════════════════════════════════════════════════════

    [HttpPost("chat3")]
    [ProducesResponseType(typeof(Chat3Response), 200)]
    public async Task<IActionResult> Chat3([FromBody] Chat3Request request)
    {
        if (request.Messages is null || request.Messages.Count == 0)
            return BadRequest(new { error = "المحادثة فاضية" });

        var sw = Stopwatch.StartNew();

        var lastUserMsg = request.Messages
            .LastOrDefault(m => m.Role == "user")?.Content ?? "";

        // ── 1. تحقق من اللغة ─────────────────────────────────────────────
        if (IsNonArabic(lastUserMsg))
        {
            sw.Stop();
            return Ok(new Chat3Response
            {
                IsComplete = false,
                Message = "برجاء الكتابة باللغة العربية حتى أتمكن من مساعدتك 😊",
                ExtractedService = request.ExtractedService,
                ExtractedCity = request.ExtractedCity,
                ExtractedCount = request.ExtractedCount,
                LatencyMs = sw.ElapsedMilliseconds
            });
        }

        // ── 2. تحقق من أكتر من تخصص ──────────────────────────────────────
        if (HasMultipleServiceKeywords(lastUserMsg) && request.ExtractedService is null)
        {
            sw.Stop();
            return Ok(new Chat3Response
            {
                IsComplete = false,
                Message = "أنا بساعدك في تخصص واحد في كل مرة 😊\nابدأ بأي مشكلة عاوز تحلها الأول؟",
                ExtractedService = null,
                ExtractedCity = request.ExtractedCity,
                ExtractedCount = null,
                LatencyMs = sw.ElapsedMilliseconds
            });
        }

        // ── 3. لو اختار "خطوات حل" ───────────────────────────────────────
        if (request.Intent == UserIntent.WantSteps
            && request.ExtractedService is not null)
        {
            // ── 3A. بننتظر إجابة "هل اتحلت المشكلة؟" ────────────────────
            if (request.FollowUpState == SolutionFollowUpState.WaitingAnswer)
            {
                bool solved = IsSolvedAnswer(lastUserMsg);
                bool notSolved = IsNotSolvedAnswer(lastUserMsg);

                if (solved)
                {
                    sw.Stop();
                    return Ok(new Chat3Response
                    {
                        IsComplete = false,
                        Message = "🎉 ممتاز! سعيد جداً إن المشكلة اتحلت.\n\nلو احتجت مساعدة في أي وقت، أنا هنا دايماً. 😊",
                        ExtractedService = request.ExtractedService,
                        ExtractedCity = request.ExtractedCity,
                        ExtractedCount = request.ExtractedCount,
                        FollowUpState = SolutionFollowUpState.None,
                        LastProblemDescription = null,
                        LatencyMs = sw.ElapsedMilliseconds
                    });
                }

                if (notSolved)
                {
                    sw.Stop();
                    return Ok(new Chat3Response
                    {
                        IsComplete = false,
                        Message = "حسناً، لنحاول مرة أخرى. 💪\n\nمن فضلك أخبرني بالتفصيل: أي جزء من المشكلة لا يزال موجوداً؟ وما الذي حدث عند تطبيق الخطوات؟",
                        ExtractedService = request.ExtractedService,
                        ExtractedCity = request.ExtractedCity,
                        ExtractedCount = request.ExtractedCount,
                        FollowUpState = SolutionFollowUpState.WaitingDetail,
                        LastProblemDescription = request.LastProblemDescription,
                        LatencyMs = sw.ElapsedMilliseconds
                    });
                }

                // مش واضح → اسأل تاني
                sw.Stop();
                return Ok(new Chat3Response
                {
                    IsComplete = false,
                    Message = "عذراً، لم أفهم إجابتك بوضوح. 😅\n\nهل تمكّنت الخطوات من حل المشكلة؟",
                    ShowSolvedQuestion = true,
                    ExtractedService = request.ExtractedService,
                    ExtractedCity = request.ExtractedCity,
                    ExtractedCount = request.ExtractedCount,
                    FollowUpState = SolutionFollowUpState.WaitingAnswer,
                    LastProblemDescription = request.LastProblemDescription,
                    LatencyMs = sw.ElapsedMilliseconds
                });
            }

            // ── 3B. بننتظر تفاصيل انهي جزء لسه فيه مشكلة ────────────────
            if (request.FollowUpState == SolutionFollowUpState.WaitingDetail)
            {
                string newProblem = lastUserMsg.Trim();
                if (string.IsNullOrEmpty(newProblem))
                    newProblem = request.LastProblemDescription ?? request.ExtractedService;

                var newSteps = await _solution.GetSolutionStepsAsync(
                    request.ExtractedService, newProblem);

                string newStepsMsg =
                    "بناءً على التفاصيل الإضافية التي ذكرتها، إليك خطوات أكثر تحديداً: 🔍\n\n" +
                    string.Join("\n", newSteps.Select((s, i) => $"✦ الخطوة {i + 1}: {s}"));

                sw.Stop();
                return Ok(new Chat3Response
                {
                    IsComplete = false,
                    Message = newStepsMsg,
                    SolutionSteps = newSteps,
                    ExtractedService = request.ExtractedService,
                    ExtractedCity = request.ExtractedCity,
                    ExtractedCount = request.ExtractedCount,
                    FollowUpState = SolutionFollowUpState.WaitingAnswer,
                    LastProblemDescription = newProblem,
                    ProblemClarificationAttempts = 0,
                    LatencyMs = sw.ElapsedMilliseconds
                });
            }

            // ── 3C. الحالة العادية — جيب خطوات ──────────────────────────
            string problem = (request.ProblemClarificationAttempts > 0 && lastUserMsg.Length > 5)
                ? lastUserMsg
                : ExtractProblemDescription(request.Messages, request.ExtractedService);

            if (problem == request.ExtractedService
                && request.ProblemClarificationAttempts == 0)
            {
                sw.Stop();
                return Ok(new Chat3Response
                {
                    IsComplete = false,
                    Message = "لمساعدتك بشكل أفضل، أحتاج مزيداً من التفاصيل حول المشكلة. 😊\n\n" +
                                                   GetExampleHint(request.ExtractedService),
                    ExtractedService = request.ExtractedService,
                    ExtractedCity = request.ExtractedCity,
                    ExtractedCount = request.ExtractedCount,
                    FollowUpState = SolutionFollowUpState.None,
                    ProblemClarificationAttempts = 1,
                    LatencyMs = sw.ElapsedMilliseconds
                });
            }

            var steps = await _solution.GetSolutionStepsAsync(
                request.ExtractedService, problem);

            string stepsMsg =
                "🔧 إليك خطوات عملية يمكنك تجربتها:\n\n" +
                string.Join("\n", steps.Select((s, i) => $"✦ الخطوة {i + 1}: {s}"));

            sw.Stop();
            return Ok(new Chat3Response
            {
                IsComplete = false,
                Message = stepsMsg,
                SolutionSteps = steps,
                ExtractedService = request.ExtractedService,
                ExtractedCity = request.ExtractedCity,
                ExtractedCount = request.ExtractedCount,
                FollowUpState = SolutionFollowUpState.WaitingAnswer,
                LastProblemDescription = problem,
                ProblemClarificationAttempts = 0,
                LatencyMs = sw.ElapsedMilliseconds
            });
        }

        // ── 3.5. رسالة حرة بتطلب خطوات ──────────────────────────────────
        if (request.ExtractedService is not null
            && request.Intent == UserIntent.NotAskedYet
            && IsWantsStepsMessage(lastUserMsg))
        {
            var reSteps = await _solution.GetSolutionStepsAsync(
                request.ExtractedService, lastUserMsg);

            string reStepsMsg =
                "🔧 إليك خطوات عملية يمكنك تجربتها:\n\n" +
                string.Join("\n", reSteps.Select((s, i) => $"✦ الخطوة {i + 1}: {s}"));

            sw.Stop();
            return Ok(new Chat3Response
            {
                IsComplete = false,
                Message = reStepsMsg,
                SolutionSteps = reSteps,
                ExtractedService = request.ExtractedService,
                ExtractedCity = request.ExtractedCity,
                ExtractedCount = request.ExtractedCount,
                FollowUpState = SolutionFollowUpState.WaitingAnswer,
                LastProblemDescription = lastUserMsg,
                ProblemClarificationAttempts = 0,
                LatencyMs = sw.ElapsedMilliseconds
            });
        }

        // ── 4. LLM يحلل المحادثة ──────────────────────────────────────────
        var extraction = await _intent.ExtractAsync(
            messages: request.Messages,
            knownService: request.ExtractedService,
            knownCity: request.ExtractedCity,
            knownCount: request.ExtractedCount,
            failedServiceAttempts: request.FailedServiceAttempts,
            failedCityAttempts: request.FailedCityAttempts,
            failedCountAttempts: request.FailedCountAttempts
        );

        _logger.LogInformation(
            "[AI] service={S} city={C} count={N} missing={M}",
            extraction.ServiceType, extraction.City,
            extraction.Count, extraction.Missing);

        // ── 5. عرف التخصص ولسه مسألش عن النية → اسأل ─────────────────────
        if (extraction.ServiceType is not null
            && request.Intent == UserIntent.NotAskedYet
            && request.ExtractedService is null)
        {
            sw.Stop();
            return Ok(new Chat3Response
            {
                IsComplete = false,
                Message = "تمام! فهمت إنك محتاج " + extraction.ServiceType + " 👍\nإيه اللي تحب أعمله؟",
                ShowIntentChoice = true,
                ExtractedService = extraction.ServiceType,
                ExtractedCity = extraction.City,
                ExtractedCount = extraction.Count,
                LatencyMs = sw.ElapsedMilliseconds
            });
        }

        // ── 6. كل البيانات كاملة → RAG ───────────────────────────────────
        if (extraction.Missing == "none"
            && extraction.ServiceType is not null
            && extraction.City is not null
            && extraction.Count is not null)
        {
            var ragResult = await _rag.QueryAsync(new QueryRequest
            {
                Question = extraction.ServiceType + " في " + extraction.City,
                TopK = extraction.Count.Value,
                ExtractedService = extraction.ServiceType,
                ExtractedCity = extraction.City
            });

            sw.Stop();
            return Ok(new Chat3Response
            {
                IsComplete = true,
                Message = "تمام! وجدت لك أفضل " + extraction.Count +
                                   " " + extraction.ServiceType +
                                   " في " + extraction.City + " 🎉\n\n" +
                                   ragResult.Answer,
                ExtractedService = extraction.ServiceType,
                ExtractedCity = extraction.City,
                ExtractedCount = extraction.Count,
                Result = ragResult,
                LatencyMs = sw.ElapsedMilliseconds
            });
        }

        // ── 7. لسه ناقص → رجّع السؤال ────────────────────────────────────
        sw.Stop();
        return Ok(new Chat3Response
        {
            IsComplete = false,
            Message = extraction.QuestionToAsk ?? "وضّحلي أكتر من فضلك.",
            ShowServicesList = extraction.ShowServicesList,
            ServicesList = extraction.ShowServicesList
                                   ? IntentService.ValidServices.ToList() : [],
            ShowCitiesList = extraction.ShowCitiesList,
            CitiesList = extraction.ShowCitiesList
                                   ? IntentService.ValidCities.ToList() : [],
            ExtractedService = extraction.ServiceType,
            ExtractedCity = extraction.City,
            ExtractedCount = extraction.Count,
            LatencyMs = sw.ElapsedMilliseconds
        });
    }

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/AI/ingest/craftsmen
    // ════════════════════════════════════════════════════════════════════════

    [HttpPost("ingest/craftsmen")]
    public async Task<IActionResult> IngestCraftsmen([FromQuery] int fromId = 0)
        => Ok(await _rag.IngestAllCraftsmenAsync(fromId));

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/AI/ingest/jobs
    // ════════════════════════════════════════════════════════════════════════

    [HttpPost("ingest/jobs")]
    public async Task<IActionResult> IngestJobs()
        => Ok(new { indexed = await _solution.IngestJobSolutionsAsync() });

    // ════════════════════════════════════════════════════════════════════════
    //  GET /api/AI/vectors/count
    // ════════════════════════════════════════════════════════════════════════

    [HttpGet("vectors/count")]
    public async Task<IActionResult> GetVectorCount()
        => Ok(new { totalVectors = await _vectorDb.CountAsync() });

    // ════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ════════════════════════════════════════════════════════════════════════

    private static string ExtractProblemDescription(
        List<ChatMsg> messages, string serviceType)
    {
        string[] nonDescriptive =
        [
            "ينفع", "عندي مشكله", "عندي مشكلة", "في مشكلة",
            "محتاج مساعدة", "محتاج مساعده", "عاوز فني",
            "عاوز خطوات", "هاي", "أهلا", "أهلاً", "مرحبا",
            "ايوه", "أيوه", "تمام", "أيوه ينفع"
        ];

        var meaningful = messages
            .Where(m => m.Role == "user")
            .Where(m =>
            {
                string c = m.Content.Trim();
                if (c.Length < 5) return false;
                if (IntentService.ValidServices.Any(s =>
                    string.Equals(c, s, StringComparison.OrdinalIgnoreCase))) return false;
                if (nonDescriptive.Any(nd =>
                    string.Equals(c.Trim(), nd, StringComparison.OrdinalIgnoreCase))) return false;
                if (IsCountOnlyRequest(c, serviceType)) return false;
                return true;
            })
            .ToList();

        if (!meaningful.Any()) return serviceType;

        return meaningful.OrderByDescending(m => m.Content.Length).First().Content;
    }

    private static bool IsCountOnlyRequest(string msg, string serviceType)
    {
        bool hasNumber = msg.Any(char.IsDigit) ||
                         msg.Contains("واحد") || msg.Contains("اتنين") ||
                         msg.Contains("تلاتة") || msg.Contains("اربعة") ||
                         msg.Contains("خمسة") || msg.Contains("ستة");

        if (!hasNumber) return false;

        bool hasServiceWord = serviceType.Split(' ').Any(w => msg.Contains(w));
        bool hasRequestWord = msg.Contains("عاوز") || msg.Contains("محتاج") ||
                              msg.Contains("نفر") || msg.Contains("حرفي");

        return hasNumber && (hasServiceWord || hasRequestWord);
    }

    private static string GetExampleHint(string serviceType) =>
        serviceType switch
        {
            "كهربائي" => "مثلاً: الكهرباء اتقطعت، أو القاطع بيطلع لوحده...",
            "سباك" => "مثلاً: حنفية بتقطر، أو بالوعة مسدودة...",
            "تكييف وتبريد" => "مثلاً: التكييف مش بيبرد، أو بيقطر مياه...",
            "نجار" => "مثلاً: باب مش بيقفل، أو سرير مكسور...",
            "دهان" => "مثلاً: الحيطة بتقشر، أو في بقع رطوبة...",
            "بناء" => "مثلاً: في تشقق في الحيطة، أو السقف بيرشح...",
            "حداد" => "مثلاً: باب حديد مش بيقفل، أو قضبان مكسورة...",
            "سيراميك" => "مثلاً: بلاطة مكسورة، أو سيراميك مرفوع...",
            _ => "مثلاً: اشرحلي المشكلة بالتفصيل..."
        };

    private static bool IsWantsStepsMessage(string msg)
    {
        string m = msg.Trim().ToLowerInvariant();
        string[] keywords =
        [
            "خطوات", "خطوه", "حل", "اصلح", "ازاي", "إزاي",
            "كيف", "طريقة", "ساعدني", "لسه", "لسا",
            "مش اتحل", "ما اتحلت", "موجودة", "مستمر"
        ];
        return keywords.Any(kw => m.Contains(kw));
    }

    private static bool IsSolvedAnswer(string msg)
    {
        string m = msg.Trim().ToLowerInvariant();
        string[] positives =
        [
            "ايوه", "أيوه", "اتحلت", "اتحل", "تمام", "نجح",
            "شغال", "صح", "اشتغل", "yes", "ok", "fixed",
            "كويس", "عظيم", "ممتاز", "حمد الله"
        ];
        return positives.Any(p => m.Contains(p));
    }

    private static bool IsNotSolvedAnswer(string msg)
    {
        string m = msg.Trim().ToLowerInvariant();
        string[] negatives =
        [
            "لا", "لأ", "لسه", "ما اتحلتش", "مش شغال",
            "مستمرة", "لم تحل", "no", "not fixed", "still",
            "زي ما هي", "نفس المشكلة"
        ];
        return negatives.Any(p => m.Contains(p));
    }

    private static bool IsNonArabic(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return false;
        var letters = msg.Where(char.IsLetter).ToList();
        if (letters.Count == 0) return false;
        return !letters.Any(c => c >= '\u0600' && c <= '\u06FF');
    }

    private static bool HasMultipleServiceKeywords(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return false;
        string[] indicators =
        [
            "سباك", "كهربائي", "نجار", "حداد", "دهان",
            "بناء", "تكييف", "زجاج", "سيراميك", "كاميرا",
            "جبس", "صيانة", "حنفية"
        ];
        int count = indicators.Count(kw => msg.Contains(kw));
        return count >= 2 && (msg.Contains(" و") || msg.Contains("و "));
    }

    private static string Str(Dictionary<string, object> d, string key)
    {
        if (!d.TryGetValue(key, out object? v)) return "-";
        return v is JsonElement je ? je.GetString() ?? "-" : v.ToString() ?? "-";
    }
}