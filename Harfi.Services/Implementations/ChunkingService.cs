using Harfi.DTOs.RAG;
using Harfi.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Harfi.Services.Implementations;

public class ChunkingService
{
    private static readonly Dictionary<string, string[]> ServiceProblems = new()
    {
        ["كهربائي"] = ["انقطاع الكهرباء", "سلك محترق", "لمبة مش بتشتغل", "فيشة معطلة", "مقبس خربان", "لوحة الكهرباء بتفصل", "قاطع بيطلع لوحده", "تماس كهربائي", "مفتاح معطل", "تأسيس كهرباء", "طاقة شمسية", "سولار", "انفرتر", "عداد كهرباء"],
        ["سباك"] = ["حنفية مكسورة", "حنفية بتقطر", "صنبور بيسرب", "تسريب مياه", "مواسير مكسورة", "بالوعة مسدودة", "صرف صحي مسدود", "سخان مياه معطل", "مرحاض بيسرب", "سيفون مكسور", "خزان بيسرب", "ضغط المياه ضعيف", "طلمبة مياه خربانة"],
        ["نجار"] = ["باب خشب مكسور", "باب خشب مش بيقفل", "باب بيعلق", "شباك خشب مكسور", "دولاب خربان", "أثاث خشبي مكسور", "سرير مكسور", "باركيه مكسور", "رف خشبي ساقط", "باب بيصدر صوت"],
        ["حداد"] = ["باب حديد مكسور", "باب حديد بيصدأ", "بوابة حديدية مكسورة", "شباك حديد مكسور", "قضبان مكسورة", "سور حديدي مكسور", "درابزين حديدي مكسور", "لحام حديد", "باب الجراج مكسور"],
        ["دهان"] = ["دهان جدران", "بوية قشرت", "جدار متبقع", "سقف مصفر", "بقع رطوبة على الجدار", "بلاستر جديد", "دهان واجهة المبنى"],
        ["بناء"] = ["تشقق في الجدار", "سقف متشقق", "رطوبة في الجدار", "ترميم شقة", "تشطيب شقة جديدة", "بناء غرفة إضافية", "مقاول بناء"],
        ["تكييف وتبريد"] = ["تكييف مش بيبرد", "مكيف خربان", "تكييف بيقطر مياه", "تكييف بيعمل صوت", "فريون ناقص", "تنظيف فلاتر التكييف"],
        ["زجاج وألمنيوم"] = ["زجاج مكسور", "شباك زجاج مكسور", "شباك ألمنيوم مكسور", "شباك مش بيقفل", "دش زجاجي مكسور", "مرآة مكسورة"],
        ["سيراميك"] = ["سيراميك مكسور", "بلاطة مكسورة", "سيراميك مرفوع", "بلاط بيصدر صوت فراغ", "رخام مكسور", "بورسلين مكسور"],
        ["كاميرات مراقبة"] = ["كاميرا مراقبة خربانة", "كاميرا مش بتشتغل", "كاميرا مش بتسجل", "CCTV", "إنذار سرقة", "بصمة إصبع"],
        ["ديكور وجبس"] = ["سقف جبس مكسور", "جبس بورد خربان", "كورنيش جبسي مكسور", "سقف جبسي بيتشقق", "شرائط LED", "قواطع جبس بورد"],
        ["صيانة عامة"] = ["صيانة منزل", "إصلاح عطل", "خراب في البيت", "أعطال متعددة", "صيانة شاملة"]
    };

    private readonly ILogger<ChunkingService> _logger;
    public ChunkingService(ILogger<ChunkingService> logger) => _logger = logger;

    //public List<CraftsmanChunk> ChunkCraftsman(Craftsman craftsman)
    //{
    //    string problems = ServiceProblems.TryGetValue(craftsman.ServiceType, out var list)
    //        ? string.Join("، ", list) : craftsman.ServiceType;

    //    string name = craftsman.User?.Name ?? $"حرفي #{craftsman.Id}";
    //    string bio = craftsman.Bio ?? "";

    //    string fullText =
    //        $"الحرفي: {name}\n" +
    //        $"التخصص: {craftsman.ServiceType}\n" +
    //        $"المدينة: {craftsman.City}\n" +
    //        $"الخبرة: {craftsman.Experience} سنة | التقييم: {craftsman.Rating}/5.0\n" +
    //        $"يحل مشاكل مثل: {problems}\n" +
    //        $"نبذة: {bio}";

    //    _logger.LogInformation("Chunked [{Id}] {Name}", craftsman.Id, name);

    //    return
    //    [
    //        new CraftsmanChunk
    //        {
    //            ChromaId    = $"craftsman-{craftsman.Id}",
    //            CraftsmanId = craftsman.Id,
    //            Text        = fullText,
    //            Metadata    = new Dictionary<string, string>
    //            {
    //                ["craftsman_id"]     = craftsman.Id.ToString(),
    //                ["name"]             = name,
    //                ["service_type"]     = craftsman.ServiceType,
    //                ["city"]             = craftsman.City,
    //                ["rating"]           = craftsman.Rating.ToString("F1"),
    //                ["experience_years"] = craftsman.Experience.ToString(),
    //                ["text"]             = fullText
    //            }
    //        }
    //    ];
    //}
    public List<CraftsmanChunk> ChunkCraftsman(Craftsman craftsman)
    {
        var serviceName = craftsman.Service?.NameAr ?? "غير محدد";
        string problems = ServiceProblems.TryGetValue(serviceName, out var list)
            ? string.Join("، ", list) : serviceName;

        string name = craftsman.User?.Name ?? $"حرفي #{craftsman.Id}";
        string bio = craftsman.Bio ?? "";

        // ── استخراج المحافظة فقط ─────────────────────────────
        string governorate = craftsman.CityNavigation?.Governorate ?? craftsman.CityNavigation?.NameAr ?? "";

        string fullText =
            $"الحرفي: {name}\n" +
            $"التخصص: {serviceName}\n" +
            $"المحافظة: {governorate}\n" +        // ← بقى محافظة مش مدينة كاملة
            $"الخبرة: {craftsman.Experience} سنة | التقييم: {craftsman.Rating}/5.0\n" +
            $"يحل مشاكل مثل: {problems}\n" +
            $"نبذة: {bio}";

        _logger.LogInformation("Chunked [{Id}] {Name} → governorate: {Gov}", craftsman.Id, name, governorate);

        return new List<CraftsmanChunk>
    {
        new CraftsmanChunk
        {
            ChromaId    = $"craftsman-{craftsman.Id}",
            CraftsmanId = craftsman.Id,
            Text        = fullText,
            Metadata    = new Dictionary<string, string>
            {
                ["craftsman_id"]     = craftsman.Id.ToString(),
                ["name"]             = name,
                ["service_type"]     = serviceName,
                ["city"]             = governorate,     // ← الـ city metadata بقت المحافظة فقط
                ["rating"]           = craftsman.Rating?.ToString("F1") ?? "0.0",
                ["experience_years"] = craftsman.Experience.ToString(),
                ["text"]             = fullText
            }
        }
    };
    }



}