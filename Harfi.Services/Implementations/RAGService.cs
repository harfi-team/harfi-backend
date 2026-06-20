
using Harfi.DTOs.RAG;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Harfi.Services.Implementations;

public class RAGService
{
    private readonly ICraftsmanRepository _repo;
    private readonly ChunkingService _chunker;
    private readonly EmbeddingService _embedder;
    private readonly VectorDbService _vectorDb;
    private readonly GroqRotatingClient _groqRotating;
    private readonly IConfiguration _config;
    private readonly ILogger<RAGService> _logger;
    private readonly AppDbContext _db;


    private static readonly (string Keyword, string Service)[] KeywordMap =
    [
        ("باب حديد","حدادة"),("بوابة حديد","حدادة"),("شباك حديد","حدادة"),
        ("قضبان","حدادة"),("درابزين حديد","حدادة"),("سور حديد","حدادة"),
        ("حداد","حدادة"),("لحام حديد","حدادة"),("باب جراج","حدادة"),("باب معدن","حدادة"),
        ("باب خشب","نجارة"),("شباك خشب","نجارة"),("دولاب خشب","نجارة"),
        ("نجار","نجارة"),("نجارة","نجارة"),("أثاث","نجارة"),("باركيه","نجارة"),
        ("سرير مكسور","نجارة"),("دولاب","نجارة"),("باب بيعلق","نجارة"),
        ("حنفية","سباكة"),("صنبور","سباكة"),("تسريب مياه","سباكة"),
        ("مواسير","سباكة"),("بالوعة","سباكة"),("صرف صحي","سباكة"),
        ("سخان مياه","سباكة"),("خزان مياه","سباكة"),("طلمبة مياه","سباكة"),
        ("مرحاض","سباكة"),("سباكة","سباكة"),("سيفون","سباكة"),
        ("كهرباء","كهرباء"),("سلك","كهرباء"),("لمبة","كهرباء"),
        ("قاطع","كهرباء"),("لوحة كهرباء","كهرباء"),("فيشة","كهرباء"),
        ("مقبس","كهرباء"),("تماس","كهرباء"),("إنارة","كهرباء"),
        ("طاقة شمسية","كهرباء"),("سولار","كهرباء"),("انفرتر","كهرباء"),
        ("تكييف","تكييف وتبريد"),("مكيف","تكييف وتبريد"),("فريون","تكييف وتبريد"),
        ("زجاج مكسور","زجاج ومرايا"),("شباك زجاج","زجاج ومرايا"),("مرايا","زجاج ومرايا"),
        ("شباك ألمنيوم","ألمنيوم"),("ألمنيوم","ألمنيوم"),("كلادينج","ألمنيوم"),
        ("سيراميك","تبليط وسيراميك"),("بلاطة","تبليط وسيراميك"),("بلاط","تبليط وسيراميك"),
        ("رخام","تبليط وسيراميك"),("بورسلين","تبليط وسيراميك"),("تبليط","تبليط وسيراميك"),
        ("دهان","دهانات"),("بوية","دهانات"),("طلاء","دهانات"),("بلاستر","دهانات"),
        ("تشقق","بناء"),("بناء","بناء"),("ترميم","بناء"),
        ("تشطيب","بناء"),("مقاول","بناء"),
        ("كاميرا","أمن وكاميرات"),("مراقبة","أمن وكاميرات"),("cctv","أمن وكاميرات"),("انتركوم","أمن وكاميرات"),
        ("جبس","جبس وأسقف"),("ديكور","جبس وأسقف"),("كورنيش","جبس وأسقف"),
        ("حشرات","مكافحة حشرات"),("رش","مكافحة حشرات"),
        ("صيانة","صيانة عامة"),
    ];

    private static readonly string[] AllCities =
    [
        "القاهرة","الجيزة","الإسكندرية","المنصورة","طنطا","أسيوط",
        "بورسعيد","السويس","الإسماعيلية","أسوان","الفيوم","الزقازيق",
        "المنوفية","الغربية","دمياط","الشرقية","كفر الشيخ","الدقهلية",
        "البحيرة","المنيا","بني سويف","سوهاج","قنا","الأقصر",
        "مطروح","شمال سيناء","جنوب سيناء","البحر الأحمر"
    ];

    private static readonly (string Trigger, string Extra)[] Expansions =
    [
        ("حنفية بتقطر",    "تسريب مياه صنبور سباكة إصلاح"),
        ("حنفية مكسورة",   "تسريب مياه صنبور سباكة"),
        ("صنبور بيسرب",    "تسريب مياه حنفية سباكة"),
        ("كهرباء اتقطعت",  "انقطاع تماس لوحة كهربائي"),
        ("تكييف مش بيبرد", "مكيف خربان فريون ناقص صيانة"),
        ("باب مكسور",      "إصلاح باب نجار حداد"),
        ("بالوعة مسدودة",  "انسداد صرف صحي تسليك سباكة"),
        ("سيراميك مكسور",  "تركيب بلاط إصلاح أرضية"),
        ("جدار متشقق",     "تشقق ترميم بناء إصلاح"),
        ("مياه بتنزل",     "تسريب سباكة مواسير"),
    ];

    private static readonly Dictionary<string, string[]> NearbyMap = new()
    {
        ["القاهرة"] = ["الجيزة", "الشرقية", "المنوفية", "الغربية", "المنصورة", "طنطا", "الإسكندرية", "الإسماعيلية", "السويس", "الزقازيق", "دمياط", "الفيوم", "بورسعيد", "كفر الشيخ", "الدقهلية", "البحيرة", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "المنيا", "بني سويف", "شمال سيناء", "جنوب سيناء", "مطروح", "البحر الأحمر"],
        ["الجيزة"] = ["القاهرة", "الفيوم", "بني سويف", "المنيا", "المنوفية", "الإسكندرية", "طنطا", "الغربية", "المنصورة", "الشرقية", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "الزقازيق", "الإسماعيلية", "السويس", "بورسعيد", "دمياط", "كفر الشيخ", "الدقهلية", "البحيرة", "شمال سيناء", "جنوب سيناء", "مطروح", "البحر الأحمر"],
        ["الإسكندرية"] = ["البحيرة", "مطروح", "الغربية", "المنوفية", "طنطا", "الجيزة", "القاهرة", "المنصورة", "كفر الشيخ", "الدقهلية", "دمياط", "الشرقية", "الفيوم", "بني سويف", "المنيا", "الزقازيق", "الإسماعيلية", "بورسعيد", "السويس", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["المنصورة"] = ["الدقهلية", "الغربية", "الشرقية", "دمياط", "طنطا", "الزقازيق", "كفر الشيخ", "الإسكندرية", "المنوفية", "القاهرة", "بورسعيد", "الجيزة", "الإسماعيلية", "السويس", "الفيوم", "البحيرة", "مطروح", "بني سويف", "المنيا", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["طنطا"] = ["الغربية", "المنوفية", "المنصورة", "الإسكندرية", "كفر الشيخ", "الدقهلية", "القاهرة", "دمياط", "البحيرة", "الشرقية", "الجيزة", "الزقازيق", "مطروح", "الفيوم", "الإسماعيلية", "بورسعيد", "السويس", "بني سويف", "المنيا", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["أسيوط"] = ["المنيا", "سوهاج", "الفيوم", "بني سويف", "قنا", "الجيزة", "القاهرة", "الأقصر", "أسوان", "المنصورة", "الغربية", "المنوفية", "طنطا", "الشرقية", "الإسكندرية", "الزقازيق", "الإسماعيلية", "السويس", "دمياط", "الدقهلية", "كفر الشيخ", "البحيرة", "بورسعيد", "مطروح", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["بورسعيد"] = ["الإسماعيلية", "الشرقية", "المنصورة", "الزقازيق", "القاهرة", "السويس", "دمياط", "الدقهلية", "الجيزة", "الغربية", "طنطا", "المنوفية", "الإسكندرية", "كفر الشيخ", "البحيرة", "الفيوم", "بني سويف", "المنيا", "مطروح", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["السويس"] = ["الإسماعيلية", "القاهرة", "الشرقية", "بورسعيد", "الجيزة", "الزقازيق", "المنصورة", "شمال سيناء", "جنوب سيناء", "البحر الأحمر", "الغربية", "طنطا", "المنوفية", "دمياط", "الدقهلية", "كفر الشيخ", "الإسكندرية", "البحيرة", "الفيوم", "بني سويف", "المنيا", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "مطروح"],
        ["الإسماعيلية"] = ["بورسعيد", "السويس", "الشرقية", "القاهرة", "الزقازيق", "الجيزة", "المنصورة", "شمال سيناء", "الغربية", "طنطا", "المنوفية", "دمياط", "الدقهلية", "كفر الشيخ", "الإسكندرية", "البحيرة", "الفيوم", "بني سويف", "المنيا", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "جنوب سيناء", "البحر الأحمر", "مطروح"],
        ["أسوان"] = ["قنا", "الأقصر", "سوهاج", "أسيوط", "المنيا", "البحر الأحمر", "جنوب سيناء", "بني سويف", "الفيوم", "الجيزة", "القاهرة", "الإسكندرية", "المنصورة", "الغربية", "طنطا", "المنوفية", "الشرقية", "الزقازيق", "الإسماعيلية", "السويس", "بورسعيد", "دمياط", "الدقهلية", "كفر الشيخ", "البحيرة", "مطروح", "شمال سيناء"],
        ["الفيوم"] = ["الجيزة", "بني سويف", "القاهرة", "المنيا", "المنوفية", "أسيوط", "الغربية", "طنطا", "المنصورة", "الشرقية", "الإسكندرية", "الزقازيق", "الإسماعيلية", "السويس", "دمياط", "الدقهلية", "كفر الشيخ", "البحيرة", "بورسعيد", "مطروح", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["الزقازيق"] = ["الشرقية", "القاهرة", "المنصورة", "الإسماعيلية", "الجيزة", "دمياط", "بورسعيد", "السويس", "الغربية", "طنطا", "المنوفية", "الدقهلية", "كفر الشيخ", "الإسكندرية", "البحيرة", "الفيوم", "بني سويف", "المنيا", "مطروح", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["المنوفية"] = ["القاهرة", "الجيزة", "طنطا", "الغربية", "الإسكندرية", "المنصورة", "كفر الشيخ", "البحيرة", "الشرقية", "الدقهلية", "دمياط", "الزقازيق", "الفيوم", "الإسماعيلية", "بورسعيد", "السويس", "بني سويف", "المنيا", "مطروح", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["الغربية"] = ["طنطا", "المنوفية", "المنصورة", "الإسكندرية", "كفر الشيخ", "القاهرة", "الدقهلية", "البحيرة", "دمياط", "الشرقية", "الجيزة", "الزقازيق", "مطروح", "الفيوم", "الإسماعيلية", "بورسعيد", "السويس", "بني سويف", "المنيا", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["دمياط"] = ["المنصورة", "الدقهلية", "بورسعيد", "الشرقية", "كفر الشيخ", "الغربية", "القاهرة", "الزقازيق", "طنطا", "الإسكندرية", "المنوفية", "الجيزة", "الإسماعيلية", "السويس", "البحيرة", "مطروح", "الفيوم", "بني سويف", "المنيا", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["الشرقية"] = ["القاهرة", "المنصورة", "الزقازيق", "الإسماعيلية", "الجيزة", "دمياط", "بورسعيد", "السويس", "الغربية", "طنطا", "المنوفية", "الدقهلية", "كفر الشيخ", "الإسكندرية", "شمال سيناء", "البحيرة", "الفيوم", "بني سويف", "المنيا", "مطروح", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "جنوب سيناء", "البحر الأحمر"],
        ["كفر الشيخ"] = ["الغربية", "البحيرة", "الدقهلية", "المنوفية", "طنطا", "الإسكندرية", "المنصورة", "دمياط", "القاهرة", "مطروح", "الجيزة", "الشرقية", "الزقازيق", "الإسماعيلية", "بورسعيد", "السويس", "الفيوم", "بني سويف", "المنيا", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["الدقهلية"] = ["المنصورة", "دمياط", "كفر الشيخ", "الغربية", "الشرقية", "الزقازيق", "طنطا", "المنوفية", "الإسكندرية", "القاهرة", "بورسعيد", "الجيزة", "الإسماعيلية", "السويس", "البحيرة", "مطروح", "الفيوم", "بني سويف", "المنيا", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["البحيرة"] = ["الإسكندرية", "مطروح", "كفر الشيخ", "الغربية", "المنوفية", "طنطا", "الجيزة", "القاهرة", "الدقهلية", "المنصورة", "دمياط", "الشرقية", "الزقازيق", "الفيوم", "بني سويف", "الإسماعيلية", "بورسعيد", "السويس", "المنيا", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["المنيا"] = ["بني سويف", "أسيوط", "الفيوم", "سوهاج", "الجيزة", "القاهرة", "قنا", "الأقصر", "أسوان", "المنوفية", "الغربية", "طنطا", "المنصورة", "الشرقية", "الإسكندرية", "الزقازيق", "الإسماعيلية", "السويس", "دمياط", "الدقهلية", "كفر الشيخ", "البحيرة", "بورسعيد", "مطروح", "شمال سيناء", "جنوب سيناء", "البحر الأحمر"],
        ["بني سويف"] = ["الفيوم", "الجيزة", "المنيا", "القاهرة", "أسيوط", "سوهاج", "المنوفية", "الغربية", "طنطا", "المنصورة", "الشرقية", "الإسكندرية", "الزقازيق", "الإسماعيلية", "السويس", "دمياط", "الدقهلية", "كفر الشيخ", "البحيرة", "بورسعيد", "مطروح", "قنا", "الأقصر", "أسوان", "شمال سيناء", "البحر الأحمر"],
        ["سوهاج"] = ["أسيوط", "قنا", "المنيا", "الأقصر", "بني سويف", "الفيوم", "أسوان", "الجيزة", "القاهرة", "البحر الأحمر", "المنوفية", "الغربية", "طنطا", "المنصورة", "الشرقية", "الإسكندرية", "الزقازيق", "الإسماعيلية", "السويس", "دمياط", "الدقهلية", "كفر الشيخ", "البحيرة", "بورسعيد", "مطروح", "شمال سيناء", "جنوب سيناء"],
        ["قنا"] = ["الأقصر", "سوهاج", "أسوان", "أسيوط", "المنيا", "البحر الأحمر", "بني سويف", "الفيوم", "الجيزة", "القاهرة", "جنوب سيناء", "الغربية", "طنطا", "المنصورة", "المنوفية", "الشرقية", "الإسكندرية", "الزقازيق", "الإسماعيلية", "السويس", "دمياط", "الدقهلية", "كفر الشيخ", "البحيرة", "بورسعيد", "مطروح", "شمال سيناء"],
        ["الأقصر"] = ["قنا", "أسوان", "سوهاج", "البحر الأحمر", "أسيوط", "المنيا", "جنوب سيناء", "بني سويف", "الفيوم", "الجيزة", "القاهرة", "الغربية", "طنطا", "المنصورة", "المنوفية", "الشرقية", "الإسكندرية", "الزقازيق", "الإسماعيلية", "السويس", "دمياط", "الدقهلية", "كفر الشيخ", "البحيرة", "بورسعيد", "مطروح", "شمال سيناء"],
        ["مطروح"] = ["الإسكندرية", "البحيرة", "كفر الشيخ", "الغربية", "المنوفية", "طنطا", "الجيزة", "القاهرة", "الدقهلية", "المنصورة", "جنوب سيناء", "دمياط", "الشرقية", "الزقازيق", "الفيوم", "بني سويف", "المنيا", "الإسماعيلية", "بورسعيد", "السويس", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "شمال سيناء", "البحر الأحمر"],
        ["شمال سيناء"] = ["الإسماعيلية", "بورسعيد", "السويس", "الشرقية", "القاهرة", "الزقازيق", "الجيزة", "جنوب سيناء", "المنصورة", "الغربية", "طنطا", "المنوفية", "دمياط", "الدقهلية", "كفر الشيخ", "الإسكندرية", "البحيرة", "الفيوم", "بني سويف", "المنيا", "مطروح", "أسيوط", "سوهاج", "قنا", "الأقصر", "أسوان", "البحر الأحمر"],
        ["جنوب سيناء"] = ["السويس", "الإسماعيلية", "البحر الأحمر", "شمال سيناء", "بورسعيد", "القاهرة", "الشرقية", "الزقازيق", "الجيزة", "قنا", "الأقصر", "أسوان", "الغربية", "طنطا", "المنوفية", "دمياط", "الدقهلية", "كفر الشيخ", "الإسكندرية", "البحيرة", "مطروح", "الفيوم", "بني سويف", "المنيا", "أسيوط", "سوهاج", "المنصورة"],
        ["البحر الأحمر"] = ["السويس", "جنوب سيناء", "الأقصر", "قنا", "أسوان", "سوهاج", "الإسماعيلية", "بورسعيد", "القاهرة", "الجيزة", "أسيوط", "المنيا", "بني سويف", "الفيوم", "الشرقية", "الزقازيق", "المنصورة", "الغربية", "طنطا", "المنوفية", "دمياط", "الدقهلية", "كفر الشيخ", "الإسكندرية", "البحيرة", "مطروح", "شمال سيناء"],
    };

    public RAGService(
        ICraftsmanRepository repo, ChunkingService chunker,
        EmbeddingService embedder, VectorDbService vectorDb,
        IHttpClientFactory httpFactory, IConfiguration config,
        GroqRotatingClient groqRotating,
        ILogger<RAGService> logger, AppDbContext db)
    {
        _repo = repo;
        _chunker = chunker;
        _embedder = embedder;
        _vectorDb = vectorDb;
        _db = db;
        _groqRotating = groqRotating;
        _config = config;
        _logger = logger;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  INGESTION
    // ════════════════════════════════════════════════════════════════════════

    public async Task<IngestResponse> IngestAllCraftsmenAsync(int fromId = 0)
    {
        var allEnum = await _repo.GetAllAsync();
        var all = allEnum.ToList();
        var craftsmen = fromId > 0 ? all.Where(c => c.Id >= fromId).ToList() : all;

        _logger.LogInformation("[Ingest] {N}/{T} craftsmen", craftsmen.Count, all.Count);
        await _vectorDb.EnsureCollectionAsync();

        var chunks = craftsmen.SelectMany(_chunker.ChunkCraftsman).ToList();
        _logger.LogInformation("[Ingest] {N} chunks total", chunks.Count);

        const int BatchSize = 2;
        const int DelayMs = 45_000;
        int done = 0;

        for (int i = 0; i < chunks.Count; i += BatchSize)
        {
            var batch = chunks.Skip(i).Take(BatchSize).ToList();
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    var embeddings = await _embedder.EmbedBatchAsync(
                        batch.Select(c => c.Text).ToList());
                    for (int j = 0; j < batch.Count; j++)
                        batch[j].Embedding = embeddings[j];
                    await _vectorDb.AddChunksAsync(batch);
                    done += batch.Count;
                    _logger.LogInformation("[Ingest] ✓ {Done}/{All}", done, chunks.Count);
                    break;
                }
                catch (Exception ex) when (attempt < 3)
                {
                    _logger.LogWarning("[Ingest] Attempt {A} failed: {M} — wait 65s",
                        attempt, ex.Message);
                    await Task.Delay(65_000);
                }
                catch (Exception ex)
                {
                    int failId = batch.First().CraftsmanId;
                    _logger.LogError("[Ingest] Failed at id={Id}: {M}", failId, ex.Message);
                    return new IngestResponse
                    {
                        TotalCraftsmen = craftsmen.Count,
                        TotalChunksIndexed = done,
                        Message = $"Paused at id={failId}. Resume: POST /ingest?fromId={failId}"
                    };
                }
            }
            if (i + BatchSize < chunks.Count)
            {
                _logger.LogInformation("[Ingest] ⏳ Waiting 45s...");
                await Task.Delay(DelayMs);
            }
        }

        return new IngestResponse
        {
            TotalCraftsmen = all.Count(),
            TotalChunksIndexed = done,
            Message = "Ingestion complete ✓"
        };
    }


    public async Task<QueryResponse> QueryAsync(QueryRequest request)
    {
        var sw = Stopwatch.StartNew();
        string q = request.Question.ToLowerInvariant().Trim();

        var (detectedService, detectedCity) = DetectIntent(q);

        string? service = !string.IsNullOrEmpty(request.ExtractedService)
            ? request.ExtractedService : detectedService;

        string? city = !string.IsNullOrEmpty(request.ExtractedCity)
            ? request.ExtractedCity : detectedCity;

        string? userNeighborhood = request.ExtractedDistrict;

        _logger.LogInformation("[Query] service={S} city={C} topK={K}",
            service ?? "any", city ?? "any", request.TopK);

        if (service is null)
            return await SqlFallbackAsync(request, null, city, sw);

        string expanded = ExpandQuery(request.Question, q);

        float[] qEmbed;
        try { qEmbed = await _embedder.EmbedQueryAsync(expanded); }
        catch (Exception ex)
        {
            _logger.LogWarning("[Query] Embed failed: {M}", ex.Message);
            return await SqlFallbackAsync(request, service, city, sw);
        }

        const int QdrantFetchMultiplier = 10;
        int fetchCount = request.TopK * QdrantFetchMultiplier;

        // normalize اسم الخدمة عشان يتطابق مع اللي متخزن في Qdrant
        string qdrantService = await NormalizeServiceForQdrantAsync(service);
        _logger.LogInformation("[Query] service normalized: {S} → {Q}", service, qdrantService);

        QdrantSearchResponse qdrantResult;
        try
        {
            qdrantResult = await _vectorDb.SearchByServiceOnlyAsync(
                qEmbed, fetchCount, qdrantService);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Query] Qdrant failed: {M}", ex.Message);
            return await SqlFallbackAsync(request, service, city, sw);
        }

        _logger.LogInformation("[Query] Qdrant returned {N} results for service={S}",
            qdrantResult.result?.Count ?? 0, qdrantService);
        _logger.LogInformation("[Query] Sample IDs: {IDs}",
            string.Join(", ", (qdrantResult.result ?? []).Take(5)
                .Select(p => GetInt(p.payload, "craftsman_id"))));

        var qdrantScores = (qdrantResult.result ?? [])
            .Select(p => new { Id = GetInt(p.payload, "craftsman_id"), Score = p.score })
            .Where(x => x.Id != 0)
            .GroupBy(x => x.Id)
            .Select(g => g.OrderByDescending(x => x.Score).First())
            .ToList();

        if (qdrantScores.Count == 0)
            return await SqlFallbackAsync(request, service, city, sw);

        var allIds = qdrantScores.Select(x => x.Id).ToList();
        var scoreMap = qdrantScores.ToDictionary(x => x.Id, x => x.Score);

        //var allCraftsmen = await _db.Craftsmen
        //    .Include(c => c.User)
        //    .Where(c => allIds.Contains(c.Id))
        //    .ToListAsync();
        var allCraftsmen = await _db.Craftsmen
    .Include(c => c.User)
    .Where(c => allIds.Contains(c.Id) && !c.IsDeleted && c.IsApproved && c.IsAvailable)
    .ToListAsync();

        _logger.LogInformation("[Query] SQL returned {N} craftsmen for IDs={IDs}",
            allCraftsmen.Count, string.Join(", ", allIds));

        var localCraftsmen = city is null
            ? allCraftsmen
            : allCraftsmen.Where(c => c.City == city).ToList();

        _logger.LogInformation("[Query] Local craftsmen (city={C}): {N}",
            city ?? "any", localCraftsmen.Count);

        List<FinalCraftsman> verifiedLocal;
        List<FinalCraftsman> verifiedNearby = [];

        if (localCraftsmen.Count >= request.TopK || city is null)
        {
            var topLocal = localCraftsmen
                .OrderByDescending(c => scoreMap.GetValueOrDefault(c.Id))
                .Take(request.TopK)
                .Select(c => new FinalCraftsman(c,
                    FuseScore(scoreMap.GetValueOrDefault(c.Id), c.Rating), false, city))
                .ToList();

            verifiedLocal = topLocal;
        }
        else
        {
            var usedIds = new HashSet<int>(localCraftsmen.Select(c => c.Id));

            verifiedLocal = localCraftsmen
                .OrderByDescending(c => scoreMap.GetValueOrDefault(c.Id))
                .Select(c => new FinalCraftsman(c,
                    FuseScore(scoreMap.GetValueOrDefault(c.Id), c.Rating), false, city))
                .ToList();

            int stillNeeded = request.TopK - verifiedLocal.Count;

            var nearbyCities = NearbyMap.TryGetValue(city, out var nb) ? nb : [];

            var nearbyCandidates = allCraftsmen
                .Where(c => !usedIds.Contains(c.Id) && nearbyCities.Contains(c.City))
                .OrderBy(c => Array.IndexOf(nearbyCities, c.City))
                .ThenByDescending(c => scoreMap.GetValueOrDefault(c.Id))
                .Take(stillNeeded)
                .Select(c => new FinalCraftsman(c,
                    FuseScore(scoreMap.GetValueOrDefault(c.Id), c.Rating), true, city))
                .ToList();

            verifiedNearby = nearbyCandidates;
        }

        var allVerified = verifiedLocal.Concat(verifiedNearby).ToList();

        _logger.LogInformation("[Query] Final: local={L} nearby={NB} total={T}",
            verifiedLocal.Count, verifiedNearby.Count, allVerified.Count);

        if (allVerified.Count == 0)
            return await SqlFallbackAsync(request, service, city, sw);

       
        {
            var allSqlCandidates = await _db.Craftsmen
                .Include(c => c.User)
                .Where(c => !c.IsDeleted && c.IsApproved && c.IsAvailable)
                .Where(c => c.ServiceType == qdrantService)
                .OrderByDescending(c => c.Rating)
                .ToListAsync();

            if (allSqlCandidates.Count > 0)
            {
                var foundIds = allVerified.Select(fc => fc.Craftsman.Id).ToHashSet();
                var missing = allSqlCandidates.Where(c => !foundIds.Contains(c.Id)).ToList();

                _logger.LogInformation("[SafetyNet] Total={T} Found={F} Missing={M}",
                    allSqlCandidates.Count, foundIds.Count, missing.Count);

                var safetyRanked = await RankByProximityAsync(
                    allSqlCandidates, city ?? "", qdrantService, request.TopK);

                allVerified = safetyRanked
                    .Select(c => new FinalCraftsman(
                        c, (double)c.Rating / 5.0,
                        city is null || c.City != city, city))
                    .ToList();

                _logger.LogInformation("[SafetyNet] Final after LLM rerank: {N}", allVerified.Count);
            }
        }
        if (!string.IsNullOrEmpty(userNeighborhood) && allVerified.Count > 1)
        {
            allVerified = await ReRankByNeighborhoodAsync(allVerified, userNeighborhood);
        }

        string nearbyNote = BuildNearbyNote(city, service, verifiedLocal.Count, verifiedNearby.Count);
        string context = BuildContext(allVerified, verifiedLocal.Count, nearbyNote);
        string answer = await CallGroqAsync(request.Question, context, nearbyNote);

        sw.Stop();

        return new QueryResponse
        {
            Answer = answer,
            RetrievedCraftsmen = allVerified.Select(fc =>
            {
                var c = fc.Craftsman;
                return new RetrievedCraftsmanDto
                {
                    Id = c.Id,
                    Name = c.User.Name,
                    ServiceType = c.ServiceType,
                    City = c.City,
                    Neighborhood = c.Neighborhood,
                    Rating = (double)c.Rating,
                    ExperienceYears = c.Experience,
                    PriceRangeMin = c.PriceRangeMin,
                    PriceRangeMax = c.PriceRangeMax,
                    RelevantText = (c.Bio ?? "")[..Math.Min(300, (c.Bio ?? "").Length)],
                    SimilarityScore = Math.Round(fc.Score, 4),
                    IsNearby = fc.IsNearby,
                    NearbyFromCity = fc.IsNearby ? fc.RequestedCity : null
                };
            }).ToList(),
            LatencyMs = sw.ElapsedMilliseconds
        };
    }

    private async Task<string> NormalizeServiceForQdrantAsync(string service)
    {
        // لو موجود في الـ switch العادي رجّعه فوراً بدون LLM call
        var quick = service switch
        {
            "سباك" or "سباكه" => "سباكة",
            "كهربائي" or "كهربجي" or "كهرباجي" => "كهرباء",
            "نجار" or "نجاره" or "موبيليا" => "نجارة",
            "حداد" or "حداده" or "لحام" => "حدادة",
            "دهان" or "نقاش" or "بوية" or "بويه" => "دهانات",
            "سيراميك" or "بلاط" or "تبليط" or "رخام" => "تبليط وسيراميك",
            "تكييف" or "مكيف" or "تبريد" => "تكييف وتبريد",
            "جبس" or "أسقف" or "اسقف" or "ديكور" => "جبس وأسقف",
            "زجاج" or "مرايا" or "زجاج وألمنيوم" => "زجاج ومرايا",
            "ألمنيوم" or "الومنيوم" or "شباك ألمنيوم" => "ألمنيوم",
            "كاميرا" or "كاميرات" or "مراقبة" or "انتركوم" or "أمن" => "أمن وكاميرات",
            "حشرات" or "رش" or "تعقيم" => "مكافحة حشرات",
            "مقاول" or "ترميم" => "بناء",
            "صيانة" => "صيانة عامة",
            _ => null  // مش عارفه → روح للـ LLM
        };

        if (quick is not null)
        {
            _logger.LogInformation("[Normalize] Quick match: {S} → {Q}", service, quick);
            return quick;
        }

        
        var validServices = await _db.Craftsmen
    .Where(c => !c.IsDeleted && c.IsApproved && c.IsAvailable && c.ServiceType != "AI")
    .Select(c => c.ServiceType)
    .Distinct()
    .ToArrayAsync();
        string prompt =
            "أنت مساعد لتصنيف الخدمات.\n\n" +
            "الخدمة المدخلة: [" + service + "]\n\n" +
            "قائمة الخدمات المتاحة:\n" +
            string.Join("\n", validServices.Select((s, i) => $"{i + 1}. {s}")) + "\n\n" +
            "المطلوب: اختر الخدمة الأقرب من القائمة للخدمة المدخلة.\n" +
            "رد بـ JSON فقط: {\"service\": \"اسم الخدمة من القائمة\"}";

        try
        {
            var payload = new
            {
                model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
                temperature = 0.0,
                max_tokens = 50,
                messages = new[]
                {
                new { role = "system", content = prompt },
                new { role = "user",   content = "صنّف الخدمة" }
            }
            };

            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("[Normalize] LLM failed — using original: {S}", service);
                return service;
            }

            var result = await resp.Content.ReadFromJsonAsync<GroqResp>();
            string raw = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            string json = raw.Replace("```json", "").Replace("```", "").Trim();

            using var doc = JsonDocument.Parse(json);
            string normalized = doc.RootElement.GetProperty("service").GetString() ?? service;

            // تأكد إن النتيجة من القائمة المسموح بيها
            if (!validServices.Contains(normalized))
            {
                _logger.LogWarning("[Normalize] LLM returned invalid service: {S} — using original", normalized);
                return service;
            }

            _logger.LogInformation("[Normalize] LLM: {S} → {Q}", service, normalized);
            return normalized;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[Normalize] Exception: {M} — using original", ex.Message);
            return service;
        }
    }
    private static double FuseScore(double vectorScore, decimal rating)
        => 0.7 * vectorScore + 0.3 * ((double)rating / 5.0);
    // ── Safety Net: ترتيب الحرفيين بالقرب من المدينة المكتوبة ───────────────
    private async Task<List<Harfi.Models.Entities.Craftsman>> RankByProximityAsync(
        List<Harfi.Models.Entities.Craftsman> candidates, string userCity, string service, int needed)
    {
        if (candidates.Count == 0) return [];

        var list = string.Join("\n", candidates.Select(c =>
            $"ID={c.Id} | التخصص: {c.ServiceType} | المدينة: {c.City} | الحي: {c.Neighborhood ?? ""} | التقييم: {c.Rating}"));

        string prompt =
            "أنت مساعد لترتيب الحرفيين حسب القرب الجغرافي.\n" +
            $"المدينة/المحافظة التي أدخلها المستخدم: [{userCity}]\n" +
            $"التخصص المطلوب: [{service}]\n\n" +
            "قائمة الحرفيين المتاحين:\n" + list + "\n\n" +
            "قواعد:\n" +
            "- رتّب الحرفيين من الأقرب للأبعد لمدينة المستخدم\n" +
            "- الأولوية: نفس المدينة أو المحافظة أولاً، ثم المجاورة\n" +
            "- لو المستخدم كتب مدينة (مثل مدينة نصر) اعتبرها في محافظتها (القاهرة)\n" +
            $"- اختر أفضل {needed} فقط\n" +
            $"- رد بـ JSON فقط: {{\"ranked_ids\": [ID1, ID2, ...]}}";

        try
        {
            var payload = new
            {
                model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
                temperature = 0.0,
                max_tokens = 200,
                messages = new[]
                {
                    new { role = "system", content = prompt },
                    new { role = "user",   content = "رتّب الحرفيين" }
                }
            };

            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
            if (!resp.IsSuccessStatusCode) return candidates.Take(needed).ToList();

            var result = await resp.Content.ReadFromJsonAsync<GroqResp>();
            string raw = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            string json = raw.Replace("```json", "").Replace("```", "").Trim();

            using var doc = JsonDocument.Parse(json);
            var rankedIds = doc.RootElement
                .GetProperty("ranked_ids")
                .EnumerateArray()
                .Select(x => x.GetInt32())
                .ToList();

            var dict = candidates.ToDictionary(c => c.Id);
            var reranked = rankedIds
                .Where(id => dict.ContainsKey(id))
                .Select(id => dict[id])
                .Take(needed)
                .ToList();

            var usedIds = reranked.Select(c => c.Id).ToHashSet();
            reranked.AddRange(candidates
                .Where(c => !usedIds.Contains(c.Id))
                .Take(needed - reranked.Count));

            _logger.LogInformation("[SafetyNet] LLM ranked {N} craftsmen near: {C}", reranked.Count, userCity);
            return reranked;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[SafetyNet] RankByProximity failed: {M}", ex.Message);
            return candidates.Take(needed).ToList();
        }
    }

    private async Task<List<FinalCraftsman>> ReRankByNeighborhoodAsync(
    List<FinalCraftsman> craftsmen, string userLocation)
    {
        // جيب الـ Neighborhood من SQL (مدينة + شارع)
        var ids = craftsmen.Select(fc => fc.Craftsman.Id).ToList();
        var neighborhoodMap = await _db.Craftsmen
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.Neighborhood })
            .ToDictionaryAsync(x => x.Id, x => x.Neighborhood ?? "");

        var list = string.Join("\n", craftsmen.Select((fc, i) =>
        {
            var nb = neighborhoodMap.TryGetValue(fc.Craftsman.Id, out var n) ? n : "";
            return $"{i + 1}. ID={fc.Craftsman.Id} | المحافظة: {fc.Craftsman.City} | العنوان: {nb}";
        }));

        string prompt =
            "أنت مساعد لترتيب الحرفيين حسب القرب الجغرافي.\n" +
            "موقع المستخدم: [" + userLocation + "]\n\n" +
            "قائمة الحرفيين:\n" + list + "\n\n" +
            "قواعد:\n" +
            "- رتّب من الأقرب للأبعد بناءً على تشابه المدينة والشارع مع موقع المستخدم\n" +
            "- الأولوية الأولى: نفس المدينة — الأولوية الثانية: أقرب شارع\n" +
            "- رد بـ JSON فقط: {\"ranked_ids\": [ID1, ID2, ...]}\n" +
            "- اذكر كل الـ IDs";

        try
        {
            var payload = new
            {
                model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
                temperature = 0.0,
                max_tokens = 200,
                messages = new[]
                {
                new { role = "system", content = prompt },
                new { role = "user",   content = "رتّب الحرفيين" }
            }
            };

            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
            if (!resp.IsSuccessStatusCode) return craftsmen;

            var result = await resp.Content.ReadFromJsonAsync<GroqResp>();
            string raw = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            string json = raw.Replace("```json", "").Replace("```", "").Trim();

            using var doc = JsonDocument.Parse(json);
            var rankedIds = doc.RootElement
                .GetProperty("ranked_ids")
                .EnumerateArray()
                .Select(x => x.GetInt32())
                .ToList();

            var craftsmanDict = craftsmen.ToDictionary(fc => fc.Craftsman.Id);
            var reranked = rankedIds
                .Where(id => craftsmanDict.ContainsKey(id))
                .Select(id => craftsmanDict[id])
                .ToList();

            // أي حد نسيه الـ LLM يتضاف في الآخر
            reranked.AddRange(craftsmen.Where(fc => !rankedIds.Contains(fc.Craftsman.Id)));

            _logger.LogInformation("[ReRank] Reranked {N} craftsmen near: {L}",
                reranked.Count, userLocation);
            return reranked;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[ReRank] Failed: {M}", ex.Message);
            return craftsmen;
        }
    }
    // ════════════════════════════════════════════════════════════════════════
    //  Re-rank by district (مدينة + شارع المستخدم)
    // ════════════════════════════════════════════════════════════════════════

    private async Task<List<FinalCraftsman>> ReRankByDistrictAsync(
        List<FinalCraftsman> craftsmen, string userDistrict)
    {
        // جيب العناوين الكاملة من SQL
        var ids = craftsmen.Select(fc => fc.Craftsman.Id).ToList();
        var addressMap = await _db.Craftsmen
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.City })  // City فيها العنوان الكامل
            .ToDictionaryAsync(x => x.Id, x => x.City ?? "");

        // ابعت للـ LLM يرتبهم
        var list = string.Join("\n", craftsmen.Select((fc, i) =>
        {
            var addr = addressMap.TryGetValue(fc.Craftsman.Id, out var a) ? a : "";
            return $"{i + 1}. ID={fc.Craftsman.Id} | العنوان: {addr}";
        }));

        string prompt =
            "أنت مساعد لترتيب الحرفيين حسب القرب الجغرافي.\n" +
            "عنوان المستخدم: [" + userDistrict + "]\n\n" +
            "قائمة الحرفيين بعناوينهم:\n" + list + "\n\n" +
            "قواعد:\n" +
            "- رتّب الحرفيين من الأقرب للأبعد بناءً على تشابه المدينة والشارع مع عنوان المستخدم\n" +
            "- الأولوية: نفس المدينة أولاً، ثم أقرب شارع\n" +
            "- رد بـ JSON فقط: {\"ranked_ids\": [ID1, ID2, ...]}\n" +
            "- اذكر كل الـ IDs بالترتيب";

        try
        {
            var payload = new
            {
                model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
                temperature = 0.0,
                max_tokens = 200,
                messages = new[]
                {
                new { role = "system", content = prompt },
                new { role = "user",   content = "رتّب الحرفيين حسب القرب من عنوان المستخدم" }
            }
            };

            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
            if (!resp.IsSuccessStatusCode) return craftsmen;

            var result = await resp.Content.ReadFromJsonAsync<GroqResp>();
            string raw = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            string json = raw.Replace("```json", "").Replace("```", "").Trim();

            using var doc = JsonDocument.Parse(json);
            var rankedIds = doc.RootElement
                .GetProperty("ranked_ids")
                .EnumerateArray()
                .Select(x => x.GetInt32())
                .ToList();

            // رتّب الـ craftsmen حسب ترتيب الـ LLM
            var craftsmanDict = craftsmen.ToDictionary(fc => fc.Craftsman.Id);
            var reranked = rankedIds
                .Where(id => craftsmanDict.ContainsKey(id))
                .Select(id => craftsmanDict[id])
                .ToList();

            // أي حد مش في القائمة (لو LLM نسي حد) يتضاف في الآخر
            var missing = craftsmen.Where(fc => !rankedIds.Contains(fc.Craftsman.Id)).ToList();
            reranked.AddRange(missing);

            _logger.LogInformation("[ReRank] Reranked {N} craftsmen by district: {D}", reranked.Count, userDistrict);
            return reranked;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("[ReRank] Failed: {M}", ex.Message);
            return craftsmen; // fallback: رجّع الترتيب الأصلي
        }
    }
    // ════════════════════════════════════════════════════════════════════════
    //  LLM يتحقق من التخصص
    // ════════════════════════════════════════════════════════════════════════

    private async Task<List<RankedCraftsman>> VerifyServiceTypeAsync(
        List<RankedCraftsman> candidates, string requiredService, int needed)
    {
        if (candidates.Count == 0) return [];

        var list = string.Join("\n", candidates.Select((c, i) =>
            $"{i + 1}. ID={c.Craftsman.Id} | {c.Craftsman.User.Name} | التخصص: {c.Craftsman.ServiceType} | {c.Craftsman.City}"));

        string prompt =
            "أنت مساعد للتحقق من تخصصات الحرفيين.\n" +
            "التخصص المطلوب بالضبط: [" + requiredService + "]\n\n" +
            "القائمة:\n" + list + "\n\n" +
            "قواعد صارمة:\n" +
            "- اختر فقط من تخصصهم هو [" + requiredService + "] بالضبط\n" +
            "- 'صيانة عامة' لا يُقبل إلا لو التخصص المطلوب هو 'صيانة عامة'\n" +
            "- اختر أقصى " + needed + " حرفيين\n" +
            "- رد بـ JSON فقط: {\"verified_ids\": [ID1, ID2, ...]}\n" +
            "- لو مفيش مناسب: {\"verified_ids\": []}";

        try
        {
            var payload = new
            {
                model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
                temperature = 0.0,
                max_tokens = 150,
                messages = new[]
                {
                    new { role = "system", content = prompt },
                    new { role = "user",   content = "تحقق واختر من تخصصهم [" + requiredService + "] فقط" }
                }
            };

            var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
            if (!resp.IsSuccessStatusCode)
                return HardFilter(candidates, requiredService, needed);

            var result = await resp.Content.ReadFromJsonAsync<GroqResp>();
            string raw = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim() ?? "";
            string json = raw.Replace("```json", "").Replace("```", "").Trim();

            using var doc = JsonDocument.Parse(json);
            var ids = doc.RootElement
                .GetProperty("verified_ids")
                .EnumerateArray()
                .Select(x => x.GetInt32())
                .ToHashSet();

            var verified = candidates.Where(c => ids.Contains(c.Craftsman.Id)).Take(needed).ToList();
            return verified.Count == 0 ? HardFilter(candidates, requiredService, needed) : verified;
        }
        catch
        {
            return HardFilter(candidates, requiredService, needed);
        }
    }

    private static List<RankedCraftsman> HardFilter(
        List<RankedCraftsman> candidates, string service, int needed)
        => candidates.Where(c => c.Craftsman.ServiceType == service).Take(needed).ToList();

    // ════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ════════════════════════════════════════════════════════════════════════

    private static List<string?> BuildCityQueue(string? city)
    {
        var queue = new List<string?>();
        if (city is not null)
        {
            queue.Add(city);
            if (NearbyMap.TryGetValue(city, out var nearby))
                queue.AddRange(nearby.Cast<string?>());
        }
        else queue.Add(null);
        return queue;
    }

    private static string BuildNearbyNote(
        string? city, string? service, int localCount, int nearbyCount)
    {
        if (nearbyCount == 0) return "";
        if (localCount == 0)
            return "لم أجد " + service + " في " + city + ".\nوجدت " + nearbyCount + " " + service + " من أقرب المحافظات.";
        return "وجدت " + localCount + " " + service + " في " + city + " فقط.\nأضفت " + nearbyCount + " " + service + " من المحافظات المجاورة.";
    }

    private static string BuildContext(
        List<FinalCraftsman> items, int localCount, string? nearbyNote)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(nearbyNote)) { sb.AppendLine("⚠️ " + nearbyNote); sb.AppendLine(); }
        sb.AppendLine("الحرفيون المتاحون:");
        sb.AppendLine("══════════════════");
        int i = 1;
        foreach (var fc in items)
        {
            var c = fc.Craftsman;
            string tag = fc.IsNearby ? " ★ من محافظة مجاورة" : "";
            sb.AppendLine($"[{i}] {c.User.Name}{tag}");
            sb.AppendLine($"    التخصص: {c.ServiceType} | المدينة: {c.City}");
            sb.AppendLine($"    الخبرة: {c.Experience} سنة | التقييم: {c.Rating}/5.0");
            sb.AppendLine($"    {c.Bio ?? ""}");
            sb.AppendLine();
            i++;
        }
        return sb.ToString();
    }

    private async Task<QueryResponse> SqlFallbackAsync(
        QueryRequest req, string? service, string? city, Stopwatch sw)
    {
        // ── 1. جلب الحرفيين من DB مع فلتر صحيح ──────────────────────────────
        var query = _db.Craftsmen
            .Include(c => c.User)
            .Where(c => !c.IsDeleted && c.IsApproved && c.IsAvailable)
            .Where(c => c.ServiceType != "AI")
            .Where(c => service == null || c.ServiceType == service);

        // لو فيه مدينة، حاول تجيب منها الأول
        List<Craftsman> top;
        if (!string.IsNullOrEmpty(city))
        {
            top = await query
                .Where(c => c.City == city)
                .OrderByDescending(c => c.Rating)
                .Take(req.TopK)
                .ToListAsync();

            // لو مفيش في المدينة دي، جرّب المحافظات المجاورة
            if (top.Count == 0)
            {
                var nearbyCities = NearbyMap.TryGetValue(city, out var nb) ? nb : [];
                if (nearbyCities.Length > 0)
                {
                    top = await query
                        .Where(c => nearbyCities.Contains(c.City))
                        .OrderByDescending(c => c.Rating)
                        .Take(req.TopK)
                        .ToListAsync();
                }
            }
        }
        else
        {
            top = await query
                .OrderByDescending(c => c.Rating)
                .Take(req.TopK)
                .ToListAsync();
        }

        sw.Stop();

        // ── 2. لو مفيش حرفيين خالص، رجّع رسالة واضحة بدون LLM ────────────────
        if (top.Count == 0)
        {
            string noResultMsg = string.IsNullOrEmpty(service)
                ? "عذراً، لا يوجد حرفيون متاحون في الوقت الحالي. 😔"
                : string.IsNullOrEmpty(city)
                    ? $"عذراً، لا يوجد حرفيون متاحون لخدمة \"{service}\" حالياً. 😔"
                    : $"عذراً، لا يوجد حرفيون متاحون لخدمة \"{service}\" في {city} أو المحافظات المجاورة حالياً. 😔\nجرّب محافظة أخرى أو تواصل معنا للمساعدة.";

            return new QueryResponse
            {
                Answer = noResultMsg,
                RetrievedCraftsmen = [],
                LatencyMs = sw.ElapsedMilliseconds
            };
        }

        // ── 3. لو فيه حرفيين، ولّد الإجابة من الـ LLM بناءً عليهم فقط ─────────
        string ctx = string.Join("\n\n", top.Select(c =>
            $"[{c.User.Name}] {c.ServiceType} — {c.City}\n" +
            $"خبرة {c.Experience}س | تقييم {c.Rating}/5\n{c.Bio ?? ""}"));

        string answer = await CallGroqAsync(req.Question, ctx, null);

        return new QueryResponse
        {
            Answer = answer,
            RetrievedCraftsmen = top.Select(c => new RetrievedCraftsmanDto
            {
                Id = c.Id,
                Name = c.User.Name,
                ServiceType = c.ServiceType,
                City = c.City,
                Neighborhood = c.Neighborhood,
                Rating = (double)c.Rating,
                ExperienceYears = c.Experience,
                RelevantText = (c.Bio ?? "")[..Math.Min(200, (c.Bio ?? "").Length)],
                SimilarityScore = (double)c.Rating / 5.0,
                IsNearby = false
            }).ToList(),
            LatencyMs = sw.ElapsedMilliseconds
        };
    }

    private async Task<string> CallGroqAsync(
        string question, string context, string? nearbyNote)
    {
        string extra = string.IsNullOrEmpty(nearbyNote)
            ? "" : "\nملاحظة: " + nearbyNote + " وضّح ذلك في إجابتك.\n";

        var payload = new
        {
            model = _config["Groq:ChatModel"] ?? "llama-3.3-70b-versatile",
            temperature = 0.1,
            max_tokens = 3000,
            messages = new[]
            {
                new
                {
                    role    = "system",
                    content = "أنت مساعد متخصص في إيجاد أفضل الحرفيين في مصر.\n" +
                              "قواعد:\n1. اعرض الحرفيين من القائمة فقط.\n" +
                              "2. اذكر: الاسم، التخصص، المدينة، الخبرة، التقييم.\n" +
                              "3. الإجابة بالعربية.\n4. لا تخترع معلومات.\n" +
                              "5. الحرفيين من المحافظة المطلوبة أولاً ثم المجاورة." + extra
                },
                new
                {
                    role    = "user",
                    content = context + "\n\nالطلب: " + question + "\n\nاعرض الحرفيين."
                }
            }
        };

        var resp = await _groqRotating.PostAsync("openai/v1/chat/completions", payload);
        if (!resp.IsSuccessStatusCode) return "عذراً، حدث خطأ في توليد الإجابة.";

        var result = await resp.Content.ReadFromJsonAsync<GroqResp>();
        return result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim()
               ?? "لم أتمكن من توليد إجابة.";
    }

    private static (string? service, string? city) DetectIntent(string q)
    {
        string? service = null;
        foreach (var (kw, svc) in KeywordMap)
            if (q.Contains(kw)) { service = svc; break; }
        string? city = AllCities.FirstOrDefault(c => q.Contains(c.ToLowerInvariant()));
        return (service, city);
    }

    private static string ExpandQuery(string original, string qLower)
    {
        foreach (var (trigger, extra) in Expansions)
            if (qLower.Contains(trigger)) return $"{original} {extra}";
        return original;
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

    private record RankedCraftsman(Harfi.Models.Entities.Craftsman Craftsman, double Score);
    private record FinalCraftsman(
        Harfi.Models.Entities.Craftsman Craftsman,
        double Score, bool IsNearby, string? RequestedCity);

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
    public async Task UpsertCraftsmanToVectorDbAsync(int craftsmanId)
    {
        var craftsman = await _repo.GetByIdAsync(craftsmanId);
        if (craftsman == null)
        {
            _logger.LogWarning("Craftsman {Id} not found for upsert", craftsmanId);
            return;
        }

        await _repo.LoadReferenceAsync(craftsman, c => c.User);
        var chunks = _chunker.ChunkCraftsman(craftsman);

        var texts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embedder.EmbedBatchAsync(texts);

        for (int i = 0; i < chunks.Count; i++)
            chunks[i].Embedding = embeddings[i];

        await _vectorDb.AddChunksAsync(chunks);
        _logger.LogInformation("Upserted Craftsman {Id} to Qdrant", craftsmanId);
    }

    public async Task DeleteCraftsmanFromVectorDbAsync(int craftsmanId)
    {
        var pointId = $"craftsman-{craftsmanId}";
        await _vectorDb.DeletePointAsync(pointId);
        _logger.LogInformation("Deleted Craftsman {Id} from Qdrant", craftsmanId);
    }

}