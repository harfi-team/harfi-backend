
using Harfi.DTOs.RAG;
using Harfi.Models.Constants;
using Harfi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harfi.Repositories.Data;

public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        AppDbContext context,
        UserManager<User> userManager,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync())
            return;

        await SeedAdminAsync();
        await SeedAiUserAsync();
        await SeedCraftsmanUsersAsync();
        await SeedCustomerUsersAsync();
        await SeedCraftsmanProfilesAsync();
        await SeedJobsAsync();
        await SeedCommonProblemsAsJobsAsync();
        await SeedReviewsAsync();
        await RecalculateRatingsAsync();
        await SeedConversationsAsync();
        await SeedMessagesAsync();
        SeedStatus.IsCompleted = true;

    }

    private async Task ClearAllDataAsync()
    {
        _logger.LogInformation("Clearing all existing data...");

        await _context.JobFeedbacks.ExecuteDeleteAsync();
        await _context.AIChatMessages.ExecuteDeleteAsync();
        await _context.MediaFiles.ExecuteDeleteAsync();
        await _context.Notifications.ExecuteDeleteAsync();
        await _context.Messages.ExecuteDeleteAsync();
        await _context.Conversations.ExecuteDeleteAsync();
        await _context.Reviews.ExecuteDeleteAsync();
        await _context.RAGDocuments.ExecuteDeleteAsync();
        await _context.Jobs.ExecuteDeleteAsync();
        await _context.Craftsmen.ExecuteDeleteAsync();
        await _context.RefreshTokens.ExecuteDeleteAsync();

        var allUsers = await _context.Users.ToListAsync();
        foreach (var user in allUsers)
            await _userManager.DeleteAsync(user);

        // ── RESEED كل الـ IDENTITY columns من 0 ──────────────────
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Jobs', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Craftsmen', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Reviews', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Conversations', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Messages', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('RAGDocuments', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('JobFeedbacks', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('AIChatMessages', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Notifications', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('RefreshTokens', RESEED, 0)");
        await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Users', RESEED, 0)");
        _logger.LogInformation("All data cleared. Starting fresh seed...");
    }
    // ───────────────────────────────────────────────────────────
    //  ADMIN
    // ───────────────────────────────────────────────────────────
    private async Task SeedAdminAsync()
    {
        var admin = new User
        {
            UserName = "admin@harfi.com",
            Name = "Harfi Admin",
            Email = "admin@harfi.com",
            Role = "admin",
            Phone = "01000000000",
            IsActive = true,
            IsVerified = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };

        var result = await _userManager.CreateAsync(admin, "Admin@1234");
        if (result.Succeeded)
            _logger.LogInformation("Admin seeded: admin@harfi.com / Admin@1234");
        else
            _logger.LogWarning("Failed to seed admin: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    // ───────────────────────────────────────────────────────────
    //  AI USER + CRAFTSMAN (required for AI fallback)
    // ───────────────────────────────────────────────────────────
    private async Task SeedAiUserAsync()
    {
        var aiUser = new User
        {
            UserName = "ai@harfi.com",
            Name = "Harfi AI",
            Email = "ai@harfi.com",
            Role = "craftsman",
            Phone = "00000000000",
            IsActive = true,
            IsVerified = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddYears(-1)
        };

        var result = await _userManager.CreateAsync(aiUser, "HarfiAI@2024");
        if (result.Succeeded)
        {
            _context.Craftsmen.Add(new Craftsman
            {
                UserId = aiUser.Id,
                ServiceType = "AI",
                City = "AI",
                Experience = 99,
                IsApproved = true,
                IsAvailable = false,
                Rating = 0,
                Bio = "Harfi AI Assistant",
                NationalIdUrl = "/uploads/ids/ai.jpg",
                CreatedAt = DateTime.UtcNow.AddYears(-1)
            });
            await _context.SaveChangesAsync();
            _logger.LogInformation("AI user + craftsman seeded: ai@harfi.com");
        }
        else
            _logger.LogWarning("Failed to seed AI user: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    // ───────────────────────────────────────────────────────────
    //  CRAFTSMAN USERS (10)
    // ───────────────────────────────────────────────────────────
    private async Task SeedCraftsmanUsersAsync()
    {
        var craftsmanData = new[]
        {
        (name: "أحمد علي", phone: "01012345678", email: "ahmed.ali@gmail.com"),
        (name: "محمد حسن", phone: "01123456789", email: "mohamed.hassan@gmail.com"),
        (name: "عبدالله خالد", phone: "01234567890", email: "abdallah.khaled@gmail.com"),
        (name: "مصطفى محمود", phone: "01512345678", email: "mostafa.mahmoud@gmail.com"),
        (name: "حسين رضا", phone: "01098765432", email: "hussien.reda@gmail.com"),
        (name: "كريم سامي", phone: "01156789012", email: "kareem.samy@gmail.com"),
        (name: "يوسف عادل", phone: "01234561234", email: "youssef.adel@gmail.com"),
        (name: "إبراهيم نصر", phone: "01567890123", email: "ibrahim.nasr@gmail.com"),
        (name: "عمرو شريف", phone: "01023456789", email: "amr.sherif@gmail.com"),
        (name: "خالد أحمد", phone: "01134567890", email: "khaled.ahmed@gmail.com"),
        (name: "محمود السيد", phone: "01011111111", email: "mahmoud.elsayed@gmail.com"),
        (name: "طارق إبراهيم", phone: "01022222222", email: "tarek.ibrahim@gmail.com"),
        (name: "أشرف رمضان", phone: "01033333333", email: "ashraf.ramadan@gmail.com"),
        (name: "وليد محمد", phone: "01044444444", email: "walid.mohamed@gmail.com"),
        (name: "سيد عبدالسلام", phone: "01055555555", email: "sayed.abdelsalam@gmail.com"),
        (name: "هاني عبدالعزيز", phone: "01066666666", email: "hani.abdelaziz@gmail.com"),
        (name: "شريف فؤاد", phone: "01077777777", email: "sherif.fouad@gmail.com"),
        (name: "رامي مجدي", phone: "01088888888", email: "ramy.magdy@gmail.com"),
        (name: "مينا جورج", phone: "01099999999", email: "mina.george@gmail.com"),
        (name: "علاء السيد", phone: "01111111111", email: "alaa.elsayed@gmail.com"),
        (name: "إسلام أحمد", phone: "01122222222", email: "eslam.ahmed@gmail.com"),
        (name: "محمد شوقي", phone: "01133333333", email: "mohamed.shawky@gmail.com"),
        (name: "أحمد السيد", phone: "01144444444", email: "ahmed.elsayed@gmail.com"),
        (name: "ياسر جمال", phone: "01155555555", email: "yasser.gamal@gmail.com"),
        (name: "محمود صبحي", phone: "01166666666", email: "mahmoud.sobhy@gmail.com"),
        (name: "أمير رمضان", phone: "01177777777", email: "ameer.ramadan@gmail.com"),
        (name: "عمر خالد", phone: "01188888888", email: "omar.khaled@gmail.com"),
        (name: "أحمد مجدي", phone: "01199999999", email: "ahmed.magdy@gmail.com"),
        (name: "خالد السيد", phone: "01211111111", email: "khaled.elsayed@gmail.com"),
        (name: "مروان عادل", phone: "01222222222", email: "marwan.adel@gmail.com"),
        (name: "محمد عادل", phone: "01233333333", email: "mohamed.adel@gmail.com"),
        (name: "أحمد رمضان", phone: "01244444444", email: "ahmed.ramadan@gmail.com"),
        (name: "مصطفى السيد", phone: "01255555555", email: "mostafa.elsayed@gmail.com"),
        (name: "كريم محمود", phone: "01266666666", email: "kareem.mahmoud@gmail.com"),
        (name: "إبراهيم عادل", phone: "01277777777", email: "ibrahim.adel@gmail.com"),
        (name: "شادي محمد", phone: "01288888888", email: "shady.mohamed@gmail.com")  // 36 users
    };

        var baseDate = DateTime.UtcNow.AddMonths(-6);

        for (int i = 0; i < craftsmanData.Length; i++)
        {
            var d = craftsmanData[i];
            var user = new User
            {
                UserName = d.email,
                Name = d.name,
                Email = d.email,
                Role = "craftsman",
                Phone = d.phone,
                IsActive = true,
                IsVerified = true,
                EmailConfirmed = true,
                CreatedAt = baseDate.AddDays(i * 18)
            };

            var result = await _userManager.CreateAsync(user, "Harfi@2024");
            if (!result.Succeeded)
                _logger.LogWarning("Failed to create craftsman user {Email}: {Errors}",
                    d.email, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("36 craftsman users seeded (Cairo: 10, other governorates: 26).");
    }

    // ───────────────────────────────────────────────────────────
    //  CUSTOMER USERS (5)
    // ───────────────────────────────────────────────────────────
    private async Task SeedCustomerUsersAsync()
    {
        var customerData = new[]
        {
            (name: "سارة أحمد",       phone: "01245678901", email: "sara.ahmed@gmail.com"),
            (name: "نورهان محمد",     phone: "01567890124", email: "nourhan.mohamed@gmail.com"),
            (name: "مريم علي",        phone: "01067890123", email: "maryam.ali@gmail.com"),
            (name: "فاطمة حسن",       phone: "01178901234", email: "fatma.hassan@gmail.com"),
            (name: "منة الله خالد",   phone: "01289012345", email: "mennatallah.khaled@gmail.com")
        };

        var baseDate = DateTime.UtcNow.AddMonths(-5).AddDays(15);

        for (int i = 0; i < customerData.Length; i++)
        {
            var d = customerData[i];
            var user = new User
            {
                UserName = d.email,
                Name = d.name,
                Email = d.email,
                Role = "customer",
                Phone = d.phone,
                IsActive = true,
                IsVerified = true,
                EmailConfirmed = true,
                CreatedAt = baseDate.AddDays(i * 14)
            };

            var result = await _userManager.CreateAsync(user, "Harfi@2024");
            if (!result.Succeeded)
                _logger.LogWarning("Failed to create customer user {Email}: {Errors}",
                    d.email, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("5 customer users seeded.");
    }

  

    private async Task SeedCraftsmanProfilesAsync()
    {
        var users = await _context.Users
            .Where(u => u.Role == "craftsman" && u.Email != "ai@harfi.com")
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        // 36 profiles: first 10 for Cairo (2 per trade), then 26 other governorates (1 each)
        var profilesRaw = new (int idx, string serviceType, string city, string? neighborhood,
            decimal? min, decimal? max, int exp, decimal rating, string? bio, string photoId)[]
        {
        // ======== القاهرة (2 لكل حرفة) ========
        (0, "سباك", "القاهرة , مدينة نصر , شارع عباس العقاد", null, 150m, 300m, 8, 4.7m, "سباك محترف", "1"),
        (1, "سباك", "القاهرة , مصر الجديدة , شارع الميرغني", null, 180m, 350m, 10, 4.6m, "معلم سباكة", "2"),
        (2, "كهربائي", "القاهرة , مدينة نصر , شارع مكرم عبيد", null, 250m, 500m, 11, 4.8m, "كهربائي منازل", "3"),
        (3, "كهربائي", "القاهرة , مصر الجديدة , شارع الحجاز", null, 220m, 450m, 8, 4.5m, "فني كهرباء", "4"),
        (4, "نجار", "القاهرة , مدينة نصر , شارع الطيران", null, 300m, 700m, 15, 4.9m, "نجار أثاث", "5"),
        (5, "نجار", "القاهرة , مصر الجديدة , شارع الثورة", null, 350m, 800m, 13, 4.8m, "نجار ديكورات", "6"),
        (6, "فني تكييف", "القاهرة , مدينة نصر , شارع مصطفى النحاس", null, 350m, 800m, 12, 4.8m, "فني تكييف", "7"),
        (7, "فني تكييف", "القاهرة , مصر الجديدة , شارع النزهة", null, 300m, 700m, 9, 4.5m, "صيانة تكييفات", "8"),
        (8, "نقاش", "القاهرة , مدينة نصر , شارع عباس العقاد", null, 200m, 500m, 10, 4.7m, "نقاش محترف", "9"),
        (9, "نقاش", "القاهرة , مصر الجديدة , شارع الميرغني", null, 180m, 450m, 7, 4.3m, "دهانات وديكورات", "10"),

        // ======== باقي المحافظات (26 محافظة، كل محافظة حرفي واحد) ========
        (10, "نجار", "الجيزة , الدقي , شارع التحرير", null, 300m, 600m, 12, 4.5m, "نجار موبيليا", "11"),
        (11, "سباك", "الإسكندرية , سموحة , شارع فوزي معاذ", null, 200m, 400m, 10, 4.8m, "سباك محترف", "12"),
        (12, "كهربائي", "كفر الشيخ , كفر الشيخ , شارع الجمهورية", null, 220m, 500m, 8, 4.5m, "كهربائي", "13"),
        (13, "فني تكييف", "البحيرة , دمنهور , شارع الجمهورية", null, 300m, 700m, 9, 4.6m, "فني تكييف", "14"),
        (14, "نقاش", "الغربية , طنطا , شارع البحر", null, 180m, 450m, 7, 4.3m, "نقاش", "15"),
        (15, "سباك", "الدقهلية , المنصورة , شارع الجيش", null, 170m, 340m, 7, 4.5m, "سباك", "16"),
        (16, "كهربائي", "الشرقية , الزقازيق , شارع أحمد عرابي", null, 250m, 520m, 11, 4.8m, "كهربائي", "17"),
        (17, "نجار", "المنوفية , شبين الكوم , شارع الاستاد", null, 280m, 620m, 9, 4.6m, "نجار", "18"),
        (18, "فني تكييف", "القليوبية , بنها , شارع الجمهورية", null, 320m, 720m, 9, 4.5m, "فني تكييف", "19"),
        (19, "سباك", "المنيا , المنيا , شارع كورنيش النيل", null, 160m, 320m, 7, 4.3m, "سباك", "20"),
        (20, "كهربائي", "أسوان , أسوان , شارع السد العالي", null, 250m, 550m, 11, 4.7m, "كهربائي", "21"),
        (21, "نجار", "بورسعيد , بورسعيد , شارع 23 يوليو", null, 270m, 600m, 10, 4.6m, "نجار", "22"),
        (22, "نقاش", "السويس , السويس , شارع الجيش", null, 190m, 460m, 7, 4.3m, "نقاش", "23"),
        (23, "سباك", "دمياط , دمياط , شارع الجلاء", null, 150m, 300m, 8, 4.4m, "سباك", "24"),
        (24, "كهربائي", "سوهاج , سوهاج , شارع النيل", null, 220m, 480m, 9, 4.5m, "كهربائي", "25"),
        (25, "نجار", "قنا , قنا , شارع الجمهورية", null, 260m, 580m, 8, 4.4m, "نجار", "26"),
        (26, "فني تكييف", "الأقصر , الأقصر , شارع الكرنك", null, 320m, 750m, 9, 4.6m, "فني تكييف", "27"),
        (27, "سباك", "البحر الأحمر , الغردقة , شارع الشيراتون", null, 200m, 400m, 8, 4.5m, "سباك", "28"),
        (28, "كهربائي", "الوادي الجديد , الخارجة , شارع الجمهورية", null, 220m, 500m, 9, 4.4m, "كهربائي", "29"),
        (29, "نجار", "مطروح , مرسى مطروح , شارع اسكندرية", null, 250m, 550m, 8, 4.3m, "نجار", "30"),
        (30, "نقاش", "شمال سيناء , العريش , شارع فلسطين", null, 180m, 420m, 7, 4.2m, "نقاش", "31"),
        (31, "سباك", "جنوب سيناء , شرم الشيخ , شارع السلام", null, 200m, 450m, 9, 4.6m, "سباك", "32"),
        (32, "كهربائي", "الفيوم , الفيوم , شارع النيل", null, 220m, 480m, 8, 4.4m, "كهربائي", "33"),
        (33, "نجار", "بني سويف , بني سويف , شارع الأهرام", null, 260m, 600m, 9, 4.5m, "نجار", "34"),
        (34, "فني تكييف", "الإسماعيلية , الإسماعيلية , شارع الجمهورية", null, 300m, 700m, 9, 4.6m, "فني تكييف", "35"),
        (35, "نقاش", "القليوبية , شبرا الخيمة , شارع النصر", null, 190m, 460m, 7, 4.3m, "نقاش", "36")
        };

        int added = 0;
        foreach (var p in profilesRaw)
        {
            if (p.idx >= users.Count)
                break;

            var user = users[p.idx];
            _context.Craftsmen.Add(new Craftsman
            {
                UserId = user.Id,
                ServiceType = p.serviceType,
                City = p.city,
                Neighborhood = p.neighborhood,
                PriceRangeMin = p.min,
                PriceRangeMax = p.max,
                Experience = p.exp,
                IsApproved = true,
                IsAvailable = true,
                Rating = p.rating,
                Bio = p.bio,
                NationalIdUrl = $"/uploads/ids/id_{p.photoId}.jpg",
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            });
            added++;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("{Count} craftsman profiles seeded (Cairo: 2 per trade, other 26 governorates: 1 each).", added);
    }
    // ───────────────────────────────────────────────────────────
    //  JOBS (20) — all completed
    // ───────────────────────────────────────────────────────────
    private async Task SeedJobsAsync()
    {
        var craftsmen = await _context.Craftsmen
            .Where(c => c.ServiceType != "AI")
            .OrderBy(c => c.Id)
            .ToListAsync();
        var customers = await _context.Users
            .Where(u => u.Role == "customer")
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        if (craftsmen.Count == 0 || customers.Count == 0)
        {
            _logger.LogWarning("No craftsmen or customers found. Jobs not seeded.");
            return;
        }

        var baseDate = DateTime.UtcNow.AddMonths(-3);

        var jobData = new (int craftsmanIdx, int customerIdx, string description, string address,
    string? problemDesc, string? solutionDesc)[]
    {
(0, 0,
"الحنفية في المطبخ بتنقط مياه باستمرار حتى بعد القفل",
"15 شارع الجيش، مدينة نصر، القاهرة",
"اشتكى العميل من تنقيط مستمر للمياه أدى إلى زيادة فاتورة الاستهلاك. لوحظ وجود ترسبات جيرية وتآكل في الجلدة الداخلية للحنفية.",
"تم غلق مصدر المياه وفك رأس الحنفية بالكامل وتنظيف الرواسب الجيرية واستبدال الجلدة التالفة ثم إعادة التركيب واختبار الحنفية للتأكد من توقف التنقيط."),

(0, 1,
"المياه مش بتنزل من حوض المطبخ وبتترجع تاني",
"8 شارع الطيران، مدينة نصر، القاهرة",
"انسداد شديد في صرف المطبخ بسبب تراكم الدهون وبقايا الطعام داخل المواسير والسيفون.",
"تم فك السيفون وتنظيفه بالكامل واستخدام معدات تسليك احترافية ثم غسل المواسير بالمياه المضغوطة واختبار التصريف."),

(1, 2,
"سخان المياه الكهربائي بيفصل بعد دقائق من التشغيل",
"22 شارع 9، المعادي، القاهرة",
"السخان يسخن المياه لفترة قصيرة ثم يتوقف. تم اكتشاف ضعف في عنصر التسخين وخلل في الثرموستات.",
"تم استبدال عنصر التسخين وضبط الثرموستات وتنظيف الرواسب الكلسية واختبار السخان على عدة دورات تشغيل."),

(2, 3,
"فيه ريحة صرف قوية في الحمام مع تسريب مياه",
"12 شارع أبو بكر، الزيتون، القاهرة",
"ظهور روائح كريهة وتسريب حول قاعدة الحمام نتيجة تلف الجلدة العازلة وضعف إحكام الوصلات.",
"تم فك القاعدة واستبدال الجلدة وإعادة تركيب الوصلات واختبار الصرف والتأكد من اختفاء التسريب والروائح."),

(3, 4,
"القاطع الكهربائي بيفصل أول ما أشغل التكييف",
"18 شارع شبرا، القاهرة",
"فصل متكرر للكهرباء عند تشغيل الأحمال العالية بسبب زيادة الحمل وتلف القاطع الرئيسي.",
"تم قياس الأحمال الكهربائية واستبدال القاطع وإعادة توزيع الأحمال واختبار الدائرة بالكامل."),

(4, 0,
"فيه شرارة طالعة من مفتاح النور في الصالة",
"25 شارع الحجاز، مصر الجديدة، القاهرة",
"وجود تماس كهربائي داخل المفتاح بسبب احتراق نقاط التوصيل الداخلية.",
"تم فصل التيار واستبدال المفتاح وفحص الأسلاك وعزل الأجزاء المتضررة واختبار التشغيل."),

(5, 1,
"باب غرفة النوم بيحك في الأرض ومش بيتقفل كويس",
"6 شارع العباسية، القاهرة",
"هبوط في مستوى الباب نتيجة ارتخاء المفصلات وتآكل بعض المسامير.",
"تم ضبط المفصلات واستبدال المسامير وإعادة اتزان الباب والتأكد من سهولة الفتح والغلق."),

(6, 2,
"الدولاب أبوابه مفكوكة والأدراج مش بتتحرك",
"14 شارع رمسيس، العباسية، القاهرة",
"تلف في المفصلات والسحابات مع ضعف تثبيت بعض الأجزاء الخشبية.",
"تم استبدال المفصلات والسحابات وتقوية الهيكل الخشبي وضبط الأبواب والأدراج."),

(7, 3,
"التكييف شغال لكن التبريد ضعيف جداً",
"20 شارع حلوان، القاهرة",
"انخفاض مستوى الفريون مع تراكم الأتربة على الفلاتر والوحدة الداخلية.",
"تم تنظيف الفلاتر وشحن الفريون وفحص الضغوط واختبار كفاءة التبريد."),

(8, 4,
"التكييف بينزل مياه على الحائط",
"11 شارع الملك فيصل، الجيزة",
"انسداد خط صرف التكثيف أدى إلى رجوع المياه للوحدة الداخلية.",
"تم تنظيف خط الصرف وإزالة الانسداد وفحص مستوى تركيب الوحدة وتشغيل الجهاز للتأكد من حل المشكلة."),

(9, 0,
"الحيطان فيها شروخ وتقشير في الدهان",
"28 شارع الهرم، الجيزة",
"وجود شروخ سطحية وتقشر في طبقات الدهان بسبب الرطوبة وسوء التجهيز السابق.",
"تم معالجة الشروخ وصنفرة الحوائط ووضع طبقة معجون ثم تنفيذ دهان جديد."),

(10, 1,
"شباك خشب مش بيقفل بسبب الرطوبة",
"10 شارع الجمهورية، كفر الشيخ",
"انتفاخ أجزاء من الخشب نتيجة تعرضها للرطوبة لفترات طويلة.",
"تم معالجة الخشب وبرد الأجزاء المتأثرة وضبط المفصلات والقفل."),

(11, 2,
"ضغط المياه ضعيف جداً في الشقة",
"15 شارع سعد زغلول، المنصورة",
"ضعف تدفق المياه من جميع الحنفيات بسبب انسداد الفلاتر وضعف الطلمبة.",
"تم تنظيف الفلاتر وفحص الطلمبة وضبط ضغط التشغيل واختبار جميع المخارج."),

(12, 3,
"الريموت مش بيشغل التكييف",
"7 شارع البحر، طنطا",
"عدم استجابة التكييف للأوامر بسبب عطل في وحدة استقبال الإشارة.",
"تم استبدال وحدة الاستقبال واختبار الريموت وإعادة ضبط الإعدادات."),

(13, 4,
"احتراق بريز المطبخ عند تشغيل الأجهزة",
"9 شارع التحرير، دمنهور",
"ارتفاع حرارة البريز نتيجة حمل زائد وضعف التوصيلات الداخلية.",
"تم استبدال البريز وفحص الأسلاك وشد جميع التوصيلات واختبار الأحمال.")


};


        for (int i = 0; i < jobData.Length; i++)
        {
            var j = jobData[i];
            var jobDate = baseDate.AddDays(i * 4);
            var completedDays = (i % 7) + 1;

            // التعديل هنا: استخدام modulo لضمان عدم خروج المؤشر عن النطاق
            int craftsmanIdx = j.craftsmanIdx % craftsmen.Count;
            int customerIdx = j.customerIdx % customers.Count;

            _context.Jobs.Add(new Job
            {
                CustomerId = customers[customerIdx].Id,
                CraftsmanId = craftsmen[craftsmanIdx].Id,
                Status = JobStatusConstants.Done,
                ServiceType = craftsmen[craftsmanIdx].ServiceType,
                Description = j.description,
                Address = j.address,
                ProblemDescription = j.problemDesc,
                SolutionDescription = j.solutionDesc,
                CreatedAt = jobDate,
                CompletedAt = jobDate.AddDays(completedDays)
            });
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("{Count} jobs seeded.", jobData.Length);
    }
    // ───────────────────────────────────────────────────────────
    //  COMMON PROBLEMS → JOBS (زيادة داتا الـ RAG)
    // ─────────
    private async Task SeedCommonProblemsAsJobsAsync()
    {
        var craftsmen = await _context.Craftsmen
            .Where(c => c.ServiceType != "AI")
            .OrderBy(c => c.Id)
            .ToListAsync();

        var customers = await _context.Users
            .Where(u => u.Role == "customer")
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        if (craftsmen.Count == 0 || customers.Count == 0)
        {
            _logger.LogWarning("No craftsmen or customers found. Common problem jobs not seeded.");
            return;
        }

        var baseDate = DateTime.UtcNow.AddMonths(-2);

        var problemJobs = new (string serviceType, string description,
 string address, string problemDesc, string solutionDesc)[]
 {
("سباك",
"الحنفية بتنقط مياه باستمرار",
"القاهرة",
"تنقيط مستمر من الحنفية بعد الإغلاق. قد يكون السبب تلف الجلدة أو الرواسب الجيرية أو تآكل قلب الحنفية.",
"فحص مصدر التسريب ثم استبدال الجلدة أو قلب الحنفية وتنظيف الرواسب وإعادة اختبار التشغيل."),

("سباك",
"انسداد حوض المطبخ",
"القاهرة",
"بطء أو توقف تصريف المياه بسبب تراكم الدهون وبقايا الطعام داخل السيفون أو المواسير.",
"تنظيف السيفون وتسليك المواسير وغسلها بالمياه الساخنة أو المضغوطة."),

("كهربائي",
"القاطع الكهربائي بيفصل باستمرار",
"الجيزة",
"فصل متكرر للكهرباء بسبب حمل زائد أو قصر كهربائي أو تلف القاطع.",
"قياس الأحمال وفحص الدوائر واستبدال القاطع إذا لزم الأمر."),

("كهربائي",
"شرارة من مفتاح الكهرباء",
"القاهرة",
"ظهور شرر أو رائحة احتراق نتيجة ضعف التوصيلات أو احتراق نقاط التلامس.",
"فصل الكهرباء واستبدال المفتاح وفحص الأسلاك المرتبطة به."),

("فني تكييف",
"التكييف لا يبرد",
"القاهرة",
"ضعف التبريد بسبب نقص الفريون أو اتساخ الفلاتر أو مشكلة بالمكثف.",
"تنظيف الفلاتر وقياس ضغط الفريون وفحص الوحدة الداخلية والخارجية."),

("فني تكييف",
"نزول مياه من التكييف",
"الجيزة",
"تسرب مياه من الوحدة الداخلية نتيجة انسداد خط الصرف أو عدم توازن الوحدة.",
"تنظيف خط الصرف وضبط مستوى الوحدة واختبار التشغيل."),

("نجار",
"باب لا يغلق بشكل صحيح",
"الإسكندرية",
"احتكاك الباب بالأرض أو الحلق بسبب هبوط المفصلات أو تمدد الخشب.",
"ضبط المفصلات أو استبدالها ومعالجة الأجزاء المتضررة."),

("نجار",
"أبواب الدولاب مفكوكة",
"المنصورة",
"ضعف المفصلات أو تلف أماكن التثبيت يؤدي إلى عدم إغلاق الأبواب.",
"استبدال المفصلات وتقوية نقاط التثبيت وإعادة ضبط الأبواب."),

("نقاش",
"تقشر الدهان",
"القاهرة",
"انفصال طبقات الدهان بسبب الرطوبة أو سوء تجهيز الحائط.",
"إزالة الطبقات التالفة ومعالجة السبب ثم إعادة الدهان."),

("نقاش",
"شروخ في الحائط",
"الجيزة",
"ظهور شروخ سطحية أو متوسطة نتيجة الانكماش أو الرطوبة.",
"فتح الشروخ ومعالجتها بمواد مناسبة ثم إعادة التشطيب والدهان.")


};


        int idx = 0;
        foreach (var p in problemJobs)
        {
            // التعديل هنا: استخدام modulo على craftsmen و customers
            var craftsman = craftsmen.FirstOrDefault(c => c.ServiceType == p.serviceType)
                            ?? craftsmen[idx % craftsmen.Count];
            var customer = customers[idx % customers.Count];
            var jobDate = baseDate.AddDays(idx * 3);

            _context.Jobs.Add(new Job
            {
                CustomerId = customer.Id,
                CraftsmanId = craftsman.Id,
                Status = JobStatusConstants.Done,
                ServiceType = p.serviceType,
                Description = p.description,
                Address = p.address,
                ProblemDescription = p.problemDesc,
                SolutionDescription = p.solutionDesc,
                CreatedAt = jobDate,
                CompletedAt = jobDate.AddDays(1)
            });

            idx++;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("{Count} common problem jobs seeded.", problemJobs.Length);
    }

    // ───────────────────────────────────────────────────────────
    //  REVIEWS (15 — one per job, for first 15 jobs)
    // ───────────────────────────────────────────────────────────
    private async Task SeedReviewsAsync()
    {
        var jobs = await _context.Jobs
            .Where(j => j.Status == JobStatusConstants.Done)
            .OrderBy(j => j.Id)
            .Take(15)
            .ToListAsync();

        var reviewData = new (int stars, string comment)[]
        {
            (5, "شغل ممتاز ونظيف جداً، الأستاذ محترم وسريع في الشغل"),
            (4, "شغل كويس بس أتأخر شوية على الموعد"),
            (5, "أحسن سباك تعاملت معاه، شغل نضيف وفي الموعد"),
            (4, "الأستاذ خلص الشغل زي ما اتفقنا، جودة ممتازة"),
            (3, "الشغل اتعمل بس كان في بعض المشاكل في الأول"),
            (5, "ممتاز جداً، أنصح بالتعامل معاه بثقة"),
            (4, "شغل محترم وسعر مناسب، هكلمه تاني أكيد"),
            (5, "فنان في شغله، تعامل محترم ونظيف"),
            (3, "محتاج يهتم شوية بالتفاصيل لكن في النهاية تمام"),
            (5, "أخلاق عالية وشغل هايل، ربنا يبارك له"),
            (4, "ممتاز، التزم بالوقت والسعر المتفق عليه"),
            (2, "الشغل ماشي لكن في حاجات ناقصة محتاج يرجع يظبطها"),
            (5, "أفضل حرفي اشتغلت معاه، محترف وشغله نضيف"),
            (4, "خلص الشغل بسرعة وجودة كويسة الحمد لله"),
            (5, "شغل فخم الصراحة، أسعاره مناسبة جداً")
        };

        for (int i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];
            var review = reviewData[i];

            _context.Reviews.Add(new Review
            {
                JobId = job.Id,
                CustomerId = job.CustomerId,
                CraftsmanId = job.CraftsmanId!.Value,
                Stars = review.stars,
                Comment = review.comment,
                CreatedAt = job.CompletedAt ?? job.CreatedAt.AddDays(1)
            });
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("15 reviews seeded.");
    }

    // ───────────────────────────────────────────────────────────
    //  RECALCULATE RATINGS
    // ───────────────────────────────────────────────────────────
    private async Task RecalculateRatingsAsync()
    {
        var craftsmen = await _context.Craftsmen
            .Include(c => c.Reviews)
            .ToListAsync();

        foreach (var craftsman in craftsmen)
        {
            craftsman.Rating = craftsman.Reviews.Count != 0
                ? (decimal)Math.Round(craftsman.Reviews.Average(r => r.Stars), 2)
                : 0m;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Ratings recalculated.");
    }

    // ───────────────────────────────────────────────────────────
    //  CONVERSATIONS (10 — one per job for first 10 jobs)
    // ───────────────────────────────────────────────────────────
    private async Task SeedConversationsAsync()
    {
        var jobs = await _context.Jobs
            .Where(j => j.CraftsmanId != null)
            .OrderBy(j => j.Id)
            .Take(10)
            .ToListAsync();

        foreach (var job in jobs)
        {
            _context.Conversations.Add(new Conversation
            {
                JobId = job.Id,
                CustomerId = job.CustomerId,
                CraftsmanId = job.CraftsmanId!.Value,
                CreatedAt = job.CreatedAt,
                LastMessageAt = job.CreatedAt.AddHours(2)
            });
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("10 conversations seeded.");
    }

    // ───────────────────────────────────────────────────────────
    //  MESSAGES (40 across 10 conversations, 4 each)
    // ───────────────────────────────────────────────────────────
    private async Task SeedMessagesAsync()
    {
        var conversations = await _context.Conversations
            .OrderBy(c => c.Id)
            .Include(c => c.Job)
            .ToListAsync();

        var craftsmanUserIds = await _context.Craftsmen
            .Where(c => conversations.Select(cv => cv.CraftsmanId).Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.UserId);

        var messageData = new Dictionary<int, (string content, int minutesOffset)[]>
        {
            [0] = new[]
            {
                ("أهلاً، محتاج حد يصلح تسريب في الحمام", 0),
                ("أهلاً بيك، امتى تقدر أستقبل؟", 10),
                ("بكرة الصبح مناسب؟", 25),
                ("تمام، هكون عندك الساعة 10 الصبح", 40),
            },
            [1] = new[]
            {
                ("المواسير في المطبخ مسدودة خالص من الصبح", 5),
                ("طب جربت تسليكها بالخلطة العادية؟", 20),
                ("جربت كذا حاجة ومفيش فايدة", 35),
                ("تمام، هجيلك النهارده بعد العصر", 50),
            },
            [2] = new[]
            {
                ("السخان مش بيسخن من النهارده الصبح", 10),
                ("ممكن يكون الهيتر عطلان، هحتاج أشوفه", 30),
                ("طب امتى تقدر تجي؟", 45),
                ("أنا في منطقة تاني النهارده، بكره الصبح إن شاء الله", 60),
            },
            [3] = new[]
            {
                ("طرمبة المياه في العمارة وقفت من النهارده", 2),
                ("عايز أكشف على الطرمبة والمحرك", 20),
                ("طيب كام تكلفة الكشف؟", 40),
                ("الكشف ببلاش والتكلفة حسب العطل", 55),
            },
            [4] = new[]
            {
                ("فيه ريحة كريهة في الحمام والمياه بتتسرب", 5),
                ("الأغلب السيفون بايظ، هحتاج أغير", 18),
                ("كام هتكلف؟", 35),
                ("هكشف الأول وبعدين أقولك التكلفة بالظبط", 50),
            },
            [5] = new[]
            {
                ("البانيو مسدود والمياه مش بتصرف خالص", 3),
                ("جربت تسليك البانيو بمادة كيميائية؟", 20),
                ("جربت كل حاجة ومفيش نتيجة", 40),
                ("هحتاج أستخدم الضغط العالي، هجيلك إن شاء الله", 55),
            },
            [6] = new[]
            {
                ("المفاتيح في البيت بتشرر والنور قطع", 8),
                ("فصل الكهربا فوراً متلمسش حاجة", 15),
                ("فصلت الكهرباء زي ما قلت", 22),
                ("كويس، هجيلك في خلال ساعة إن شاء الله", 35),
            },
            [7] = new[]
            {
                ("اللمبات كلها بتطفي وتفضل لماعة في الشقة", 5),
                ("العطل في الدائرة العامة للشقة", 18),
                ("طيب هتستغرق وقت في التصليح؟", 32),
                ("ساعتين تلاتة حسب العطل بالظبط", 50),
            },
            [8] = new[]
            {
                ("المراوح كلها وقفت والتيار بينقطع كل شوية", 3),
                ("مشكلة في الدائرة العامة للشقة، هحتاج أفحص", 20),
                ("كام تكلفة الإصلاح تقريباً؟", 40),
                ("حسب العطل، هقولك بعد الكشف", 55),
            },
            [9] = new[]
            {
                ("مفتاح التكييف سخن والنور فصل في الصالة", 8),
                ("متدورش على المفتاح تاني خالص، ممكن يحرق", 18),
                ("طيب هجيلك امتى؟", 32),
                ("هاروحلك النهارده بعد المغرب إن شاء الله", 50),
            },
        };

        var messagesCount = 0;

        foreach (var (convIdx, messages) in messageData)
        {
            if (convIdx >= conversations.Count) continue;
            var conv = conversations[convIdx];
            var custId = conv.CustomerId;

            if (!craftsmanUserIds.TryGetValue(conv.CraftsmanId, out var craftUserId))
                continue;

            for (int mIdx = 0; mIdx < messages.Length; mIdx++)
            {
                var msg = messages[mIdx];
                var senderId = mIdx % 2 == 0 ? custId : craftUserId;

                _context.Messages.Add(new Message
                {
                    ConversationId = conv.Id,
                    SenderId = senderId,
                    Content = msg.content,
                    MessageType = "text",
                    IsRead = true,
                    SentAt = conv.CreatedAt.AddMinutes(msg.minutesOffset)
                });

                messagesCount++;
            }

            conv.LastMessageAt = conv.CreatedAt.AddMinutes(messages[^1].minutesOffset);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("{Count} messages seeded.", messagesCount);
    }
   
}