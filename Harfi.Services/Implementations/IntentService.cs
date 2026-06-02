using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Harfi.DTOs.RAG;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harfi.Services.Implementations;

public class IntentService
{
    private readonly GroqRotatingClient _groqRotating;
    private readonly IConfiguration _config;
    private readonly ILogger<IntentService> _logger;

    public static readonly string[] ValidServices =
    [
        "كهربائي", "سباك", "نجار", "حداد", "دهان", "بناء",
        "تكييف وتبريد", "زجاج وألمنيوم", "سيراميك",
        "كاميرات مراقبة", "ديكور وجبس", "صيانة عامة"
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
        GroqRotatingClient groqRotating)
    {
        _groqRotating = groqRotating;
        _config = config;
        _logger = logger;
    }

    public async Task<LlmExtractionResult> ExtractAsync(
        List<ChatMsg> messages,
        string? knownService, string? knownCity, int? knownCount,
        int failedServiceAttempts, int failedCityAttempts, int failedCountAttempts)
    {
        string services = string.Join("، ", ValidServices);
        string cities = string.Join("، ", ValidCities);

        var contextParts = new List<string>();
        if (knownService is not null) contextParts.Add("التخصص المعروف حتى الآن: " + knownService);
        if (knownCity is not null) contextParts.Add("المحافظة المعروفة حتى الآن: " + knownCity);
        if (knownCount is not null) contextParts.Add("العدد المعروف حتى الآن: " + knownCount);
        if (failedServiceAttempts > 0) contextParts.Add("عدد محاولات فهم التخصص الفاشلة: " + failedServiceAttempts);
        if (failedCityAttempts > 0) contextParts.Add("عدد محاولات فهم المحافظة الفاشلة: " + failedCityAttempts);

        string knownContext = contextParts.Count > 0
            ? "السياق المعروف:\n" + string.Join("\n", contextParts) + "\n\n" : "";

        string systemPrompt =
            "أنت مساعد ذكي متخصص في استخراج معلومات طلب حرفي في مصر.\n\n" +
            "مهم جداً: اقرأ كل المحادثة من أولها وليس فقط آخر رسالة.\n" +
            "لو العميل ذكر التخصص أو المحافظة في أي رسالة سابقة، استخرجها منها.\n\n" +
            "قاعدة مهمة جداً:\n" +
            "- لو العميل طلب أكتر من تخصص في نفس الوقت → اختر التخصص الأول فقط\n" +
            "- لو العميل بعت عدد لأكتر من تخصص → خد العدد الأول فقط\n\n" +
            "مهمتك: استخراج 3 معلومات:\n" +
            "1. نوع التخصص المطلوب\n2. المحافظة المطلوبة\n3. عدد الحرفيين (بين 1 و10)\n\n" +
            "التخصصات المتاحة فقط:\n" + services + "\n\n" +
            "المحافظات المتاحة فقط:\n" + cities + "\n\n" +
            "قواعد استخراج التخصص:\n" +
            "- حنفية/مياه/تسريب/بالوعة/صرف/سخان/مرحاض/ماسورة → سباك\n" +
            "- كهرباء/سلك/لمبة/تماس/قاطع/فيشة/إنارة/سولار → كهربائي\n" +
            "- باب خشب/دولاب/باركيه/أثاث خشب/سرير مكسور → نجار\n" +
            "- باب حديد/بوابة/قضبان/سور حديد/لحام → حداد\n" +
            "- دهان/بوية/طلاء/بلاستر → دهان\n" +
            "- تشقق/ترميم/بناء/تشطيب/مقاول → بناء\n" +
            "- تكييف/مكيف/فريون → تكييف وتبريد\n" +
            "- زجاج/شباك ألمنيوم/مرآة/ألمنيوم → زجاج وألمنيوم\n" +
            "- سيراميك/بلاط/رخام/بورسلين → سيراميك\n" +
            "- كاميرا/مراقبة/CCTV/إنذار → كاميرات مراقبة\n" +
            "- جبس/ديكور/سقف جبسي/كورنيش → ديكور وجبس\n" +
            "- صيانة/أعطال متعددة → صيانة عامة\n\n" +
            "قواعد show_services_list:\n" +
            "- اكتب true فقط لو عدد محاولات فهم التخصص الفاشلة >= 3\n" +
            "- وإلا اكتب false\n\n" +
            "قواعد show_cities_list:\n" +
            "- اكتب true لو التخصص معروف والمحافظة ناقصة\n" +
            "- اكتب false في أي حالة تانية\n\n" +
            knownContext +
            "رد بـ JSON فقط:\n" +
            "{\n" +
            "  \"service_type\": \"التخصص أو null\",\n" +
            "  \"city\": \"المحافظة أو null\",\n" +
            "  \"count\": العدد_كرقم_أو_null,\n" +
            "  \"missing\": \"none | service | city | count | service_city | service_count | city_count | all\",\n" +
            "  \"question_to_ask\": \"السؤال للعميل أو null\",\n" +
            "  \"show_services_list\": true_أو_false,\n" +
            "  \"show_cities_list\": true_أو_false\n" +
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

            return ParseResult(raw, knownService, knownCity, knownCount,
                failedServiceAttempts, failedCityAttempts);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Intent] Exception: {M}", ex.Message);
            return BuildFallback(knownService, knownCity, knownCount,
                failedServiceAttempts, failedCityAttempts);
        }
    }

    private LlmExtractionResult ParseResult(
        string raw, string? knownService, string? knownCity, int? knownCount,
        int failedServiceAttempts, int failedCityAttempts)
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
            bool showServices = root.TryGetProperty("show_services_list", out var ssl) && ssl.GetBoolean();

            if (service is not null && !ValidServices.Contains(service)) service = knownService;
            if (city is not null && !ValidCities.Contains(city)) city = knownCity;

            string missing = CalcMissing(service, city, count);

            if (failedServiceAttempts >= 3 && service is null) showServices = true;
            if (service is not null) showServices = false;

            bool showCities = service is not null && city is null;

            return new LlmExtractionResult
            {
                ServiceType = service,
                City = city,
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