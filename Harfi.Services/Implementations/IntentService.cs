//using System.Net.Http.Json;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using Harfi.DTOs.RAG;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//namespace Harfi.Services.Implementations;

//public class IntentService
//{
//    private readonly GroqRotatingClient _groqRotating;
//    private readonly IConfiguration _config;
//    private readonly ILogger<IntentService> _logger;

//    public static readonly string[] ValidServices =
//    [
//        "كهربائي", "سباك", "نجار", "حداد", "دهان", "بناء",
//        "تكييف وتبريد", "زجاج وألمنيوم", "سيراميك",
//        "كاميرات مراقبة", "ديكور وجبس","ميكانيكي سيارات", "صيانة عامة"
//    ];

//    public static readonly string[] ValidCities =
//    [
//        "القاهرة", "الجيزة", "الإسكندرية", "المنصورة", "طنطا", "أسيوط",
//        "بورسعيد", "السويس", "الإسماعيلية", "أسوان", "الفيوم", "الزقازيق",
//        "المنوفية", "الغربية", "دمياط", "الشرقية", "كفر الشيخ", "الدقهلية",
//        "البحيرة", "المنيا", "بني سويف", "سوهاج", "قنا", "الأقصر",
//        "مطروح", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"
//    ];

//    public IntentService(
//        IConfiguration config,
//        ILogger<IntentService> logger,
//        GroqRotatingClient groqRotating)
//    {
//        _groqRotating = groqRotating;
//        _config = config;
//        _logger = logger;
//    }

//    public async Task<LlmExtractionResult> ExtractAsync(
//        List<ChatMsg> messages,
//        string? knownService, string? knownCity, int? knownCount,
//        int failedServiceAttempts, int failedCityAttempts, int failedCountAttempts)
//    {
//        string services = string.Join("، ", ValidServices);
//        string cities = string.Join("، ", ValidCities);

//        var contextParts = new List<string>();
//        if (knownService is not null) contextParts.Add("التخصص المعروف حتى الآن: " + knownService);
//        if (knownCity is not null) contextParts.Add("المحافظة المعروفة حتى الآن: " + knownCity);
//        if (knownCount is not null) contextParts.Add("العدد المعروف حتى الآن: " + knownCount);
//        if (failedServiceAttempts > 0) contextParts.Add("عدد محاولات فهم التخصص الفاشلة: " + failedServiceAttempts);
//        if (failedCityAttempts > 0) contextParts.Add("عدد محاولات فهم المحافظة الفاشلة: " + failedCityAttempts);

//        string knownContext = contextParts.Count > 0
//            ? "السياق المعروف:\n" + string.Join("\n", contextParts) + "\n\n" : "";

//        string systemPrompt =
//            "أنت مساعد ذكي متخصص في استخراج معلومات طلب حرفي في مصر.\n\n" +
//            "مهم جداً: اقرأ كل المحادثة من أولها وليس فقط آخر رسالة.\n" +
//            "لو العميل ذكر التخصص أو المحافظة في أي رسالة سابقة، استخرجها منها.\n\n" +
//            "قاعدة مهمة جداً:\n" +
//            "- لو العميل طلب أكتر من تخصص في نفس الوقت → اختر التخصص الأول فقط\n" +
//            "- لو العميل بعت عدد لأكتر من تخصص → خد العدد الأول فقط\n\n" +
//            "مهمتك: استخراج 3 معلومات:\n" +
//            "1. نوع التخصص المطلوب\n2. المحافظة المطلوبة\n3. عدد الحرفيين (بين 1 و10)\n\n" +
//            "التخصصات المتاحة فقط:\n" + services + "\n\n" +
//            "المحافظات المتاحة فقط:\n" + cities + "\n\n" +
//            "قواعد استخراج التخصص:\n" +
//            "- حنفية/مياه/تسريب/بالوعة/صرف/سخان/مرحاض/ماسورة → سباك\n" +
//            "- كهرباء/سلك/لمبة/تماس/قاطع/فيشة/إنارة/سولار → كهربائي\n" +
//            "- باب خشب/دولاب/باركيه/أثاث خشب/سرير مكسور → نجار\n" +
//            "- باب حديد/بوابة/قضبان/سور حديد/لحام → حداد\n" +
//            "- دهان/بوية/طلاء/بلاستر → دهان\n" +
//            "- تشقق/ترميم/بناء/تشطيب/مقاول → بناء\n" +
//            "- تكييف/مكيف/فريون → تكييف وتبريد\n" +
//            "- زجاج/شباك ألمنيوم/مرآة/ألمنيوم → زجاج وألمنيوم\n" +
//            "- سيراميك/بلاط/رخام/بورسلين → سيراميك\n" +
//            "- كاميرا/مراقبة/CCTV/إنذار → كاميرات مراقبة\n" +
//            "- جبس/ديكور/سقف جبسي/كورنيش → ديكور وجبس\n" +
//            "- صيانة/أعطال متعددة → صيانة عامة\n\n" +
//            "قواعد show_services_list:\n" +
//            "- اكتب true فقط لو عدد محاولات فهم التخصص الفاشلة >= 3\n" +
//            "- وإلا اكتب false\n\n" +
//            "قواعد show_cities_list:\n" +
//            "- اكتب true لو التخصص معروف والمحافظة ناقصة\n" +
//            "- اكتب false في أي حالة تانية\n\n" +
//            knownContext +
//            "رد بـ JSON فقط:\n" +
//            "{\n" +
//            "  \"service_type\": \"التخصص أو null\",\n" +
//            "  \"city\": \"المحافظة أو null\",\n" +
//            "  \"count\": العدد_كرقم_أو_null,\n" +
//            "  \"missing\": \"none | service | city | count | service_city | service_count | city_count | all\",\n" +
//            "  \"question_to_ask\": \"السؤال للعميل أو null\",\n" +
//            "  \"show_services_list\": true_أو_false,\n" +
//            "  \"show_cities_list\": true_أو_false\n" +
//            "}";

//        var llmMessages = new List<object> { new { role = "system", content = systemPrompt } };
//        foreach (var m in messages)
//            llmMessages.Add(new { role = m.Role, content = m.Content });

//        var payload = new
//        {
//            model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
//            temperature = 0.0,
//            max_tokens = 300,
//            messages = llmMessages
//        };

//        try
//        {
//            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
//            if (!resp.IsSuccessStatusCode)
//                return BuildFallback(knownService, knownCity, knownCount,
//                    failedServiceAttempts, failedCityAttempts);

//            var groqResult = await resp.Content.ReadFromJsonAsync<GroqResp>();
//            string raw = groqResult?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
//            _logger.LogInformation("[Intent] LLM raw: {R}", raw);

//            return ParseResult(raw, knownService, knownCity, knownCount,
//                failedServiceAttempts, failedCityAttempts);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogWarning("[Intent] Exception: {M}", ex.Message);
//            return BuildFallback(knownService, knownCity, knownCount,
//                failedServiceAttempts, failedCityAttempts);
//        }
//    }

//    //private LlmExtractionResult ParseResult(
//    //    string raw, string? knownService, string? knownCity, int? knownCount,
//    //    int failedServiceAttempts, int failedCityAttempts)
//    //{
//    //    try
//    //    {
//    //        string json = raw.Replace("```json", "").Replace("```", "").Trim();
//    //        using var doc = JsonDocument.Parse(json);
//    //        var root = doc.RootElement;

//    //        string? service = GetStr(root, "service_type") ?? knownService;
//    //        string? city = GetStr(root, "city") ?? knownCity;
//    //        int? count = null;

//    //        if (root.TryGetProperty("count", out var cProp))
//    //        {
//    //            if (cProp.ValueKind == JsonValueKind.Number)
//    //                count = Math.Clamp(cProp.GetInt32(), 1, 10);
//    //            else if (cProp.ValueKind == JsonValueKind.String)
//    //            {
//    //                var numStr = new string(cProp.GetString()!.Where(char.IsDigit).ToArray());
//    //                if (int.TryParse(numStr.Length > 0 ? numStr[..1] : "", out int n))
//    //                    count = Math.Clamp(n, 1, 10);
//    //            }
//    //        }
//    //        else count = knownCount;

//    //        string? question = GetStr(root, "question_to_ask");
//    //        bool showServices = root.TryGetProperty("show_services_list", out var ssl) && ssl.GetBoolean();

//    //        if (service is not null && !ValidServices.Contains(service)) service = knownService;
//    //        if (city is not null && !ValidCities.Contains(city)) city = knownCity;

//    //        string missing = CalcMissing(service, city, count);

//    //        if (failedServiceAttempts >= 3 && service is null) showServices = true;
//    //        if (service is not null) showServices = false;

//    //        bool showCities = service is not null && city is null;

//    //        return new LlmExtractionResult
//    //        {
//    //            ServiceType = service,
//    //            City = city,
//    //            Count = count,
//    //            Missing = missing,
//    //            QuestionToAsk = missing == "none" ? null : (question ?? DefaultQuestion(service, city, count)),
//    //            ShowServicesList = showServices,
//    //            ShowCitiesList = showCities
//    //        };
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        _logger.LogWarning("[Intent] Parse error: {M}", ex.Message);
//    //        return BuildFallback(knownService, knownCity, knownCount,
//    //            failedServiceAttempts, failedCityAttempts);
//    //    }
//    //}

//    private LlmExtractionResult ParseResult(
//    string raw, string? knownService, string? knownCity, int? knownCount,
//    int failedServiceAttempts, int failedCityAttempts)
//    {
//        try
//        {
//            string json = raw.Replace("```json", "").Replace("```", "").Trim();
//            using var doc = JsonDocument.Parse(json);
//            var root = doc.RootElement;

//            string? service = GetStr(root, "service_type") ?? knownService;
//            string? city = GetStr(root, "city") ?? knownCity;
//            int? count = null;

//            if (root.TryGetProperty("count", out var cProp))
//            {
//                if (cProp.ValueKind == JsonValueKind.Number)
//                    count = Math.Clamp(cProp.GetInt32(), 1, 10);
//                else if (cProp.ValueKind == JsonValueKind.String)
//                {
//                    var numStr = new string(cProp.GetString()!.Where(char.IsDigit).ToArray());
//                    if (int.TryParse(numStr.Length > 0 ? numStr[..1] : "", out int n))
//                        count = Math.Clamp(n, 1, 10);
//                }
//            }
//            else count = knownCount;

//            string? question = GetStr(root, "question_to_ask");
//            bool showServices = root.TryGetProperty("show_services_list", out var ssl) && ssl.GetBoolean();

//            // لو الـ LLM استخرج خدمة بس مش موجودة في قائمتنا → خدمة غير متاحة
//            string? rawService = GetStr(root, "service_type");
//            bool unknownService = rawService is not null && !ValidServices.Contains(rawService);

//            if (service is not null && !ValidServices.Contains(service)) service = knownService;
//            if (city is not null && !ValidCities.Contains(city)) city = knownCity;

//            // لو الخدمة غير معروفة ومش في قائمتنا → رد بسؤال يوضح المتاح
//            if (unknownService && knownService is null)
//            {
//                return new LlmExtractionResult
//                {
//                    ServiceType = null,
//                    City = city,
//                    Count = count,
//                    Missing = "service",
//                    QuestionToAsk = $"عذراً، خدمة \"{rawService}\" غير متاحة في منصة حرفي. 😊\nالخدمات المتاحة هي: {string.Join("، ", ValidServices)}",
//                    ShowServicesList = true,
//                    ShowCitiesList = false
//                };
//            }

//            string missing = CalcMissing(service, city, count);

//            if (failedServiceAttempts >= 1 && service is null) showServices = true;
//            if (service is not null) showServices = false;

//            bool showCities = service is not null && city is null;

//            return new LlmExtractionResult
//            {
//                ServiceType = service,
//                City = city,
//                Count = count,
//                Missing = missing,
//                QuestionToAsk = missing == "none" ? null : (question ?? DefaultQuestion(service, city, count)),
//                ShowServicesList = showServices,
//                ShowCitiesList = showCities
//            };
//        }
//        catch (Exception ex)
//        {
//            _logger.LogWarning("[Intent] Parse error: {M}", ex.Message);
//            return BuildFallback(knownService, knownCity, knownCount,
//                failedServiceAttempts, failedCityAttempts);
//        }
//    }
//    public async Task<IntentAnalysis> AnalyzeAsync(List<ConversationMessage> messages)
//    {
//        var chatMsgs = messages
//            .Select(m => new ChatMsg { Role = m.Role, Content = m.Content })
//            .ToList();

//        var result = await ExtractAsync(chatMsgs, null, null, null, 0, 0, 0);

//        return new IntentAnalysis
//        {
//            IsComplete = result.Missing == "none",
//            ServiceType = result.ServiceType,
//            City = result.City,
//            CleanProblem = messages.LastOrDefault(m => m.Role == "user")?.Content,
//            QuestionToAsk = result.QuestionToAsk
//        };
//    }

//    private static string CalcMissing(string? service, string? city, int? count)
//    {
//        bool ms = service is null, mc = city is null, mn = count is null;
//        if (!ms && !mc && !mn) return "none";
//        if (ms && mc && mn) return "all";
//        if (ms && mc) return "service_city";
//        if (ms && mn) return "service_count";
//        if (mc && mn) return "city_count";
//        if (ms) return "service";
//        if (mc) return "city";
//        return "count";
//    }

//    private static string DefaultQuestion(string? service, string? city, int? count)
//    {
//        if (service is null) return "إيه المشكلة اللي عندك؟ وضّحلي أكتر 😊";
//        if (city is null) return "في أنهي محافظة عاوز " + service + "؟";
//        if (count is null) return "كويس! كام حرفي عاوز أجيبلك؟";
//        return "وضّحلي أكتر من فضلك.";
//    }

//    private static LlmExtractionResult BuildFallback(
//        string? knownService, string? knownCity, int? knownCount,
//        int failedService, int failedCity)
//    {
//        string missing = CalcMissing(knownService, knownCity, knownCount);
//        bool showCities = knownService is not null && knownCity is null;

//        return new LlmExtractionResult
//        {
//            ServiceType = knownService,
//            City = knownCity,
//            Count = knownCount,
//            Missing = missing,
//            QuestionToAsk = DefaultQuestion(knownService, knownCity, knownCount),
//            ShowServicesList = failedService >= 3 && knownService is null,
//            ShowCitiesList = showCities
//        };
//    }

//    private static string? GetStr(JsonElement root, string key)
//    {
//        if (!root.TryGetProperty(key, out var el)) return null;
//        if (el.ValueKind == JsonValueKind.Null) return null;
//        return el.GetString();
//    }

//    private class GroqResp
//    {
//        [JsonPropertyName("choices")] public List<GroqChoice> Choices { get; set; } = [];
//    }
//    private class GroqChoice
//    {
//        [JsonPropertyName("message")] public GroqMsg Message { get; set; } = new();
//    }
//    private class GroqMsg
//    {
//        [JsonPropertyName("content")] public string Content { get; set; } = "";
//    }
//}
using Harfi.DTOs.RAG;
using Harfi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Harfi.Services.Implementations;

public class IntentService
{
    private readonly GroqRotatingClient _groqRotating;
    private readonly IConfiguration _config;
    private readonly ILogger<IntentService> _logger;
    private readonly Harfi.Repositories.Data.AppDbContext _db;

    public static readonly string[] ValidServices =
    [
        "كهربائي", "سباك", "نجار", "حداد", "دهان", "بناء",
        "تكييف وتبريد", "زجاج وألمنيوم", "سيراميك",
        "كاميرات مراقبة", "ديكور وجبس","ميكانيكي سيارات", "صيانة عامة"
    ];

    public static readonly string[] ValidCities =
    [
        "القاهرة", "الجيزة", "الإسكندرية", "المنصورة", "طنطا", "أسيوط",
        "بورسعيد", "السويس", "الإسماعيلية", "أسوان", "الفيوم", "الزقازيق",
        "المنوفية", "الغربية", "دمياط", "الشرقية", "كفر الشيخ", "الدقهلية",
        "البحيرة", "المنيا", "بني سويف", "سوهاج", "قنا", "الأقصر",
        "مطروح", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"
    ];

    public IntentService(
        IConfiguration config,
        ILogger<IntentService> logger,
        GroqRotatingClient groqRotating,
        Harfi.Repositories.Data.AppDbContext db)
    {
        _db = db;
        _groqRotating = groqRotating;
        _config = config;
        _logger = logger;
    }

    public async Task<LlmExtractionResult> ExtractAsync(
        List<ChatMsg> messages,
        string? knownService, string? knownCity, int? knownCount,
        int failedServiceAttempts, int failedCityAttempts, int failedCountAttempts)
    {
        var availableServices = await _db.Craftsmen
            .Where(c => !c.IsDeleted && c.IsApproved && c.IsAvailable && c.ServiceType != "AI")
            .Select(c => c.ServiceType)
            .Distinct()
            .OrderBy(s => s)
            .ToArrayAsync();

        string services = string.Join("، ", availableServices);
        string cities = string.Join("، ", ValidCities);

        var contextParts = new List<string>();
        if (knownService is not null) contextParts.Add("التخصص المعروف حتى الآن: " + knownService);
        if (knownCity is not null) contextParts.Add("المحافظة المعروفة حتى الآن: " + knownCity);
        if (knownCount is not null) contextParts.Add("العدد المعروف حتى الآن: " + knownCount);
        if (failedServiceAttempts > 0) contextParts.Add("عدد محاولات فهم التخصص الفاشلة: " + failedServiceAttempts);
        if (failedCityAttempts > 0) contextParts.Add("عدد محاولات فهم المحافظة الفاشلة: " + failedCityAttempts);

        string knownContext = contextParts.Count > 0
            ? "السياق المعروف:\n" + string.Join("\n", contextParts) + "\n\n" : "";

        //        string systemPrompt =
        //            "أنت مساعد ذكي متخصص في استخراج معلومات طلب حرفي في مصر.\n\n" +
        //            "مهم جداً: اقرأ كل المحادثة من أولها وليس فقط آخر رسالة.\n" +
        //            "لو العميل ذكر التخصص أو المحافظة في أي رسالة سابقة، استخرجها منها.\n\n" +
        //            "قاعدة مهمة جداً:\n" +
        //            "- لو العميل طلب أكتر من تخصص في نفس الوقت → اختر التخصص الأول فقط\n" +
        //            "- لو العميل بعت عدد لأكتر من تخصص → خد العدد الأول فقط\n\n" +
        //            "مهمتك: استخراج 3 معلومات:\n" +
        //            "1. نوع التخصص المطلوب\n2. المحافظة المطلوبة\n3. عدد الحرفيين (بين 1 و10)\n\n" +
        //            "التخصصات المتاحة فقط:\n" + services + "\n\n" +
        //            "المحافظات المتاحة فقط:\n" + cities + "\n\n" +
        //            "قواعد استخراج التخصص:\n" +
        //            "- لو المستخدم ذكر تخصصاً موجوداً في القائمة أو مشكلة تنتمي لتخصص فيها → اكتبه بنفس الاسم في القائمة حرفاً بحرف\n" +
        //"- لو المستخدم ذكر مهنة أو شيء مش له علاقة بأي تخصص في القائمة → اكتب ما قاله كما هو في service_type\n" +
        //"- لا تخمن ولا تقرب مهن مختلفة ببعض — جزار ≠ نجار، طيار ≠ أي تخصص\n" +
        //"- أمثلة صح: نجارين/نجار/باب خشب/أثاث → نجارة | حنفية/مياه/تسريب → سباكة | كهرباء/لمبة → كهرباء\n" +
        //"- أمثلة غلط: جزار/طيار/طباخ → اكتبهم كما هم لأنهم مش في القائمة\n\n" +
        //            "- استخرج التخصص من كلام المستخدم وطابقه بأقرب تخصص من القائمة\n" +
        //"- اكتب التخصص بنفس الاسم الموجود في القائمة بالضبط\n" +
        //"- لو مفيش تخصص مناسب في القائمة اكتب ما قاله المستخدم\n" +
        //            "- حنفية/مياه/تسريب/بالوعة/صرف/سخان/مرحاض/ماسورة → سباك\n" +
        //            "- كهرباء/سلك/لمبة/تماس/قاطع/فيشة/إنارة/سولار → كهربائي\n" +
        //            "- باب خشب/دولاب/باركيه/أثاث خشب/سرير مكسور → نجار\n" +
        //            "- باب حديد/بوابة/قضبان/سور حديد/لحام → حداد\n" +
        //            "- دهان/بوية/طلاء/بلاستر → دهان\n" +
        //            "- تشقق/ترميم/بناء/تشطيب/مقاول → بناء\n" +
        //            "- تكييف/مكيف/فريون → تكييف وتبريد\n" +
        //            "- زجاج/شباك ألمنيوم/مرآة/ألمنيوم → زجاج وألمنيوم\n" +
        //            "- سيراميك/بلاط/رخام/بورسلين → سيراميك\n" +
        //            "- كاميرا/مراقبة/CCTV/إنذار → كاميرات مراقبة\n" +
        //            "- جبس/ديكور/سقف جبسي/كورنيش → ديكور وجبس\n" +
        //            "- صيانة/أعطال متعددة → صيانة عامة\n\n" +
        //            "قواعد show_services_list:\n" +
        //            "- اكتب true فقط لو عدد محاولات فهم التخصص الفاشلة >= 3\n" +
        //            "- وإلا اكتب false\n\n" +
        //            "قواعد show_cities_list:\n" +
        //            "- اكتب true لو التخصص معروف والمحافظة ناقصة\n" +
        //            "- اكتب false في أي حالة تانية\n\n" +
        //            knownContext +
        //            "رد بـ JSON فقط:\n" +
        //            "{\n" +
        //            "  \"service_type\": \"التخصص أو null\",\n" +
        //            "  \"city\": \"المحافظة أو null\",\n" +
        //            "  \"count\": العدد_كرقم_أو_null,\n" +
        //            "  \"missing\": \"none | service | city | count | service_city | service_count | city_count | all\",\n" +
        //            "  \"question_to_ask\": \"السؤال للعميل أو null\",\n" +
        //            "  \"show_services_list\": true_أو_false,\n" +
        //            "  \"show_cities_list\": true_أو_false\n" +
        //            "}";
        //        string systemPrompt =
        //            "أنت مساعد ذكي متخصص في استخراج معلومات طلب حرفي في مصر.\n\n" +

        //            "مهمتك: استخراج 3 معلومات من المحادثة:\n" +
        //            "1. نوع التخصص المطلوب\n" +
        //            "2. المحافظة المطلوبة\n" +
        //            "3. عدد الحرفيين (بين 1 و10)\n\n" + 

        //            //"قواعد القراءة:\n" +
        //            //"- اقرأ كل المحادثة من أولها وليس فقط آخر رسالة\n" +
        //            //"- لو العميل ذكر التخصص أو المحافظة في أي رسالة سابقة استخرجها\n" +
        //            //"- لو طلب أكتر من تخصص اختر الأول فقط\n\n" +


        //            "قواعد القراءة:\n" +
        //"- ركز على آخر رسالة للمستخدم أولاً\n" +
        //"-  لو آخر رسالة فيها تخصص أو مشكلة جديدة → استخدمها وتجاهل التخصص القديم \n " +
        //"- لو آخر رسالة مش فيها تخصص واضح → ارجع للمحادثة السابقة\n" +
        //"- لو طلب أكتر من تخصص اختر الأول فقط\n\n" +


        //" استخرج التخصص من آخر رسالة للمستخدم أولاً\n" +
        //" لو آخر رسالة فيها تخصص أو مشكلة تنتمي لتخصص → استخدمه حتى لو مختلف عن أي تخصص قبله في المحادثة\n" +

        //            //"التخصصات المتاحة:\n" + services + "\n\n" +
        //            "التخصصات المتاحة في المنصة:\n" + services + "\n\n" +
        //"مهم جداً: لو المستخدم طلب أي تخصص غير موجود في القائمة، اكتبه كما قاله في service_type ولا تكتب null أبداً\n\n" +
        //            "المحافظات المتاحة:\n" + cities + "\n\n" +

        //            "قواعد استخراج التخصص:\n" +
        //            "- افهم مشكلة المستخدم واختر التخصص المناسب من القائمة بالاسم الحرفي الموجود فيها\n" +

        //            "- لو المستخدم ذكر اسم تخصص موجود في القائمة أو مرادفاً له أو مشكلة تنتمي له → اكتبه بنفس الاسم في القائمة حرفاً بحرف\n" +
        //            "- لو المستخدم ذكر مهنة أو شيء لا علاقة له بأي تخصص في القائمة → اكتب ما قاله كما هو\n" +
        //            "- لا تخمن ولا تقرب مهن مختلفة — جزار ≠ نجار | طيار ≠ أي تخصص | طباخ ≠ أي تخصص\n\n" +
        //            "- اختار اخر تخصص هو ذكره ف الشات \n" +

        //            "- لو اخر رساره كان قصده بيها تخصص ومش موجود اعرضلة قائمه التخصصات \n" +

        //            "أمثلة صحيحة:\n" +
        //            "- نجارين / نجار / باب خشب / أثاث / دولاب / باركيه → نجارة\n" +
        //            "- حنفية / مياه / تسريب / بالوعة / صرف / سخان / مرحاض → سباكة\n" +
        //            "- كهرباء / لمبة / سلك / تماس / قاطع / فيشة / إنارة / سولار → كهرباء\n" +
        //            "- باب حديد / بوابة / قضبان / سور حديد / درابزين / لحام → حدادة\n" +
        //            "- دهان / بوية / طلاء / بلاستر / نقاشة → دهانات\n" +
        //            "- تكييف / مكيف / فريون / تبريد → تكييف وتبريد\n" +
        //            "- سيراميك / بلاط / رخام / بورسلين / تبليط → تبليط وسيراميك\n" +
        //            "- جبس / أسقف / ديكور / كورنيش / سقف جبسي → جبس وأسقف\n" +
        //            "- زجاج / شباك زجاج / مرايا → زجاج ومرايا\n" +
        //            "- ألمنيوم / شباك ألمنيوم / كلادينج → ألمنيوم\n" +
        //            "- كاميرا / مراقبة / CCTV / إنذار / إنتركوم / أمن → أمن وكاميرات\n" +
        //            "- حشرات / رش / تعقيم → مكافحة حشرات\n" +
        //            "- تشقق / ترميم / بناء / تشطيب / مقاول → بناء\n\n" +
        //            "-  لو هو كتب رساله مشكله  في الاخر في تخصص غير التخصص او التخصصات المكتوبه فوق شوف التخخ بتاع المشكله دي موجود والا لا لو مش موجود اعرض قائمه الحرف المتاحه لو موجود اختار الحرفه دي \n\n" +
        //            "-  لو اخر رساله تخصص مختلف مش موجود عندي اعرضله التخصصات الموجوده \n" +

        //            "أمثلة خاطئة يجب تجنبها:\n" +
        //            "- جزار → اكتب 'جزار' كما هو (مش نجار)\n" +
        //            "- طيار → اكتب 'طيار' كما هو\n" +
        //            "- طباخ → اكتب 'طباخ' كما هو\n\n" +

        //            "قواعد show_services_list:\n" +
        //            "- اكتب true لو عدد محاولات فهم التخصص الفاشلة >= 2\n" +
        //            "- وإلا اكتب false\n\n" +

        //            "قواعد show_cities_list:\n" +
        //            "- اكتب true لو التخصص معروف والمحافظة ناقصة\n" +
        //            "- اكتب false في أي حالة تانية\n\n" +

        //            knownContext +

        //            "رد بـ JSON فقط:\n" +
        //            "{\n" +
        //            "  \"service_type\": \"التخصص بنفس اسمه في القائمة أو ما قاله المستخدم أو null\",\n" +
        //            "  \"city\": \"المحافظة أو null\",\n" +
        //            "  \"count\": العدد_كرقم_أو_null,\n" +
        //            "  \"missing\": \"none | service | city | count | service_city | service_count | city_count | all\",\n" +
        //            "  \"question_to_ask\": \"السؤال للعميل أو null\",\n" +
        //            "  \"show_services_list\": true_أو_false,\n" +
        //            "  \"show_cities_list\": true_أو_false\n" +
        //            "}";

        string systemPrompt =
    "أنت مساعد ذكي متخصص في استخراج معلومات طلب حرفي في مصر.\n\n" +

    "مهمتك: استخراج 3 معلومات من المحادثة:\n" +
    "1. نوع التخصص المطلوب\n" +
    "2. المحافظة المطلوبة\n" +
    "3. عدد الحرفيين (بين 1 و10)\n\n" +

    "═══ قواعد تحديد التخصص — اتبعها بالترتيب ═══\n" +
    "الخطوة 1: انظر لآخر رسالة للمستخدم\n" +
    "  - لو فيها تخصص واضح أو مشكلة تنتمي لتخصص → استخدمه فوراً بغض النظر عن أي تخصص قبله\n" +
    "  - لو فيها رد قصير فقط (لا / أيوه / تمام / لسه / مش اتحلت / موجودة) → انتقل للخطوة 2\n" +
    "الخطوة 2: لو آخر رسالة مش فيها تخصص → استخدم التخصص الموجود في السياق السابق للمحادثة\n\n" +

    "أمثلة على الخطوة 1:\n" +
    "- المحادثة كانت عن سباكة + آخر رسالة 'عندي مشكله في الكهرباء' → التخصص = كهرباء\n" +
    "- المحادثة كانت عن نجارة + آخر رسالة 'الماتور مش بيشتغل' → التخصص = ميكانيكي سيارات\n" +
    "- المحادثة كانت عن سباكة + آخر رسالة 'لا، لسه موجودة' → التخصص = سباكة (رد قصير)\n" +
    "- المحادثة كانت عن كهرباء + آخر رسالة 'أيوه اتحلت' → التخصص = كهرباء (رد قصير)\n\n" +

    "═══ قواعد مطابقة التخصص بالقائمة ═══\n" +
    "التخصصات المتاحة في المنصة:\n" + services + "\n\n" +
    "- لو التخصص موجود في القائمة أو مرادف له → اكتبه بنفس الاسم الحرفي في القائمة\n" +
    "- لو التخصص غير موجود في القائمة → اكتبه كما قاله المستخدم، لا تكتب null أبداً\n" +
    "- لا تقرّب مهن مختلفة: جزار ≠ نجار | طيار ≠ أي تخصص | جزار ≠ أي تخصص\n\n" +

    "أمثلة مطابقة صحيحة:\n" +
    "- حنفية / مياه / تسريب / بالوعة / صرف / سخان / مرحاض / ماسورة → سباكة\n" +
    "- كهرباء / لمبة / سلك / تماس / قاطع / فيشة / إنارة / سولار → كهرباء\n" +
    "- باب خشب / دولاب / باركيه / أثاث / سرير مكسور → نجارة\n" +
    "- باب حديد / بوابة / قضبان / سور / درابزين / لحام → حدادة\n" +
    "- دهان / بوية / طلاء / بلاستر / نقاشة → دهانات\n" +
    "- تكييف / مكيف / فريون / تبريد → تكييف وتبريد\n" +
    "- سيراميك / بلاط / رخام / بورسلين / تبليط → تبليط وسيراميك\n" +
    "- جبس / أسقف / ديكور / كورنيش / سقف جبسي → جبس وأسقف\n" +
    "- زجاج / شباك زجاج / مرايا → زجاج ومرايا\n" +
    "- ألمنيوم / شباك ألمنيوم / كلادينج → ألمنيوم\n" +
    "- كاميرا / مراقبة / CCTV / إنذار / إنتركوم / أمن → أمن وكاميرات\n" +
    "- حشرات / رش / تعقيم → مكافحة حشرات\n" +
    "- تشقق / ترميم / بناء / تشطيب / مقاول → بناء\n" +
    "- عربية / سيارة / ماتور سيارة / إطار / كاوتش → ميكانيكي سيارات\n\n" +

    "═══ المحافظات المتاحة ═══\n" +
    cities + "\n\n" +

    "═══ قواعد show_services_list ═══\n" +
    "- اكتب true لو التخصص المستخرج غير موجود في قائمة التخصصات المتاحة\n" +
    "- اكتب true لو عدد محاولات فهم التخصص الفاشلة >= 2\n" +
    "- اكتب false في أي حالة تانية\n\n" +

    "═══ قواعد show_cities_list ═══\n" +
    "- اكتب true لو التخصص معروف والمحافظة ناقصة\n" +
    "- اكتب false في أي حالة تانية\n\n" +

    knownContext +

    "═══ قواعد استخراج الحي والشارع ═══\n" +
"- لو المستخدم ذكر حي أو شارع أو مدينة فرعية → استخرجه في district\n" +
"- أمثلة: مدينة نصر، شارع عباس العقاد، المعادي، الزمالك، شبرا، فيصل\n" +
"- لو مش موجود → اكتب null\n\n" +

    "رد بـ JSON فقط:\n" +
    "{\n" +
    "  \"service_type\": \"التخصص بنفس اسمه في القائمة أو ما قاله المستخدم أو null\",\n" +
    "  \"city\": \"المحافظة أو null\",\n" +
    "  \"count\": العدد_كرقم_أو_null,\n" +
    "  \"missing\": \"none | service | city | count | service_city | service_count | city_count | all\",\n" +
    "  \"question_to_ask\": \"السؤال للعميل أو null\",\n" +
    "  \"show_services_list\": true_أو_false,\n" +
    "  \"show_cities_list\": true_أو_false\n" +
    "  \"district\": \"الحي أو الشارع أو null\"\n" +
    "}";

        var llmMessages = new List<object> { new { role = "system", content = systemPrompt } };
        foreach (var m in messages)
            llmMessages.Add(new { role = m.Role, content = m.Content });

        var payload = new
        {
            model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
            temperature = 0.0,
            max_tokens = 300,
            messages = llmMessages
        };

        try
        {
            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
            if (!resp.IsSuccessStatusCode)
                return BuildFallback(knownService, knownCity, knownCount,
                    failedServiceAttempts, failedCityAttempts);

            var groqResult = await resp.Content.ReadFromJsonAsync<GroqResp>();
            string raw = groqResult?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            _logger.LogInformation("[Intent] LLM raw: {R}", raw);

            //return ParseResult(raw, knownService, knownCity, knownCount,
            //    failedServiceAttempts, failedCityAttempts, availableServices);
            return ParseResult(raw, knownService, knownCity, knownCount,
    failedServiceAttempts, failedCityAttempts, availableServices, messages);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Intent] Exception: {M}", ex.Message);
            return BuildFallback(knownService, knownCity, knownCount,
                failedServiceAttempts, failedCityAttempts);
        }
    }



    //private LlmExtractionResult ParseResult(
    //string raw, string? knownService, string? knownCity, int? knownCount,
    //int failedServiceAttempts, int failedCityAttempts, string[] availableServices)
    private LlmExtractionResult ParseResult(
    string raw, string? knownService, string? knownCity, int? knownCount,
    int failedServiceAttempts, int failedCityAttempts, string[] availableServices,
    List<ChatMsg> messages)
    {
        try
        {
            string json = raw.Replace("```json", "").Replace("```", "").Trim();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string? service = GetStr(root, "service_type") ?? knownService;
            string? city = GetStr(root, "city") ?? knownCity;
            int? count = null;

            if (root.TryGetProperty("count", out var cProp))
            {
                if (cProp.ValueKind == JsonValueKind.Number)
                    count = Math.Clamp(cProp.GetInt32(), 1, 10);
                else if (cProp.ValueKind == JsonValueKind.String)
                {
                    var numStr = new string(cProp.GetString()!.Where(char.IsDigit).ToArray());
                    if (int.TryParse(numStr.Length > 0 ? numStr[..1] : "", out int n))
                        count = Math.Clamp(n, 1, 10);
                }
            }
            else count = knownCount;

            string? question = GetStr(root, "question_to_ask");
            string? district = GetStr(root, "district");

            //bool showServices = root.TryGetProperty("show_services_list", out var ssl) && ssl.GetBoolean();

            bool showServices = false;
if (root.TryGetProperty("show_services_list", out var ssl))
{
    if (ssl.ValueKind == JsonValueKind.True) showServices = true;
    else if (ssl.ValueKind == JsonValueKind.String)
        showServices = ssl.GetString()?.ToLower() == "true";
}
            // لو الـ LLM استخرج خدمة بس مش موجودة في قائمتنا → خدمة غير متاحة
            string? rawService = GetStr(root, "service_type");

            if (service is not null && !availableServices.Contains(service)) service = knownService;
            if (city is not null && !ValidCities.Contains(city)) city = knownCity;

            bool unknownService = rawService is not null && !availableServices.Contains(rawService);
            string lastUserMsg = messages
    .LastOrDefault(m => m.Role == "user")?.Content ?? "";

            bool wantsSteps =
                lastUserMsg.Contains("خطوات") || lastUserMsg.Contains("خطوه") ||
                lastUserMsg.Contains("كيف") || lastUserMsg.Contains("اعمل") ||
                lastUserMsg.Contains("اصلح") || lastUserMsg.Contains("أصلح") ||
                lastUserMsg.Contains("عاوز خطوات حل مشكله") ||
                lastUserMsg.Contains("عاوز خطوات حل مشكلة");
            //        bool wantsSteps = messages.Any(m => m.Role == "user" && (
            //m.Content.Contains("خطوات") || m.Content.Contains("خطوه") ||
            //m.Content.Contains("كيف") || m.Content.Contains("اعمل") ||
            //m.Content.Contains("اصلح") || m.Content.Contains("أصلح")|| m.Content.Contains("عاوز خطوات حل مشكله") || m.Content.Contains("عاوز خطوات حل مشكلة")));
           
            if (unknownService && knownService is null&& !wantsSteps)
            {
                return new LlmExtractionResult
                {
                    ServiceType = null,
                    City = city,
                    Count = count,
                    Missing = "service",
                    QuestionToAsk = $"عذراً، خدمة \"{rawService}\" غير متاحة في منصة حرفي. 😊\nالخدمات المتاحة هي: {string.Join("، ", availableServices)}",
                    ShowServicesList = true,
                    ShowCitiesList = false
                };
            }
          
 
            string missing = CalcMissing(service, city, count);

            if (failedServiceAttempts >= 1 && service is null) showServices = true;
            if (service is not null) showServices = false;

            //bool showCities = service is not null && city is null;
            bool showCities = service is not null && city is null;
            if (root.TryGetProperty("show_cities_list", out var scl))
            {
                if (scl.ValueKind == JsonValueKind.True) showCities = true;
                else if (scl.ValueKind == JsonValueKind.String && scl.GetString()?.ToLower() == "true")
                    showCities = true;
            }

            return new LlmExtractionResult
            {
                ServiceType = service,
                City = city,
                District = district,  // ← أضف
                Count = count,
                Missing = missing,
                QuestionToAsk = missing == "none" ? null : (question ?? DefaultQuestion(service, city, count)),
                ShowServicesList = showServices,
                ShowCitiesList = showCities
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Intent] Parse error: {M}", ex.Message);
            return BuildFallback(knownService, knownCity, knownCount,
                failedServiceAttempts, failedCityAttempts);
        }
    }
    public async Task<IntentAnalysis> AnalyzeAsync(List<ConversationMessage> messages)
    {
        var chatMsgs = messages
            .Select(m => new ChatMsg { Role = m.Role, Content = m.Content })
            .ToList();

        var result = await ExtractAsync(chatMsgs, null, null, null, 0, 0, 0);

        return new IntentAnalysis
        {
            IsComplete = result.Missing == "none",
            ServiceType = result.ServiceType,
            City = result.City,
            CleanProblem = messages.LastOrDefault(m => m.Role == "user")?.Content,
            QuestionToAsk = result.QuestionToAsk
        };
    }

    private static string CalcMissing(string? service, string? city, int? count)
    {
        bool ms = service is null, mc = city is null, mn = count is null;
        if (!ms && !mc && !mn) return "none";
        if (ms && mc && mn) return "all";
        if (ms && mc) return "service_city";
        if (ms && mn) return "service_count";
        if (mc && mn) return "city_count";
        if (ms) return "service";
        if (mc) return "city";
        return "count";
    }

    private static string DefaultQuestion(string? service, string? city, int? count)
    {
        if (service is null) return "إيه المشكلة اللي عندك؟ وضّحلي أكتر 😊";
        if (city is null) return "في أنهي محافظة عاوز " + service + "؟";
        if (count is null) return "كويس! كام حرفي عاوز أجيبلك؟";
        return "وضّحلي أكتر من فضلك.";
    }

    private static LlmExtractionResult BuildFallback(
        string? knownService, string? knownCity, int? knownCount,
        int failedService, int failedCity)
    {
        string missing = CalcMissing(knownService, knownCity, knownCount);
        bool showCities = knownService is not null && knownCity is null;

        return new LlmExtractionResult
        {
            ServiceType = knownService,
            City = knownCity,
            Count = knownCount,
            Missing = missing,
            QuestionToAsk = DefaultQuestion(knownService, knownCity, knownCount),
            ShowServicesList = failedService >= 3 && knownService is null,
            ShowCitiesList = showCities
        };
    }

    private static string? GetStr(JsonElement root, string key)
    {
        if (!root.TryGetProperty(key, out var el)) return null;
        if (el.ValueKind == JsonValueKind.Null) return null;
        return el.GetString();
    }

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