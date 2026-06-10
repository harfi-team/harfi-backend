using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using Harfi.DTOs.RAG;
using Harfi.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

    private static readonly (string Keyword, string Service)[] KeywordMap =
    [
        ("باب حديد","حداد"),("بوابة حديد","حداد"),("شباك حديد","حداد"),
        ("قضبان","حداد"),("درابزين حديد","حداد"),("سور حديد","حداد"),
        ("حداد","حداد"),("لحام حديد","حداد"),("باب جراج","حداد"),("باب معدن","حداد"),
        ("باب خشب","نجار"),("شباك خشب","نجار"),("دولاب خشب","نجار"),
        ("نجار","نجار"),("نجارة","نجار"),("أثاث","نجار"),("باركيه","نجار"),
        ("سرير مكسور","نجار"),("دولاب","نجار"),("باب بيعلق","نجار"),
        ("حنفية","سباك"),("صنبور","سباك"),("تسريب مياه","سباك"),
        ("مواسير","سباك"),("بالوعة","سباك"),("صرف صحي","سباك"),
        ("سخان مياه","سباك"),("خزان مياه","سباك"),("طلمبة مياه","سباك"),
        ("مرحاض","سباك"),("سباكة","سباك"),("سيفون","سباك"),
        ("كهرباء","كهربائي"),("سلك","كهربائي"),("لمبة","كهربائي"),
        ("قاطع","كهربائي"),("لوحة كهرباء","كهربائي"),("فيشة","كهربائي"),
        ("مقبس","كهربائي"),("تماس","كهربائي"),("إنارة","كهربائي"),
        ("طاقة شمسية","كهربائي"),("سولار","كهربائي"),("انفرتر","كهربائي"),
        ("تكييف","تكييف وتبريد"),("مكيف","تكييف وتبريد"),("فريون","تكييف وتبريد"),
        ("زجاج مكسور","زجاج وألمنيوم"),("شباك زجاج","زجاج وألمنيوم"),
        ("شباك ألمنيوم","زجاج وألمنيوم"),("ألمنيوم","زجاج وألمنيوم"),
        ("سيراميك","سيراميك"),("بلاطة","سيراميك"),("بلاط","سيراميك"),
        ("رخام","سيراميك"),("بورسلين","سيراميك"),
        ("دهان","دهان"),("بوية","دهان"),("طلاء","دهان"),("بلاستر","دهان"),
        ("تشقق","بناء"),("بناء","بناء"),("ترميم","بناء"),
        ("تشطيب","بناء"),("مقاول","بناء"),
        ("كاميرا","كاميرات مراقبة"),("مراقبة","كاميرات مراقبة"),("cctv","كاميرات مراقبة"),
        ("جبس","ديكور وجبس"),("ديكور","ديكور وجبس"),("كورنيش","ديكور وجبس"),
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
        ILogger<RAGService> logger)
    {
        _repo = repo;
        _chunker = chunker;
        _embedder = embedder;
        _vectorDb = vectorDb;
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

    // ════════════════════════════════════════════════════════════════════════
    //  QUERY
    // ════════════════════════════════════════════════════════════════════════

    public async Task<QueryResponse> QueryAsync(QueryRequest request)
    {
        var sw = Stopwatch.StartNew();
        string q = request.Question.ToLowerInvariant().Trim();

        var (detectedService, detectedCity) = DetectIntent(q);

        string? service = !string.IsNullOrEmpty(request.ExtractedService)
            ? request.ExtractedService : detectedService;

        string? city = !string.IsNullOrEmpty(request.ExtractedCity)
            ? request.ExtractedCity : detectedCity;

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

        var citiesToSearch = BuildCityQueue(city);
        var verifiedLocal = new List<FinalCraftsman>();
        var verifiedNearby = new List<FinalCraftsman>();
        var usedIds = new HashSet<int>();
        bool isFirstCity = true;

        foreach (var searchCity in citiesToSearch)
        {
            int totalSoFar = verifiedLocal.Count + verifiedNearby.Count;
            if (totalSoFar >= request.TopK) break;

            int needed = request.TopK - totalSoFar;

            QdrantSearchResponse qdrantResult;
            try
            {
                qdrantResult = await _vectorDb.SearchAsync(
                    qEmbed, needed * 5, service, searchCity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[Query] Qdrant failed for {C}: {M}", searchCity, ex.Message);
                isFirstCity = false;
                continue;
            }

            var qdrantScores = (qdrantResult.result ?? [])
                .Select(p => new { Id = GetInt(p.payload, "craftsman_id"), Score = p.score })
                .Where(x => x.Id != 0 && !usedIds.Contains(x.Id))
                .GroupBy(x => x.Id)
                .Select(g => g.OrderByDescending(x => x.Score).First())
                .OrderByDescending(x => x.Score)
                .Take(needed * 2)
                .ToList();

            if (qdrantScores.Count == 0) { isFirstCity = false; continue; }

            var ids = qdrantScores.Select(x => x.Id).ToList();
            var craftsmen = await _repo.FindAsync(c => ids.Contains(c.Id));
            var craftsmanMap = craftsmen.ToDictionary(c => c.Id);

            var ranked = qdrantScores
                .Where(x => craftsmanMap.ContainsKey(x.Id))
                .Select(x => new RankedCraftsman(craftsmanMap[x.Id], x.Score))
                .ToList();

            var verified = await VerifyServiceTypeAsync(ranked, service, needed);

            foreach (var v in verified)
            {
                usedIds.Add(v.Craftsman.Id);
                double fused = 0.7 * v.Score + 0.3 * ((double)v.Craftsman.Rating / 5.0);
                var fc = new FinalCraftsman(v.Craftsman, fused, !isFirstCity, city);
                if (isFirstCity) verifiedLocal.Add(fc);
                else verifiedNearby.Add(fc);
            }

            isFirstCity = false;
        }

        var allVerified = verifiedLocal.Concat(verifiedNearby).ToList();
        if (allVerified.Count == 0)
            return await SqlFallbackAsync(request, service, city, sw);

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
        var top = (await _repo.GetAllAsync())
            .Where(c => service is null || c.ServiceType == service)
            .OrderByDescending(c => c.Rating)
            .Take(req.TopK)
            .ToList();

        string ctx = string.Join("\n\n", top.Select(c =>
            $"[{c.User.Name}] {c.ServiceType} — {c.City}\n" +
            $"خبرة {c.Experience}س | تقييم {c.Rating}/5\n{c.Bio ?? ""}"));

        string answer = await CallGroqAsync(req.Question, ctx, null);
        sw.Stop();

        return new QueryResponse
        {
            Answer = answer,
            RetrievedCraftsmen = top.Select(c => new RetrievedCraftsmanDto
            {
                Id = c.Id,
                Name = c.User.Name,
                ServiceType = c.ServiceType,
                City = c.City,
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
}