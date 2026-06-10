using Harfi.DTOs.RAG;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Services.Implementations;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
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
    private readonly AppDbContext _db;
    private readonly GroqRotatingClient _groq;

    private const string WelcomeMessage =
        "أهلاً بك! 👋\n" +
        "أنا مساعدك الذكي للعثور على أفضل الحرفيين في مصر.\n" +
        "أخبرني بمشكلتك وسأجد لك الحرفي المناسب فوراً! 🔧";


    private readonly IWebHostEnvironment _env;
    private static readonly string ImagesFolder = Path.Combine("wwwroot", "AiChat", "images");
    private static readonly string AudioFolder = Path.Combine("wwwroot", "AiChat", "audio");

    public AIController(
            RAGService rag,
            IntentService intent,
            ISolutionService solution,
            VectorDbService vectorDb,
            ILogger<AIController> logger,
            AppDbContext db,
            GroqRotatingClient groq,
             IWebHostEnvironment env
            )
    {
        _rag = rag;
        _intent = intent;
        _solution = solution;
        _vectorDb = vectorDb;
        _logger = logger;
        _db = db;
        _groq = groq;
        _env = env;
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
            // ── 3AA. بننتظر feedback هل الخطوات مفيدة؟ ──────────────────
            if (request.FollowUpState == SolutionFollowUpState.WaitingFeedback)
            {
                bool helpful = IsHelpfulAnswer(lastUserMsg);
                bool notHelpful = IsNotHelpfulAnswer(lastUserMsg);

                if (notHelpful)
                {
                    _logger.LogInformation("[Feedback] Not helpful — skip saving");
                    sw.Stop();
                    return Ok(new Chat3Response
                    {
                        IsComplete = false,
                        Message = "شكراً على رأيك! 🙏 سأعمل على تحسين الخطوات.",
                        ExtractedService = request.ExtractedService,
                        ExtractedCity = request.ExtractedCity,
                        ExtractedCount = request.ExtractedCount,
                        FollowUpState = SolutionFollowUpState.None,
                        LatencyMs = sw.ElapsedMilliseconds
                    });
                }

                if (helpful)
                {
                    _logger.LogInformation("[Feedback] Helpful — saving to DB and Qdrant");
                    try
                    {
                        int aiUserId = request.UserId ?? 0;
                        if (aiUserId <= 0)
                        {
                            _logger.LogWarning("[Feedback] userId مش موجود في الـ request");
                            sw.Stop();
                            return Ok(new Chat3Response
                            {
                                IsComplete = false,
                                Message = "مش قادر أحفظ الخطوات — مفيش userId.",
                                LatencyMs = sw.ElapsedMilliseconds
                            });
                        }
                        var userCraftsman = await _db.Craftsmen
                            .FirstOrDefaultAsync(c => c.UserId == aiUserId);
                        int aiCraftsmanId = userCraftsman?.Id
                            ?? (await _db.Craftsmen.FirstOrDefaultAsync(c =>
                                c.UserId == _db.Users
                                    .Where(u => u.Email == "ai@harfi.com")
                                    .Select(u => u.Id)
                                    .FirstOrDefault()))!.Id;

                        //string stepsText = string.Join("\n", request.SolutionSteps
                        //    .Select((s, i) => $"{i + 1}. {s}"));


                        // جديد
                        _logger.LogInformation("[Feedback] SolutionSteps count={N}", request.SolutionSteps.Count);

                        var (desc, prob, sol) = await PrepareRagFieldsAsync(
                            request.ExtractedService ?? "صيانة",
                            request.LastProblemDescription ?? "",
                            request.SolutionSteps);


                        var job = new Job
                        {
                            CustomerId = aiUserId,   // دلوقتي بيبقى userId المستخدم الحالي
                            CraftsmanId = aiCraftsmanId,
                            Status = "AI",
                            ServiceType = request.ExtractedService ?? "صيانة عامة",
                            Description = desc,        // ← جديد
                            Address = "AI",
                            ProblemImageUrl = "AI",
                            ProblemDescription = prob,        // ← جديد
                            SolutionDescription = sol,         // ← جديد
                            CreatedAt = DateTime.UtcNow,
                            CompletedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _db.Jobs.Add(job);
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("[Feedback] ✓ Job saved Id={JobId}", job.Id);

                        var ragDoc = new RAGDocument
                        {
                            JobId = job.Id,
                            ChromaDocumentId = "AI",
                            ChunkType = "solution",
                            EmbeddingModel = "voyage-3",
                            CreatedAt = DateTime.UtcNow
                        };
                        _db.RAGDocuments.Add(ragDoc);
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("[Feedback] ✓ RAGDocument saved Id={RagId}", ragDoc.Id);

                        var feedback = new JobFeedback
                        {
                            UserId = aiUserId,
                            RAGDocumentId = ragDoc.Id,
                            FeedbackType = "helpful",
                            CreatedAt = DateTime.UtcNow
                        };
                        _db.JobFeedbacks.Add(feedback);
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("[Feedback] ✓ JobFeedback saved");

                        int upserted = await _solution.IngestSingleJobSolutionAsync(job.Id);
                        _logger.LogInformation("[Feedback] ✓ Qdrant upserted={N}", upserted);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("[Feedback] Error: {M}", ex.Message);
                    }

                    sw.Stop();
                    return Ok(new Chat3Response
                    {
                        IsComplete = false,
                        Message = "شكراً جزيلاً! 🙏 سيتم حفظ هذه الخطوات لمساعدة المزيد من الأشخاص. 💪",
                        ExtractedService = request.ExtractedService,
                        ExtractedCity = request.ExtractedCity,
                        ExtractedCount = request.ExtractedCount,
                        FollowUpState = SolutionFollowUpState.None,
                        LatencyMs = sw.ElapsedMilliseconds
                    });
                }

                // مش واضح → اسأل تاني
                sw.Stop();
                return Ok(new Chat3Response
                {
                    IsComplete = false,
                    Message = "لم أفهم إجابتك. هل كانت الخطوات مفيدة؟",
                    ExtractedService = request.ExtractedService,
                    ExtractedCity = request.ExtractedCity,
                    ExtractedCount = request.ExtractedCount,
                    FollowUpState = SolutionFollowUpState.WaitingFeedback,
                    LastProblemDescription = request.LastProblemDescription,
                    SolutionSteps = request.SolutionSteps,
                    ShowFeedbackQuestion = true,
                    LatencyMs = sw.ElapsedMilliseconds
                });
            }

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
                        Message = "🎉 ممتاز! سعيد جداً إن المشكلة اتحلت.\n\nهل كانت خطوات الحل مفيدة؟",
                        ExtractedService = request.ExtractedService,
                        ExtractedCity = request.ExtractedCity,
                        ExtractedCount = request.ExtractedCount,
                        FollowUpState = SolutionFollowUpState.WaitingFeedback,
                        LastProblemDescription = request.LastProblemDescription,
                        SolutionSteps = request.SolutionSteps,
                        ShowFeedbackQuestion = true,
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

            _logger.LogInformation("[3C] problem='{P}' service='{S}' equal={E}",
                problem, request.ExtractedService, problem == request.ExtractedService);

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
            // ── 6A. سألنا عن المدينة/الشارع في الرسالة السابقة → امسك الإجابة ──
            string? district = request.ExtractedDistrict;
            if (string.IsNullOrEmpty(district))
            {
                // تحقق: هل الرسالة السابقة من الـ assistant كانت سؤال عن المدينة؟
                var msgs = request.Messages;
                bool prevWasDistrictQuestion = msgs.Count >= 2
                    && msgs[^2].Role == "assistant"  // الرسالة قبل الأخيرة من الـ assistant
                    && (msgs[^2].Content.Contains("المدينة") || msgs[^2].Content.Contains("الشارع"));

                if (prevWasDistrictQuestion && msgs[^1].Role == "user")
                {
                    // رد المستخدم هو الـ district
                    district = msgs[^1].Content.Trim();
                }
            }

            // ── 6B. لو لسه مسألناش عن المدينة والشارع → اسأل ──────────────
            if (string.IsNullOrEmpty(district))
            {
                sw.Stop();
                return Ok(new Chat3Response
                {
                    IsComplete = false,
                    Message = "👍 تمام! وإيه المدينة والشارع اللي أنت فيه؟\n(مثلاً: مدينة نصر، شارع عباس العقاد)",
                    ExtractedService = extraction.ServiceType,
                    ExtractedCity = extraction.City,
                    ExtractedDistrict = null,
                    ExtractedCount = extraction.Count,
                    LatencyMs = sw.ElapsedMilliseconds
                });
            }

            // ── 6C. عندنا كل حاجة → RAG + re-rank ──────────────────────────
            var ragResult = await _rag.QueryAsync(new QueryRequest
            {
                Question = extraction.ServiceType + " في " + extraction.City,
                TopK = extraction.Count.Value,
                ExtractedService = extraction.ServiceType,
                ExtractedCity = extraction.City,
                ExtractedDistrict = district
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
                ExtractedDistrict = district,
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

    private static string ExtractProblemDescription(List<ChatMsg> messages, string serviceType)
    {
        string[] nonDescriptive =
[
    "ينفع", "عندي مشكله", "عندي مشكلة", "في مشكلة",
        "محتاج مساعدة", "محتاج مساعده", "عاوز فني",
        "عاوز خطوات", "هاي", "أهلا", "أهلاً", "مرحبا",
        "ايوه", "أيوه", "تمام", "أيوه ينفع",
        "🔧 عاوز خطوات حل المشكلة",
        "👷 عاوز فني متخصص",
        "عاوز خطوات حل المشكلة",
        "عاوز فني متخصص"
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

    private static bool IsHelpfulAnswer(string msg)
    {
        var lower = msg.Trim().ToLower();
        string[] positive = ["نعم", "ايوه", "أيوه", "ايوا", "اه", "آه",
                                 "مفيدة", "مفيده", "تمام", "ممتاز",
                                 "شكرا", "شكراً", "yes", "👍"];
        return positive.Any(w => lower.Contains(w));
    }

    private static bool IsNotHelpfulAnswer(string msg)
    {
        var lower = msg.Trim().ToLower();
        string[] negative = ["لا", "مش مفيدة", "مش مفيده", "no",
                                 "مفيدتش", "👎"];
        return negative.Any(w => lower.Contains(w));
    }


    private async Task<(string description, string problemDescription, string solutionDescription)>
    PrepareRagFieldsAsync(string serviceType, string problemDescription, List<string> steps)
    {
        string stepsText = string.Join("\n", steps.Select((s, i) => $"{i + 1}. {s}"));

        string prompt =
            $"أنت متخصص في {serviceType}.\n\n" +
            $"المشكلة التي أبلغ عنها المستخدم:\n{problemDescription}\n\n" +
            $"الخطوات التي حلت المشكلة:\n{stepsText}\n\n" +
            "اكتب بالعربية الفصحى البسيطة:\n" +
            "- لا تستخدم أي كلمات إنجليزية أو أحرف غير عربية\n" +
            "1. description: جملة واحدة تصف المشكلة والحل معاً بشكل مختصر\n" +
            "2. problem_description: جملتان تصفان المشكلة بدقة بكلمات مفيدة للبحث\n" +
            "3. solution_description: الخطوات مكتوبة بشكل نظيف ومرقم بدون تنسيق\n\n" +
            "رد بـ JSON فقط بدون أي كلام إضافي:\n" +
            "{\"description\": \"...\", \"problem_description\": \"...\", \"solution_description\": \"...\"}";

        try
        {
            string raw = await _groq.CompleteAsync(prompt, maxTokens: 500);
            string json = raw.Replace("```json", "").Replace("```", "").Trim();

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            string desc = root.GetProperty("description").GetString() ?? problemDescription;
            string prob = root.GetProperty("problem_description").GetString() ?? problemDescription;
            string sol = root.GetProperty("solution_description").GetString() ?? stepsText;

            _logger.LogInformation("[Feedback] ✓ RAG fields prepared — desc={D}", desc);
            return (desc, prob, sol);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Feedback] PrepareRagFields failed: {M} — using raw", ex.Message);
            string stepsRaw = string.Join("\n", steps.Select((s, i) => $"{i + 1}. {s}"));
            return (problemDescription, problemDescription, stepsRaw);
        }
    }
    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/AI/ingest/job/{jobId}
    // ════════════════════════════════════════════════════════════════════════

    [HttpPost("ingest/job/{jobId}")]
    public async Task<IActionResult> IngestSingleJob(int jobId)
    {
        _logger.LogInformation("[API] IngestSingleJob called for JobId={JobId}", jobId);
        var upserted = await _solution.IngestSingleJobSolutionAsync(jobId);
        return Ok(new { jobId, upserted });
    }
    private static bool IsIntentOnlyMessage(string msg)
    {
        string[] intentOnly =
        [
            "عاوز خطوات", "خطوات حل", "ابدأ", "هاي",
                "أهلا", "مرحبا", "ايوه", "أيوه", "تمام",
                "ينفع", "عاوز فني", "محتاج مساعدة", "عندي مشكلة"
        ];
        string m = msg.Trim();
        return intentOnly.Any(w =>
            string.Equals(m, w, StringComparison.OrdinalIgnoreCase));
    }
    private async Task<bool> IsProblemDescriptionAsync(string serviceType, string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim().Length < 4) return false;

        string prompt =
            $"أنت مساعد ذكي.\n\n" +
            $"التخصص: {serviceType}\n" +
            $"النص: \"{text}\"\n\n" +
            "هل هذا النص يصف مشكلة محددة يريد المستخدم حلها؟\n" +
            "أم أنه مجرد ذكر اسم التخصص أو طلب عام؟\n\n" +
            "رد بـ JSON فقط: {\"is_problem\": true} أو {\"is_problem\": false}";

        try
        {
            string raw = await _groq.CompleteAsync(prompt, maxTokens: 20);
            string json = raw.Replace("```json", "").Replace("```", "").Trim();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("is_problem").GetBoolean();
        }
        catch
        {
            return text.Length >= 8;
        }
    }



    [HttpPost("analyze-media")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AnalyzeMedia([FromForm] AnalyzeMediaDto dto)
    {
        var sw = Stopwatch.StartNew();

        bool hasImages = dto.Images is { Count: > 0 };
        bool hasAudio = dto.Audio is not null;

        if (!hasImages && !hasAudio)
        {
            return Ok(new Chat3Response
            {
                IsComplete = false,
                Message = "من فضلك ابعت صورة أو تسجيل صوتي للمشكلة. 😊",
                LatencyMs = sw.ElapsedMilliseconds
            });
        }

        // ════════════════════════════════════════════════════════════════
        //  💾 1) احفظ الـ files في wwwroot أول حاجة
        // ════════════════════════════════════════════════════════════════
        var savedImageUrls = new List<string>();
        string? savedAudioUrl = null;

        if (hasImages)
        {
            var dir = Path.Combine(_env.ContentRootPath, ImagesFolder);
            Directory.CreateDirectory(dir);
            foreach (var img in dto.Images!)
            {
                var ext = Path.GetExtension(img.FileName).ToLower();
                if (string.IsNullOrEmpty(ext)) ext = ".jpg";
                var name = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(dir, name);
                await using (var fs = System.IO.File.Create(path))
                    await img.OpenReadStream().CopyToAsync(fs);
                savedImageUrls.Add($"/AiChat/images/{name}");
            }
        }

        if (hasAudio)
        {
            var dir = Path.Combine(_env.ContentRootPath, AudioFolder);
            Directory.CreateDirectory(dir);
            var ext = Path.GetExtension(dto.Audio!.FileName).ToLower().TrimStart('.');
            if (string.IsNullOrEmpty(ext)) ext = "wav";
            var name = $"{Guid.NewGuid()}.{ext}";
            var path = Path.Combine(dir, name);
            await using (var fs = System.IO.File.Create(path))
                await dto.Audio.OpenReadStream().CopyToAsync(fs);
            savedAudioUrl = $"/AiChat/audio/{name}";
        }

        // ════════════════════════════════════════════════════════════════
        //  💾 2) احفظ رسالة الـ user في AIChatMessages (مع الـ media markers)
        // ════════════════════════════════════════════════════════════════
        if (dto.UserId.HasValue && !string.IsNullOrEmpty(dto.SessionId))
        {
            try
            {
                var userText = dto.UserText
                    ?? (hasImages && hasAudio ? "📷 صورة + 🎤 صوت"
                       : hasImages ? "📷 صورة"
                                                : "🎤 تسجيل صوتي");

                var userContent = BuildContent(userText, savedImageUrls, savedAudioUrl);
                _db.AIChatMessages.Add(new AIChatMessage
                {
                    UserId = dto.UserId.Value,
                    SessionId = dto.SessionId,
                    Role = "user",
                    Content = userContent.Length > 4000 ? userContent[..4000] : userContent,
                    CreatedAt = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[AnalyzeMedia] save user msg failed: {M}", ex.Message);
            }
        }

        try
        {
            // ════════════════════════════════════════════════════════════
            //  3) ابني payload للـ n8n (زي ما هو)
            // ════════════════════════════════════════════════════════════
            var payload = new Dictionary<string, object?>();

            if (hasImages)
            {
                var imageUrls = new List<string>();
                foreach (var img in dto.Images!)
                {
                    using var ms = new MemoryStream();
                    await img.OpenReadStream().CopyToAsync(ms);
                    var base64 = Convert.ToBase64String(ms.ToArray());
                    var mime = img.ContentType ?? "image/jpeg";
                    imageUrls.Add($"data:{mime};base64,{base64}");
                }
                payload["imageUrls"] = imageUrls;
            }

            if (hasAudio)
            {
                using var ms = new MemoryStream();
                await dto.Audio!.OpenReadStream().CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                payload["audioBase64"] = base64;

                var ext = Path.GetExtension(dto.Audio.FileName)?.TrimStart('.').ToLower() ?? "wav";
                payload["audioFormat"] = ext;
            }

            if (!string.IsNullOrWhiteSpace(dto.UserText))
                payload["userText"] = dto.UserText;

            if (!string.IsNullOrWhiteSpace(dto.ExtractedService))
                payload["extractedService"] = dto.ExtractedService;
            if (!string.IsNullOrWhiteSpace(dto.ExtractedCity))
                payload["extractedCity"] = dto.ExtractedCity;
            if (dto.ExtractedCount.HasValue)
                payload["extractedCount"] = dto.ExtractedCount.Value;

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            const string n8nUrl = "https://ahmeddabish2.app.n8n.cloud/webhook/analyze-media";

            var n8nResp = await http.PostAsync(n8nUrl, content);
            string respBody = await n8nResp.Content.ReadAsStringAsync();

            _logger.LogInformation("[AnalyzeMedia] n8n status={S} body={B}",
                n8nResp.StatusCode, respBody);

            // ════════════════════════════════════════════════════════════
            //  Helper: يحفظ رد assistant
            // ════════════════════════════════════════════════════════════
            async Task SaveAssistantAsync(string assistantMsg)
            {
                if (!dto.UserId.HasValue || string.IsNullOrEmpty(dto.SessionId)) return;
                try
                {
                    _db.AIChatMessages.Add(new AIChatMessage
                    {
                        UserId = dto.UserId.Value,
                        SessionId = dto.SessionId,
                        Role = "assistant",
                        Content = assistantMsg.Length > 4000 ? assistantMsg[..4000] : assistantMsg,
                        CreatedAt = DateTime.UtcNow.AddMilliseconds(1)
                    });
                    await _db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[AnalyzeMedia] save assistant msg failed: {M}", ex.Message);
                }
            }

            // ────────────────────────────────────────────────────────────
            if (!n8nResp.IsSuccessStatusCode)
            {
                var msg = "حصلت مشكلة في تحليل الوسائط. حاول تاني بعد شوية. 🙏";
                await SaveAssistantAsync(msg);
                return Ok(new Chat3Response
                {
                    IsComplete = false,
                    Message = msg,
                    LatencyMs = sw.ElapsedMilliseconds
                });
            }

            if (string.IsNullOrWhiteSpace(respBody))
            {
                var msg = "مش قادر أحدد المشكلة. حاول تاني. 🙏";
                await SaveAssistantAsync(msg);
                return Ok(new Chat3Response
                {
                    IsComplete = false,
                    Message = msg,
                    LatencyMs = sw.ElapsedMilliseconds
                });
            }

            using var doc = JsonDocument.Parse(respBody);
            var root = doc.RootElement;

            bool understood = root.TryGetProperty("understood", out var u) && u.ValueKind == JsonValueKind.True;

            if (!understood)
            {
                var msg = "مش قادر أحدد المشكلة من اللي بعته. 🤔\n\nممكن تبعت صورة أوضح، أو تسجيل صوتي تشرح فيه المشكلة بالتفصيل؟";
                await SaveAssistantAsync(msg);
                return Ok(new Chat3Response
                {
                    IsComplete = false,
                    Message = msg,
                    LatencyMs = sw.ElapsedMilliseconds
                });
            }

            string serviceType = root.TryGetProperty("service_type", out var st) ? st.GetString() ?? "صيانة عامة" : "صيانة عامة";
            string problemDesc = root.TryGetProperty("problem_description", out var pd) ? pd.GetString() ?? "" : "";

            var steps = await _solution.GetSolutionStepsAsync(serviceType, problemDesc);

            string stepsMsg =
                $"🔧 فهمت إن المشكلة في تخصص: {serviceType}\n\n" +
                $"📋 المشكلة: {problemDesc}\n\n" +
                "إليك خطوات عملية يمكنك تجربتها:\n\n" +
                string.Join("\n", steps.Select((s, i) => $"✦ الخطوة {i + 1}: {s}"));

            // ════════════════════════════════════════════════════════════
            //  💾 4) احفظ رد الـ assistant
            // ════════════════════════════════════════════════════════════
            await SaveAssistantAsync(stepsMsg);

            sw.Stop();
            return Ok(new Chat3Response
            {
                IsComplete = false,
                Message = stepsMsg,
                SolutionSteps = steps,
                ExtractedService = serviceType,
                ExtractedCity = dto.ExtractedCity,
                ExtractedCount = dto.ExtractedCount,
                FollowUpState = SolutionFollowUpState.WaitingAnswer,
                LastProblemDescription = problemDesc,
                ProblemClarificationAttempts = 0,
                LatencyMs = sw.ElapsedMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("[AnalyzeMedia] Error: {M}", ex.Message);
            sw.Stop();
            var msg = "حصلت مشكلة في تحليل الوسائط. حاول تاني. 🙏";

            // حفظ رد الـ error في الـ DB
            if (dto.UserId.HasValue && !string.IsNullOrEmpty(dto.SessionId))
            {
                try
                {
                    _db.AIChatMessages.Add(new AIChatMessage
                    {
                        UserId = dto.UserId.Value,
                        SessionId = dto.SessionId,
                        Role = "assistant",
                        Content = msg,
                        CreatedAt = DateTime.UtcNow.AddMilliseconds(1)
                    });
                    await _db.SaveChangesAsync();
                }
                catch { /* صامت */ }
            }

            return Ok(new Chat3Response
            {
                IsComplete = false,
                Message = msg,
                LatencyMs = sw.ElapsedMilliseconds
            });
        }
    }


    // ════════════════════════════════════════════════════════════════
    //  GET /api/AI/sessions/{userId}
    //  جيب ملخص كل محادثات المستخدم
    // ════════════════════════════════════════════════════════════════
    [HttpGet("sessions/{userId:int}")]
    public async Task<IActionResult> GetSessions(int userId)
    {
        var sessions = await _db.AIChatMessages
            .Where(m => m.UserId == userId)
            .GroupBy(m => m.SessionId)
            .Select(g => new AiSessionSummaryDto
            {
                SessionId = g.Key,
                Title = g.Where(m => m.ToolUsed != null && m.ToolUsed.StartsWith("__title__:"))
          .Select(m => m.ToolUsed!.Substring("__title__:".Length))
          .FirstOrDefault()
         ?? g.Where(m => m.Role == "user")
              .OrderBy(m => m.CreatedAt)
              .Select(m => m.Content).FirstOrDefault() ?? "محادثة جديدة",
                LastMessage = g.OrderByDescending(m => m.CreatedAt)
                                .Select(m => m.Content).FirstOrDefault() ?? "",
                LastActivity = g.Max(m => m.CreatedAt),
                MessageCount = g.Count()
            })
            .OrderByDescending(s => s.LastActivity)
            .ToListAsync();

        foreach (var s in sessions)
        {
            s.Title = CleanMediaMarkers(s.Title);
            s.LastMessage = CleanMediaMarkers(s.LastMessage);
            if (s.Title.Length > 60) s.Title = s.Title[..60] + "...";
            if (s.LastMessage.Length > 80) s.LastMessage = s.LastMessage[..80] + "...";
        }
        return Ok(sessions);
    }

    // ════════════════════════════════════════════════════════════════
    //  GET /api/AI/sessions/{userId}/{sessionId}
    //  جيب رسائل محادثة كاملة (مع تفكيك الـ media markers)
    // ════════════════════════════════════════════════════════════════
    [HttpGet("sessions/{userId:int}/{sessionId}")]
    public async Task<IActionResult> GetSessionDetail(int userId, string sessionId)
    {
        var rows = await _db.AIChatMessages
            .Where(m => m.UserId == userId && m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        if (!rows.Any()) return NotFound(new { error = "المحادثة مش موجودة" });

        var messages = rows.Select(m =>
        {
            var (text, imgs, aud) = ParseContent(m.Content);
            return new AiSessionMessageDto
            {
                Id = m.Id,
                Role = m.Role,
                Content = text,
                Images = imgs,
                Audio = aud,
                CreatedAt = m.CreatedAt
            };
        }).ToList();

        string title = messages.FirstOrDefault(x => x.Role == "user")?.Content ?? "محادثة جديدة";
        if (title.Length > 60) title = title[..60] + "...";

        return Ok(new AiSessionDetailDto
        {
            SessionId = sessionId,
            Title = title,
            Messages = messages
        });
    }

    // ════════════════════════════════════════════════════════════════
    //  POST /api/AI/sessions/message
    //  احفظ رسالة (مع صور/صوت اختياري) — multipart/form-data
    // ════════════════════════════════════════════════════════════════
    [HttpPost("sessions/message")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SaveMessage([FromForm] SaveMessageFormDto dto)
    {
        if (string.IsNullOrEmpty(dto.SessionId) || dto.UserId <= 0)
            return BadRequest(new { error = "userId و sessionId مطلوبين" });

        var imageUrls = new List<string>();
        string? audioUrl = null;

        // ── حفظ الصور في wwwroot/AiChat/images ──
        if (dto.Images is { Count: > 0 })
        {
            var dir = Path.Combine(_env.ContentRootPath, ImagesFolder);
            Directory.CreateDirectory(dir);
            foreach (var img in dto.Images)
            {
                var ext = Path.GetExtension(img.FileName).ToLower();
                if (string.IsNullOrEmpty(ext)) ext = ".jpg";
                var name = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(dir, name);
                await using var fs = System.IO.File.Create(path);
                await img.CopyToAsync(fs);
                imageUrls.Add($"/AiChat/images/{name}");
            }
        }

        // ── حفظ الصوت في wwwroot/AiChat/audio ──
        if (dto.Audio is not null)
        {
            var dir = Path.Combine(_env.ContentRootPath, AudioFolder);
            Directory.CreateDirectory(dir);
            var ext = Path.GetExtension(dto.Audio.FileName).ToLower().TrimStart('.');
            if (string.IsNullOrEmpty(ext)) ext = "wav";
            var name = $"{Guid.NewGuid()}.{ext}";
            var path = Path.Combine(dir, name);
            await using var fs = System.IO.File.Create(path);
            await dto.Audio.CopyToAsync(fs);
            audioUrl = $"/AiChat/audio/{name}";
        }

        // ── حفظ الـ record في AIChatMessages ──
        var content = BuildContent(dto.Content ?? "", imageUrls, audioUrl);
        var msg = new AIChatMessage
        {
            UserId = dto.UserId,
            SessionId = dto.SessionId,
            Role = dto.Role,
            Content = content.Length > 4000 ? content[..4000] : content,
            ToolUsed = dto.ToolUsed,
            CreatedAt = DateTime.UtcNow
        };
        _db.AIChatMessages.Add(msg);
        await _db.SaveChangesAsync();

        // ── ولّد عنوان لو دي أول رسالة user في الـ session ──
        if (dto.Role == "user")
        {
            bool isFirst = !await _db.AIChatMessages
                .AnyAsync(m => m.UserId == dto.UserId
                            && m.SessionId == dto.SessionId
                            && m.Id != msg.Id
                            && m.Role == "user");
            if (isFirst)
            {
                var generatedTitle = await GenerateSessionTitleAsync(dto.Content ?? "");
                msg.ToolUsed = $"__title__:{generatedTitle}";
                await _db.SaveChangesAsync();
            }
        }

        _logger.LogInformation("[AI/Save] msg={Id} session={S} role={R}", msg.Id, msg.SessionId, msg.Role);
        return Ok(new { id = msg.Id, images = imageUrls, audio = audioUrl });
    }

    // ════════════════════════════════════════════════════════════════
    //  DELETE /api/AI/sessions/{userId}/{sessionId}
    //  امسح محادثة كاملة + الملفات من الـ disk
    // ════════════════════════════════════════════════════════════════
    [HttpDelete("sessions/{userId:int}/{sessionId}")]
    public async Task<IActionResult> DeleteSession(int userId, string sessionId)
    {
        var rows = await _db.AIChatMessages
            .Where(m => m.UserId == userId && m.SessionId == sessionId)
            .ToListAsync();

        if (!rows.Any()) return NotFound(new { error = "المحادثة مش موجودة" });

        // ── امسح الملفات من wwwroot ──
        foreach (var m in rows)
        {
            var (_, imgs, aud) = ParseContent(m.Content);
            foreach (var u in imgs) TryDeleteFile(u);
            TryDeleteFile(aud);
        }

        _db.AIChatMessages.RemoveRange(rows);
        await _db.SaveChangesAsync();

        _logger.LogInformation("[AI/Delete] session={S} count={N}", sessionId, rows.Count);
        return Ok(new { deleted = rows.Count });
    }
    // ════════════════════════════════════════════════════════════════
    //  Media Markers Helpers
    //  الـ Content بيتخزن بصيغة:
    //  {{IMG:/AiChat/images/x.jpg}}{{AUD:/AiChat/audio/y.wav}}النص الأصلي
    // ════════════════════════════════════════════════════════════════

    private static string BuildContent(string text, List<string> images, string? audio)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var u in images)
            sb.Append("{{IMG:").Append(u).Append("}}");
        if (!string.IsNullOrEmpty(audio))
            sb.Append("{{AUD:").Append(audio).Append("}}");
        sb.Append(text);
        return sb.ToString();
    }

    private static (string text, List<string> images, string? audio) ParseContent(string content)
    {
        var imgs = new List<string>();
        string? aud = null;
        string text = content ?? "";

        var imgRx = new System.Text.RegularExpressions.Regex(@"\{\{IMG:([^}]+)\}\}");
        foreach (System.Text.RegularExpressions.Match m in imgRx.Matches(text))
            imgs.Add(m.Groups[1].Value);
        text = imgRx.Replace(text, "");

        var audRx = new System.Text.RegularExpressions.Regex(@"\{\{AUD:([^}]+)\}\}");
        var audMatch = audRx.Match(text);
        if (audMatch.Success) aud = audMatch.Groups[1].Value;
        text = audRx.Replace(text, "");

        return (text.Trim(), imgs, aud);
    }

    private static string CleanMediaMarkers(string content)
    {
        if (string.IsNullOrEmpty(content)) return "";
        var rx1 = new System.Text.RegularExpressions.Regex(@"\{\{IMG:[^}]+\}\}");
        var rx2 = new System.Text.RegularExpressions.Regex(@"\{\{AUD:[^}]+\}\}");
        return rx2.Replace(rx1.Replace(content, ""), "").Trim();
    }

    private void TryDeleteFile(string? relUrl)
    {
        if (string.IsNullOrEmpty(relUrl)) return;
        try
        {
            var rel = relUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var full = Path.Combine(_env.ContentRootPath, "wwwroot", rel);
            if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[AI] DeleteFile failed: {M}", ex.Message);
        }
    }


    // أضف الـ method دي جوه الـ AIController class

    private async Task<string> GenerateSessionTitleAsync(string firstUserMessage)
    {
        if (string.IsNullOrWhiteSpace(firstUserMessage)) return "محادثة جديدة";

        string prompt =
            "أنت مساعد ذكي. المستخدم كتب المشكلة دي:\n" +
            $"\"{firstUserMessage}\"\n\n" +
            "اكتب عنوان قصير من 3 كلمات بالعربية الفصحى يصف المشكلة.\n" +
            "مثال: \"حنفية بتقطر\" أو \"كهرباء مقطوعة\" أو \"باب مكسور\"\n" +
            "رد بالعنوان فقط بدون أي كلام إضافي أو علامات ترقيم.";

        try
        {
            string title = await _groq.CompleteAsync(prompt, maxTokens: 20);
            title = title.Trim().Trim('"').Trim();
            // تأكد مش أكتر من 3 كلمات
            var words = title.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 3) title = string.Join(" ", words.Take(3));
            return string.IsNullOrWhiteSpace(title) ? "محادثة جديدة" : title;
        }
        catch
        {
            return "محادثة جديدة";
        }
    }



    //// ════════════════════════════════════════════════════════════════════════
    ////  POST /api/AI/craftsman/submit-solution
    ////  الحرفي بيرسل خطوات الحل → LLM يصلحها → تتحفظ في DB + Qdrant
    //// ════════════════════════════════════════════════════════════════════════
    //[HttpPost("craftsman/submit-solution")]
    //public async Task<IActionResult> SubmitCraftsmanSolution([FromBody] CraftsmanSolutionDto dto)
    //{
    //    if (string.IsNullOrWhiteSpace(dto.ServiceType))
    //        return BadRequest(new { error = "التخصص مطلوب" });

    //    if (dto.Steps is null || dto.Steps.Count == 0)
    //        return BadRequest(new { error = "الخطوات مطلوبة" });

    //    // ── 1. LLM يصلح الخطوات ──────────────────────────────────────────
    //    string rawSteps = string.Join("\n", dto.Steps.Select((s, i) => $"{i + 1}. {s}"));

    //    string prompt =
    //        $"أنت حرفي متخصص في {dto.ServiceType}.\n\n" +
    //        $"المشكلة: {dto.ProblemDescription}\n\n" +
    //        $"الخطوات اللي كتبها الحرفي:\n{rawSteps}\n\n" +
    //        "المطلوب:\n" +
    //        "1. صحح الأخطاء الإملائية\n" +
    //        "2. رتب الخطوات بشكل منطقي لو محتاج\n" +
    //        "3. اكتبها بالعربية المصرية الشعبية البسيطة\n" +
    //        "4. كل خطوة تبدأ بفعل أمر واضح زي: افتح، افصل، نظف، ربط...\n" +
    //        "5. متزودش ولا تنقص خطوات — بس صحح ورتب اللي موجود\n\n" +
    //        "رد بـ JSON فقط بدون أي كلام:\n" +
    //        "{\"steps\": [\"الخطوة الأولى\", \"الخطوة التانية\", ...]}";

    //    List<string> fixedSteps;
    //    try
    //    {
    //        string raw = await _groq.CompleteAsync(prompt, maxTokens: 600);
    //        string json = raw.Replace("```json", "").Replace("```", "").Trim();
    //        using var doc = JsonDocument.Parse(json);
    //        fixedSteps = doc.RootElement
    //            .GetProperty("steps")
    //            .EnumerateArray()
    //            .Select(e => e.GetString() ?? "")
    //            .Where(s => !string.IsNullOrWhiteSpace(s))
    //            .ToList();

    //        if (fixedSteps.Count == 0) throw new Exception("steps فاضية");
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogWarning("[SubmitSolution] LLM fix failed: {M} — using raw steps", ex.Message);
    //        fixedSteps = dto.Steps; // fallback للخطوات الأصلية
    //    }

    //    // ── 2. حضّر الـ fields للـ RAG ────────────────────────────────────
    //    var (desc, prob, sol) = await PrepareRagFieldsAsync(
    //        dto.ServiceType,
    //        dto.ProblemDescription ?? dto.ServiceType,
    //        fixedSteps);

    //    // ── 3. احفظ في DB ────────────────────────────────────────────────
    //    if (dto.UserId <= 0)
    //        return BadRequest(new { error = "userId مطلوب" });

    //    var userCraftsman = await _db.Craftsmen
    //        .FirstOrDefaultAsync(c => c.UserId == dto.UserId);
    //    int craftsmanId = dto.CraftsmanId > 0
    //        ? dto.CraftsmanId
    //        : userCraftsman?.Id
    //          ?? (await _db.Craftsmen.FirstOrDefaultAsync(c =>
    //              c.UserId == _db.Users
    //                  .Where(u => u.Email == "ai@harfi.com")
    //                  .Select(u => u.Id)
    //                  .FirstOrDefault()))!.Id;

    //    var job = new Job
    //    {
    //        CustomerId = dto.UserId,
    //        CraftsmanId = craftsmanId,
    //        Status = "AI",
    //        ServiceType = dto.ServiceType,
    //        Description = desc,
    //        Address = "AI",
    //        ProblemImageUrl = "AI",
    //        ProblemDescription = prob,
    //        SolutionDescription = sol,
    //        CreatedAt = DateTime.UtcNow,
    //        CompletedAt = DateTime.UtcNow,
    //        UpdatedAt = DateTime.UtcNow
    //    };
    //    _db.Jobs.Add(job);
    //    await _db.SaveChangesAsync();
    //    _logger.LogInformation("[SubmitSolution] ✓ Job saved Id={JobId}", job.Id);

    //    // ── 4. RAGDocument ────────────────────────────────────────────────
    //    var ragDoc = new RAGDocument
    //    {
    //        JobId = job.Id,
    //        ChromaDocumentId = "AI",
    //        ChunkType = "solution",
    //        EmbeddingModel = "voyage-3",
    //        CreatedAt = DateTime.UtcNow
    //    };
    //    _db.RAGDocuments.Add(ragDoc);
    //    await _db.SaveChangesAsync();

    //    // ── 5. Ingest في Qdrant ───────────────────────────────────────────
    //    int upserted = await _solution.IngestSingleJobSolutionAsync(job.Id);
    //    _logger.LogInformation("[SubmitSolution] ✓ Qdrant upserted={N}", upserted);

    //    return Ok(new
    //    {
    //        jobId = job.Id,
    //        upserted,
    //        originalSteps = dto.Steps,
    //        fixedSteps
    //    });
    //}

    // ════════════════════════════════════════════════════════════════════════
    //  POST /api/AI/craftsman/check-and-submit-solution
    //  LLM يتحقق + يصلح إملاء في call واحدة → لو مناسبة يحفظ في SQL + Qdrant
    // ════════════════════════════════════════════════════════════════════════
    [HttpPost("craftsman/check-and-submit-solution")]
    public async Task<IActionResult> CheckAndSubmitSolution([FromBody] CraftsmanSolutionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ServiceType))
            return BadRequest(new { error = "التخصص مطلوب" });

        if (dto.Steps is null || dto.Steps.Count == 0)
            return BadRequest(new { error = "الخطوات مطلوبة" });

        if (dto.UserId <= 0)
            return BadRequest(new { error = "userId مطلوب" });

        string rawSteps = string.Join("\n", dto.Steps.Select((s, i) => $"{i + 1}. {s}"));
        string problemText = dto.ProblemDescription ?? dto.ServiceType;

        // ── 1. LLM يتحقق ويصلح في call واحدة ──────────────────────────────
        string prompt =
            $"أنت خبير في مجال {dto.ServiceType}.\n\n" +
            $"المشكلة: {problemText}\n\n" +
            $"الخطوات:\n{rawSteps}\n\n" +
            "المطلوب:\n" +
            "- لو الخطوات بتحل المشكلة فعلاً: صحح الأخطاء الإملائية بس من غير ما تغير المعنى أو الترتيب، وحط suitable: true\n" +
            "- لو الخطوات مش بتحل المشكلة: حط suitable: false وقول السبب\n\n" +
            "رد بـ JSON فقط بدون أي كلام:\n" +
            "{\"suitable\": true/false, \"reason\": \"سبب قصير لو مش مناسبة، فاضي لو مناسبة\", \"steps\": [\"...\"]}";

        bool suitable;
        string reason;
        List<string> fixedSteps;

        try
        {
            string raw = await _groq.CompleteAsync(prompt, maxTokens: 600);
            string json = raw.Replace("```json", "").Replace("```", "").Trim();
            using var doc = JsonDocument.Parse(json);

            suitable = doc.RootElement.GetProperty("suitable").GetBoolean();
            reason = doc.RootElement.GetProperty("reason").GetString() ?? "";
            fixedSteps = doc.RootElement
                .GetProperty("steps")
                .EnumerateArray()
                .Select(e => e.GetString() ?? "")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[CheckAndSubmit] LLM failed: {M}", ex.Message);
            return StatusCode(500, new { error = "فيه مشكلة في التحقق، حاول تاني" });
        }

        // ── 2. مش مناسبة → ارفض ────────────────────────────────────────────
        if (!suitable)
        {
            _logger.LogInformation("[CheckAndSubmit] Rejected — {Reason}", reason);
            return Ok(new CheckAndSubmitResultDto
            {
                Accepted = false,
                Message = $"الخطوات مش مناسبة لحل المشكلة دي 🙅 {reason}"
            });
        }

        // ── 3. مناسبة → لو الـ LLM مرجعش steps نستخدم الأصلية ─────────────
        if (fixedSteps.Count == 0)
            fixedSteps = dto.Steps;

        // ── 4. جيب بيانات الحرفي من الـ request ─────────────────────────────
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user is null)
            return BadRequest(new { error = "المستخدم مش موجود" });

        var craftsman = dto.CraftsmanId > 0
            ? await _db.Craftsmen.FirstOrDefaultAsync(c => c.Id == dto.CraftsmanId)
            : await _db.Craftsmen.FirstOrDefaultAsync(c => c.UserId == dto.UserId);

        if (craftsman is null)
            return BadRequest(new { error = "الحرفي مش موجود" });

        // ── 5. احفظ في SQL ───────────────────────────────────────────────────
        var (desc, prob, sol) = await PrepareRagFieldsAsync(
            dto.ServiceType,
            problemText,
            fixedSteps);

        var job = new Job
        {
            CustomerId = dto.UserId,
            CraftsmanId = craftsman.Id,
            Status = "مكتمل",
            ServiceType = dto.ServiceType,
            Description = desc,
            Address = "",
            ProblemImageUrl = "",
            ProblemDescription = prob,
            SolutionDescription = sol,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
        _logger.LogInformation("[CheckAndSubmit] ✓ Job saved Id={JobId}", job.Id);

        // ── 6. Ingest في Qdrant ───────────────────────────────────────────────
        int upserted = await _solution.IngestSingleJobSolutionAsync(job.Id);
        _logger.LogInformation("[CheckAndSubmit] ✓ Qdrant upserted={N}", upserted);

        return Ok(new CheckAndSubmitResultDto
        {
            Accepted = true,
            Message = "تم قبول الخطوات وحفظها بنجاح ✅",
            JobId = job.Id,
            Upserted = upserted,
            FixedSteps = fixedSteps
        });
    } 
}