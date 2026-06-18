using Harfi.DTOs.RAG;
using Harfi.Models.Constants;
using Harfi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Harfi.Repositories.Data;

/// <summary>
/// Seeds realistic Arabic production-ready test data covering ALL 20 entities.
/// Runs once on startup when the Users table is empty.
/// Config tables (ServiceTypes, Cities, FeatureFlags) are always seeded independently.
/// </summary>
public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DataSeeder> _logger;
    private readonly string _webRootPath;
    private readonly HttpClient _httpClient = new HttpClient();
    private int _maleAvatarIndex = 0;
    private int _femaleAvatarIndex = 0;
    private static readonly DateTime BaseDate = DateTime.UtcNow.AddMonths(-6);
    private static readonly Random Rng = new(42);

    public DataSeeder(
        AppDbContext context,
        UserManager<User> userManager,
        ILogger<DataSeeder> logger,
        string webRootPath)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _webRootPath = webRootPath;
    }

    /// <summary>
    /// ينزل صورة بورتريه عشوائية من randomuser.me، يحفظها في wwwroot/uploads/profiles،
    /// ويرجع الـ relative path بنفس فورمات Imageservice.SaveFileAsync.
    /// </summary>
    private async Task<string?> DownloadAndSaveProfileImageAsync(bool isMale, string folder = "uploads/profiles")
    {
        try
        {
            int index = isMale
                ? (_maleAvatarIndex++ % 100)
                : (_femaleAvatarIndex++ % 100);

            var gender = isMale ? "men" : "women";
            var sourceUrl = $"https://randomuser.me/api/portraits/{gender}/{index}.jpg";

            var bytes = await _httpClient.GetByteArrayAsync(sourceUrl);

            var folderPath = Path.Combine(_webRootPath, folder);
            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}.jpg";
            var fullPath = Path.Combine(folderPath, fileName);


            await File.WriteAllBytesAsync(fullPath, bytes);

            return $"/{folder}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to download/save seed profile image — skipping.");
            return null;
        }
    }

    public async Task SeedAsync()
    {


        bool hasUsers = await _context.Users.IgnoreQueryFilters().AnyAsync();

        if (!await _context.ServiceTypes.AnyAsync()) await SeedServiceTypesAsync();
        if (!await _context.Cities.AnyAsync()) await SeedCitiesAsync();
        if (!await _context.FeatureFlags.AnyAsync()) await SeedFeatureFlagsAsync();

        if (hasUsers)
        {
            _logger.LogInformation("Seed skipped — data already exists.");
            SeedStatus.IsCompleted = true;

            return;
        }

        _logger.LogInformation("Starting full data seed...");
        await SeedAdminAsync();
        await SeedCustomerUsersAsync();
        await SeedCraftsmanUsersAsync();
        await SeedCraftsmanProfilesAsync();
        await SeedJobsAsync();
        await SeedCommonProblemsAsJobsAsync();
        await SeedReviewsAsync();
        await RecalculateRatingsAsync();
        await SeedConversationsAsync();
        await SeedMessagesAsync();
        await SeedNotificationsAsync();
        await SeedAdminAuditLogsAsync();
        await SeedReportsAsync();
        await SeedAIChatMessagesAsync();
        await SeedMediaFilesAsync();
        await SeedRefreshTokensAsync();
        await SeedEmailVerificationsAsync();
        await SeedPhoneVerificationsAsync();
        await SeedRAGDocumentsAsync();
        await SeedJobFeedbacksAsync();
        await SeedUserConnectionsAsync();
        await SeedIdentityClaimsAsync();
        _logger.LogInformation("Full seed completed successfully — all 20 tables populated.");
        SeedStatus.IsCompleted = true;

    }




    // ═══════════════════════════════════════════════════════════
    //  1. SERVICE TYPES (15 diverse trades)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedServiceTypesAsync()
    {
        var list = new[]
        {
            new ServiceType { NameAr = "سباكة",          NameEn = "Plumbing",          Icon = "🔧", IsActive = true },
            new ServiceType { NameAr = "كهرباء",         NameEn = "Electrical",        Icon = "⚡", IsActive = true },
            new ServiceType { NameAr = "نجارة",          NameEn = "Carpentry",         Icon = "🪚", IsActive = true },
            new ServiceType { NameAr = "دهانات",         NameEn = "Painting",          Icon = "🎨", IsActive = true },
            new ServiceType { NameAr = "تكييف وتبريد",   NameEn = "AC & Cooling",      Icon = "❄️", IsActive = true },
            new ServiceType { NameAr = "تبليط وسيراميك", NameEn = "Tiling & Ceramic",  Icon = "🪟", IsActive = true },
            new ServiceType { NameAr = "حدادة",          NameEn = "Ironwork",          Icon = "⚒️", IsActive = true },
            new ServiceType { NameAr = "جبس وأسقف",      NameEn = "Gypsum & Ceilings", Icon = "🏗️", IsActive = true },
            new ServiceType { NameAr = "زجاج ومرايا",    NameEn = "Glass & Mirrors",   Icon = "🪞", IsActive = true },
            new ServiceType { NameAr = "ألمنيوم",        NameEn = "Aluminum",          Icon = "🪟", IsActive = true },
            new ServiceType { NameAr = "تنظيف وتعقيم",   NameEn = "Cleaning",          Icon = "🧹", IsActive = true },
            new ServiceType { NameAr = "نقاشة وديكور",   NameEn = "Decorative Painting", Icon = "🎭", IsActive = true },
            new ServiceType { NameAr = "مكافحة حشرات",   NameEn = "Pest Control",      Icon = "🐛", IsActive = true },
            new ServiceType { NameAr = "أمن وكاميرات",   NameEn = "Security Cameras",  Icon = "📹", IsActive = true },
            new ServiceType { NameAr = "صيانة أجهزة",    NameEn = "Appliance Repair",  Icon = "🔩", IsActive = false }
        };
        await _context.ServiceTypes.AddRangeAsync(list);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Service types seeded: {N}", list.Length);
    }

    // ═══════════════════════════════════════════════════════════
    //  2. CITIES (18 Egyptian cities across governorates)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedCitiesAsync()
    {
        var list = new[]
        {
            new City { NameAr = "القاهرة",        NameEn = "Cairo",        Governorate = "القاهرة",        IsActive = true },
            new City { NameAr = "الإسكندرية",     NameEn = "Alexandria",   Governorate = "الإسكندرية",     IsActive = true },
            new City { NameAr = "الجيزة",         NameEn = "Giza",         Governorate = "الجيزة",         IsActive = true },
            new City { NameAr = "شبرا الخيمة",    NameEn = "Shubra",       Governorate = "القليوبية",     IsActive = true },
            new City { NameAr = "المنصورة",       NameEn = "Mansoura",     Governorate = "الدقهلية",       IsActive = true },
            new City { NameAr = "طنطا",           NameEn = "Tanta",        Governorate = "الغربية",        IsActive = true },
            new City { NameAr = "أسيوط",          NameEn = "Assiut",       Governorate = "أسيوط",          IsActive = true },
            new City { NameAr = "الإسماعيلية",    NameEn = "Ismailia",     Governorate = "الإسماعيلية",   IsActive = true },
            new City { NameAr = "الأقصر",         NameEn = "Luxor",        Governorate = "الأقصر",         IsActive = true },
            new City { NameAr = "الغردقة",        NameEn = "Hurghada",     Governorate = "البحر الأحمر",  IsActive = true },
            new City { NameAr = "بورسعيد",        NameEn = "Port Said",    Governorate = "بورسعيد",        IsActive = true },
            new City { NameAr = "السويس",         NameEn = "Suez",         Governorate = "السويس",          IsActive = true },
            new City { NameAr = "دمنهور",         NameEn = "Damanhur",     Governorate = "البحيرة",        IsActive = true },
            new City { NameAr = "المنيا",         NameEn = "Minya",        Governorate = "المنيا",          IsActive = true },
            new City { NameAr = "سوهاج",          NameEn = "Sohag",        Governorate = "سوهاج",          IsActive = true },
            new City { NameAr = "بني سويف",       NameEn = "Beni Suef",    Governorate = "بني سويف",      IsActive = true },
            new City { NameAr = "الفيوم",         NameEn = "Fayoum",       Governorate = "الفيوم",         IsActive = true },
            new City { NameAr = "كفر الشيخ",      NameEn = "Kafr El Sheikh", Governorate = "كفر الشيخ",   IsActive = true }
        };
        await _context.Cities.AddRangeAsync(list);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cities seeded: {N}", list.Length);
    }

    // ═══════════════════════════════════════════════════════════
    //  3. FEATURE FLAGS (8 flags)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedFeatureFlagsAsync()
    {
        var list = new[]
        {
            new FeatureFlag { Key = "SelfFixGuideEnabled",      IsEnabled = true,  UpdatedAt = BaseDate.AddMonths(3) },
            new FeatureFlag { Key = "VoiceSearchEnabled",       IsEnabled = false, UpdatedAt = BaseDate.AddMonths(2) },
            new FeatureFlag { Key = "AIMatchingEnabled",        IsEnabled = true,  UpdatedAt = BaseDate.AddMonths(1) },
            new FeatureFlag { Key = "CraftsmanDirectBooking",  IsEnabled = true,  UpdatedAt = BaseDate.AddMonths(1) },
            new FeatureFlag { Key = "MaintenanceContractEnabled", IsEnabled = false, UpdatedAt = BaseDate },
            new FeatureFlag { Key = "DisputeAutoResolution",    IsEnabled = true,  UpdatedAt = BaseDate.AddMonths(4) },
            new FeatureFlag { Key = "MultiLanguageEnabled",     IsEnabled = false, UpdatedAt = BaseDate.AddMonths(5) },
            new FeatureFlag { Key = "RAGSolutionEnabled",       IsEnabled = true,  UpdatedAt = BaseDate.AddDays(15) }
        };
        await _context.FeatureFlags.AddRangeAsync(list);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Feature flags seeded: {N}", list.Length);
    }

    // ═══════════════════════════════════════════════════════════
    //  4. ADMIN USERS (3 support team members)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedAdminAsync()
    {
        var admins = new[]
        {
            new { Email = "admin@harfi.com",       Name = "مدير النظام",       Phone = "01000000000" },
            new { Email = "support1@harfi.com",    Name = "أحمد المشرف",       Phone = "01000000001" },
            new { Email = "support2@harfi.com",    Name = "مريم الدعم",         Phone = "01000000002" }
        };
        for (int i = 0; i < admins.Length; i++)
        {
            var d = admins[i];
            var user = new User
            {
                UserName = d.Email,
                Email = d.Email,
                Name = d.Name,
                Role = "admin",
                Phone = d.Phone,
                IsActive = true,
                IsVerified = true,
                EmailConfirmed = true,
                CreatedAt = BaseDate.AddDays(-i * 2)
            };
            user.ProfileImageUrl = await DownloadAndSaveProfileImageAsync(isMale: true);
            var result = await _userManager.CreateAsync(user, "Admin@Harfi2024!");
            if (!result.Succeeded)
                _logger.LogWarning("Admin seed failed {E}: {Err}", d.Email,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
        }
        _logger.LogInformation("Admin users seeded: {N}", admins.Length);
    }

    private static readonly HashSet<string> FemaleFirstNames = new()
    {
        "سارة", "نورهان", "مريم", "فاطمة", "منة", "دينا", "ليلى", "هبة",
        "ريم", "شيماء", "إيمان", "أمنية", "مروة", "ناهد", "رنا"
    };

    private static bool IsMaleName(string fullName)
    {
        var firstWord = fullName.Split(' ').FirstOrDefault() ?? fullName;
        return !FemaleFirstNames.Contains(firstWord);
    }

    // ═══════════════════════════════════════════════════════════
    //  5. CUSTOMER USERS (25 with diverse states)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedCustomerUsersAsync()
    {
        var data = new[]
        {
            new { Email = "sara.ahmed@gmail.com",          Name = "سارة أحمد",           Phone = "01245678901", Active = true,  Verified = true,  Deleted = false, Days = 5   },
            new { Email = "nourhan.mohamed@gmail.com",     Name = "نورهان محمد",         Phone = "01567890124", Active = true,  Verified = true,  Deleted = false, Days = 15  },
            new { Email = "maryam.ali@gmail.com",          Name = "مريم علي",             Phone = "01067890123", Active = false, Verified = true,  Deleted = false, Days = 20  },
            new { Email = "fatma.hassan@gmail.com",        Name = "فاطمة حسن",           Phone = "01178901234", Active = false, Verified = true,  Deleted = true,  Days = 25  },
            new { Email = "mennatallah.khaled@gmail.com",  Name = "منة الله خالد",       Phone = "01289012345", Active = true,  Verified = false, Deleted = false, Days = 160 },
            new { Email = "amr.ali@gmail.com",             Name = "عمرو علي",             Phone = "01012349876", Active = true,  Verified = true,  Deleted = false, Days = 10  },
            new { Email = "dina.mahmoud@gmail.com",        Name = "دينا محمود",           Phone = "01123456701", Active = true,  Verified = true,  Deleted = false, Days = 18  },
            new { Email = "hosam.adel@gmail.com",          Name = "حسام عادل",            Phone = "01234567012", Active = true,  Verified = true,  Deleted = false, Days = 22  },
            new { Email = "layla.karim@gmail.com",         Name = "ليلى كريم",            Phone = "01567890123", Active = true,  Verified = true,  Deleted = false, Days = 28  },
            new { Email = "khaled.omar@gmail.com",         Name = "خالد عمر",             Phone = "01045678901", Active = true,  Verified = true,  Deleted = false, Days = 35  },
            new { Email = "rana.adel@gmail.com",           Name = "رنا عادل",             Phone = "01156789012", Active = true,  Verified = true,  Deleted = false, Days = 40  },
            new { Email = "tamer.hassan@gmail.com",        Name = "تامر حسن",             Phone = "01267890123", Active = true,  Verified = true,  Deleted = false, Days = 45  },
            new { Email = "heba.nabil@gmail.com",          Name = "هبة نبيل",             Phone = "01578901234", Active = true,  Verified = true,  Deleted = false, Days = 50  },
            new { Email = "mosaab.ahmed@gmail.com",        Name = "مصعب أحمد",            Phone = "01089012345", Active = true,  Verified = true,  Deleted = false, Days = 55  },
            new { Email = "reem.yasser@gmail.com",         Name = "ريم ياسر",             Phone = "01190123456", Active = true,  Verified = true,  Deleted = false, Days = 60  },
            new { Email = "gamal.abdel@gmail.com",         Name = "جمال عبد اللطيف",      Phone = "01201234567", Active = true,  Verified = true,  Deleted = false, Days = 65  },
            new { Email = "shaimaa.ahmed@gmail.com",       Name = "شيماء أحمد",           Phone = "01512345678", Active = false, Verified = true,  Deleted = false, Days = 70  },
            new { Email = "walid.sayed@gmail.com",         Name = "وليد سيد",             Phone = "01023456789", Active = true,  Verified = false, Deleted = false, Days = 170 },
            new { Email = "eman.ali@gmail.com",            Name = "إيمان علي",            Phone = "01134567890", Active = true,  Verified = true,  Deleted = false, Days = 75  },
            new { Email = "hany.kamal@gmail.com",          Name = "هاني كمال",            Phone = "01245678901", Active = true,  Verified = true,  Deleted = false, Days = 80  },
            new { Email = "omnya.reda@gmail.com",          Name = "أمنية رضا",            Phone = "01556789012", Active = true,  Verified = true,  Deleted = false, Days = 85  },
            new { Email = "ashraf.mahmoud@gmail.com",      Name = "أشرف محمود",           Phone = "01067890123", Active = true,  Verified = true,  Deleted = false, Days = 90  },
            new { Email = "marwa.khaled@gmail.com",        Name = "مروة خالد",            Phone = "01178901234", Active = false, Verified = true,  Deleted = true,  Days = 95  },
            new { Email = "samer.fathy@gmail.com",         Name = "سامر فتحي",            Phone = "01289012345", Active = true,  Verified = true,  Deleted = false, Days = 100 },
            new { Email = "nahed.adel@gmail.com",          Name = "ناهد عادل",            Phone = "01590123456", Active = true,  Verified = true,  Deleted = false, Days = 105 }
        };

        int deletedCount = 0;
        foreach (var d in data)
        {
            var user = new User
            {
                UserName = d.Email,
                Email = d.Email,
                Name = d.Name,
                Role = "customer",
                Phone = d.Phone,
                IsActive = d.Active,
                IsVerified = d.Verified,
                IsDeleted = d.Deleted,
                EmailConfirmed = d.Verified,
                CreatedAt = BaseDate.AddDays(d.Days)
            };
            if (d.Deleted)
            {
                deletedCount++;
                user.DeletedAt = user.CreatedAt.AddMonths(2);
                user.DeletedByAdminId = 1;
                user.DeletionReason = $"انتهاك شروط الاستخدام — بلاغ #{deletedCount}";
            }
            user.ProfileImageUrl = await DownloadAndSaveProfileImageAsync(isMale: IsMaleName(d.Name));
            var result = await _userManager.CreateAsync(user, "Customer@2024");
            if (!result.Succeeded)
                _logger.LogWarning("Customer seed failed {E}", d.Email);
        }
        _logger.LogInformation("Customer users seeded: {N}", data.Length);
    }

    // ═══════════════════════════════════════════════════════════
    //  6. CRAFTSMAN USERS (20 covering all states)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedCraftsmanUsersAsync()
    {
        var data = new[]
        {
            new { Email = "ahmed.ali@gmail.com",        Name = "احمد علي",       Phone = "01012345678", Days = 10  },
            new { Email = "mohamed.hassan@gmail.com",   Name = "محمد حسن",       Phone = "01123456789", Days = 8   },
            new { Email = "abdallah.khaled@gmail.com",  Name = "عبدالله خالد",    Phone = "01234567890", Days = 30  },
            new { Email = "mostafa.mahmoud@gmail.com",  Name = "مصطفى محمود",    Phone = "01512345678", Days = 7   },
            new { Email = "hussien.reda@gmail.com",     Name = "حسين رضا",       Phone = "01098765432", Days = 40  },
            new { Email = "kareem.samy@gmail.com",      Name = "كريم سامي",      Phone = "01156789012", Days = 6   },
            new { Email = "youssef.adel@gmail.com",     Name = "يوسف عادل",      Phone = "01234561234", Days = 151 },
            new { Email = "ibrahim.nasr@gmail.com",     Name = "إبراهيم نصر",    Phone = "01567890123", Days = 12  },
            new { Email = "michael.awad@gmail.com",     Name = "ميشيل عوض",      Phone = "01011111111", Days = 14  },
            new { Email = "hassan.shahat@gmail.com",    Name = "حسن شحاتة",      Phone = "01122222222", Days = 16  },
            new { Email = "nader.hamdy@gmail.com",      Name = "نادر حمدي",      Phone = "01233333333", Days = 18  },
            new { Email = "sameh.fawzy@gmail.com",      Name = "سامح فوزي",      Phone = "01544444444", Days = 20  },
            new { Email = "tamer.nabil@gmail.com",      Name = "تامر نبيل",      Phone = "01055555555", Days = 25  },
            new { Email = "adel.makram@gmail.com",      Name = "عادل مكرم",      Phone = "01166666666", Days = 32  },
            new { Email = "wael.gamal@gmail.com",       Name = "وائل جمال",      Phone = "01277777777", Days = 38  },
            new { Email = "fady.shafik@gmail.com",      Name = "فادي شفيق",      Phone = "01588888888", Days = 42  },
            new { Email = "marwan.atef@gmail.com",      Name = "مروان عاطف",     Phone = "01099999999", Days = 48  },
            new { Email = "sherif.ashraf@gmail.com",    Name = "شريف أشرف",      Phone = "01100000001", Days = 52  },
            new { Email = "khaled.nasr@gmail.com",      Name = "خالد نصر",       Phone = "01200000002", Days = 58  },
            new { Email = "george.ramzy@gmail.com",     Name = "جورج رمسي",      Phone = "01500000003", Days = 62  },
            new { Email = "tanta.plumber@gmail.com",   Name = "محمود السيد",   Phone = "01000011111", Days = 5  },
            new { Email = "tanta.electric@gmail.com",  Name = "طارق محمود",    Phone = "01000022222", Days = 6  },
            new { Email = "tanta.carpenter@gmail.com", Name = "سعيد النجار",   Phone = "01000033333", Days = 7  },
            new { Email = "tanta.painter@gmail.com",   Name = "عمر النقاش",    Phone = "01000044444", Days = 8  },
            new { Email = "tanta.hvac@gmail.com",      Name = "كريم تكييف",    Phone = "01000055555", Days = 9  },

            new { Email = "karim.ali@gmail.com",       Name = "كريم علي",       Phone = "01011111222", Days = 5  },
            new { Email = "mahmoud.faris@gmail.com",   Name = "محمود فارس",     Phone = "01022222333", Days = 7  },
            new { Email = "yasser.nour@gmail.com",     Name = "ياسر نور",       Phone = "01033333444", Days = 9  },
            new { Email = "ayman.saad@gmail.com",      Name = "أيمن سعد",       Phone = "01044444555", Days = 11 },
            new { Email = "rami.khattab@gmail.com",    Name = "رامي الخطاب",    Phone = "01055555666", Days = 13 },
            new { Email = "magdy.hassan@gmail.com",    Name = "مجدي حسن",       Phone = "01066666777", Days = 17 },
            new { Email = "nabil.rushdy@gmail.com",    Name = "نبيل رشدي",      Phone = "01077777888", Days = 19 },
            new { Email = "essam.farid@gmail.com",     Name = "عصام فريد",      Phone = "01088888999", Days = 21 },
            new { Email = "waleed.mansour@gmail.com",  Name = "وليد منصور",     Phone = "01099999111", Days = 23 },
            new { Email = "ashraf.nagi@gmail.com",     Name = "أشرف ناجي",      Phone = "01111111222", Days = 26 },
            new { Email = "emad.khalil@gmail.com",     Name = "عماد خليل",      Phone = "01122222333", Days = 28 },
            new { Email = "hazem.saber@gmail.com",     Name = "حازم صابر",      Phone = "01133333444", Days = 33 },
            new { Email = "sherif.anwar@gmail.com",    Name = "شريف أنور",      Phone = "01144444555", Days = 36 },
            new { Email = "amgad.refaat@gmail.com",    Name = "أمجد رفعت",      Phone = "01155555666", Days = 39 },
            new { Email = "taher.badran@gmail.com",    Name = "طاهر بدران",     Phone = "01166666777", Days = 43 },
            new { Email = "ibrahim.selim@gmail.com",   Name = "إبراهيم سليم",   Phone = "01177777888", Days = 46 },
            new { Email = "hassan.morsi@gmail.com",    Name = "حسن مرسي",       Phone = "01188888999", Days = 49 },
            new { Email = "gamal.fouad@gmail.com",     Name = "جمال فؤاد",      Phone = "01199999111", Days = 53 },
            new { Email = "samir.lotfy@gmail.com",     Name = "سمير لطفي",      Phone = "01211111222", Days = 56 },
            new { Email = "medhat.wahba@gmail.com",    Name = "مدحت وهبة",      Phone = "01222222333", Days = 59 },
            new { Email = "ai@harfi.com",      Name = "AI",       Phone = "01200000002", Days = 58  }

        };
        foreach (var d in data)
        {
            var user = new User
            {
                UserName = d.Email,
                Email = d.Email,
                Name = d.Name,
                Role = "craftsman",
                Phone = d.Phone,
                IsActive = true,
                IsVerified = true,
                EmailConfirmed = true,
                CreatedAt = BaseDate.AddDays(d.Days)
            };
            user.ProfileImageUrl = await DownloadAndSaveProfileImageAsync(isMale: true);
            var result = await _userManager.CreateAsync(user, "Craftsman@2024");
            if (!result.Succeeded)
                _logger.LogWarning("Craftsman seed failed {E}", d.Email);
        }
        _logger.LogInformation("Craftsman users seeded: {N}", data.Length);
    }
    // ═══════════════════════════════════════════════════════════
    //  7. CRAFTSMAN PROFILES (20 — one per business state)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedCraftsmanProfilesAsync()
    {
        var users = await _context.Users.IgnoreQueryFilters()
            .Where(u => u.Role == "craftsman").OrderBy(u => u.CreatedAt).ToListAsync();
        if (users.Count < 46) { _logger.LogWarning("Expected 46 craftsman users, got {N}", users.Count); return; }
        User U(string email) => users.First(u => u.Email == email);

        var profiles = new[]
        {
            new Craftsman { UserId = U("ahmed.ali@gmail.com").Id,       ServiceType = "سباكة",          City = "القاهرة",        Neighborhood = "مدينة نصر",     PriceRangeMin = 150m, PriceRangeMax = 400m, Experience = 3,  IsApproved = false, IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "سباك متخصص في تركيب وصيانة شبكات المياه والصرف الصحي",                           NationalIdUrl = "/uploads/ids/id_1.jpg",  CreatedAt = U("ahmed.ali@gmail.com").CreatedAt },
            new Craftsman { UserId = U("mohamed.hassan@gmail.com").Id,  ServiceType = "كهرباء",         City = "الإسكندرية",     Neighborhood = "سيدي بشر",      PriceRangeMin = 200m, PriceRangeMax = 600m, Experience = 12, IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "مهندس كهربائي خبرة 12 سنة في تمديد الكهرباء والصيانة الشاملة",                     NationalIdUrl = "/uploads/ids/id_2.jpg",  CreatedAt = U("mohamed.hassan@gmail.com").CreatedAt },
            new Craftsman { UserId = U("abdallah.khaled@gmail.com").Id, ServiceType = "دهانات",         City = "الجيزة",          Neighborhood = "الهرم",         PriceRangeMin = 100m, PriceRangeMax = 300m, Experience = 2,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "نقاش دهانات بأسعار مناسبة — دهان داخلي وخارجي",                                       NationalIdUrl = "/uploads/ids/id_3.jpg",  CreatedAt = U("abdallah.khaled@gmail.com").CreatedAt },
            new Craftsman { UserId = U("mostafa.mahmoud@gmail.com").Id, ServiceType = "نجارة",          City = "القاهرة",        Neighborhood = "العباسية",      PriceRangeMin = 300m, PriceRangeMax = 800m, Experience = 8,  IsApproved = true,  IsAvailable = false, IsDeleted = false, Rating = 0m,  Bio = "نجار موبيليا وباركيه خبرة 8 سنوات",                                                    NationalIdUrl = "/uploads/ids/id_4.jpg",  CreatedAt = U("mostafa.mahmoud@gmail.com").CreatedAt },
            new Craftsman { UserId = U("hussien.reda@gmail.com").Id,    ServiceType = "تكييف وتبريد",   City = "طنطا",            Neighborhood = "شارع البحر",             PriceRangeMin = 250m, PriceRangeMax = 700m, Experience = 1,  IsApproved = false, IsAvailable = false, IsDeleted = true,  Rating = 0m,  Bio = "فني تكييف", RejectionReason = "بيانات الهوية الوطنية غير واضحة", DeletedAt = U("hussien.reda@gmail.com").CreatedAt.AddDays(3), DeletedByAdminId = 1, DeletionReason = "رفض طلب التسجيل: بيانات غير صحيحة", NationalIdUrl = "/uploads/ids/id_5.jpg", CreatedAt = U("hussien.reda@gmail.com").CreatedAt },
            new Craftsman { UserId = U("kareem.samy@gmail.com").Id,     ServiceType = "سباكة",          City = "المنصورة",        Neighborhood = "المنصورة",      PriceRangeMin = 180m, PriceRangeMax = 500m, Experience = 6,  IsApproved = true,  IsAvailable = false, IsDeleted = true,  Rating = 0m,  Bio = "سباك عام", DeletedAt = U("kareem.samy@gmail.com").CreatedAt.AddMonths(2), DeletedByAdminId = 1, DeletionReason = "شكاوى متعددة من العملاء", NationalIdUrl = "/uploads/ids/id_6.jpg",  CreatedAt = U("kareem.samy@gmail.com").CreatedAt },
            new Craftsman { UserId = U("youssef.adel@gmail.com").Id,    ServiceType = "تبليط وسيراميك", City = "الإسماعيلية",     Neighborhood = "الإسماعيلية",   PriceRangeMin = 200m, PriceRangeMax = 600m, Experience = 5,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "فني تبليط متخصص في السيراميك والرخام والبورسلين",                                    NationalIdUrl = "/uploads/ids/id_7.jpg",  CreatedAt = U("youssef.adel@gmail.com").CreatedAt },
            new Craftsman { UserId = U("ibrahim.nasr@gmail.com").Id,    ServiceType = "كهرباء",         City = "الأقصر",          Neighborhood = "الأقصر",        PriceRangeMin = 150m, PriceRangeMax = 450m, Experience = 9,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "كهربائي معتمد خبرة 9 سنوات في المنازل والمحلات التجارية",                             NationalIdUrl = "/uploads/ids/id_8.jpg",  CreatedAt = U("ibrahim.nasr@gmail.com").CreatedAt },
            new Craftsman { UserId = U("michael.awad@gmail.com").Id,    ServiceType = "سباكة",          City = "القاهرة",        Neighborhood = "شبرا",          PriceRangeMin = 120m, PriceRangeMax = 350m, Experience = 7,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "جميع أعمال السباكة والصرف الصحي — تركيب وصيانة",                                     NationalIdUrl = "/uploads/ids/id_9.jpg",  CreatedAt = U("michael.awad@gmail.com").CreatedAt },
            new Craftsman { UserId = U("hassan.shahat@gmail.com").Id,   ServiceType = "جبس وأسقف",     City = "الجيزة",          Neighborhood = "الدقي",         PriceRangeMin = 250m, PriceRangeMax = 800m, Experience = 10, IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "أسقف جبس معلقة وديكورات جبسية بجميع الأشكال — تنفيذ عصري",                           NationalIdUrl = "/uploads/ids/id_10.jpg", CreatedAt = U("hassan.shahat@gmail.com").CreatedAt },
            new Craftsman { UserId = U("nader.hamdy@gmail.com").Id,     ServiceType = "كهرباء",         City = "الإسكندرية",     Neighborhood = "محرم بك",       PriceRangeMin = 180m, PriceRangeMax = 500m, Experience = 15, IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "مهندس كهرباء خبرة 15 سنة — صيانة وتركيب جميع الأنظمة الكهربائية",                     NationalIdUrl = "/uploads/ids/id_11.jpg", CreatedAt = U("nader.hamdy@gmail.com").CreatedAt },
            new Craftsman { UserId = U("sameh.fawzy@gmail.com").Id,     ServiceType = "دهانات",         City = "المنصورة",        Neighborhood = "طلخا",          PriceRangeMin = 90m,  PriceRangeMax = 250m, Experience = 4,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "دهانات داخلية وخارجية — نقاشة ديكور وألوان جدران مودرن",                             NationalIdUrl = "/uploads/ids/id_12.jpg", CreatedAt = U("sameh.fawzy@gmail.com").CreatedAt },
            new Craftsman { UserId = U("tamer.nabil@gmail.com").Id,     ServiceType = "نجارة",          City = "بني سويف",        Neighborhood = "بني سويف",      PriceRangeMin = 200m, PriceRangeMax = 600m, Experience = 6,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "نجار أثاث وديكور — خشب طبيعي وأبلكاش — تصميم وتنفيذ",                                 NationalIdUrl = "/uploads/ids/id_13.jpg", CreatedAt = U("tamer.nabil@gmail.com").CreatedAt },
            new Craftsman { UserId = U("adel.makram@gmail.com").Id,     ServiceType = "تكييف وتبريد",   City = "بورسعيد",         Neighborhood = "بورسعيد",       PriceRangeMin = 300m, PriceRangeMax = 900m, Experience = 11, IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "فني تكييف وتبريد معتمد — جميع الماركات — تركيب وصيانة",                              NationalIdUrl = "/uploads/ids/id_14.jpg", CreatedAt = U("adel.makram@gmail.com").CreatedAt },
            new Craftsman { UserId = U("wael.gamal@gmail.com").Id,      ServiceType = "تبليط وسيراميك", City = "دمنهور",          Neighborhood = "دمنهور",        PriceRangeMin = 180m, PriceRangeMax = 500m, Experience = 14, IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "معلم سيراميك وبورسلين ورخام — شغل يدوي ممتاز وضمان على العمل",                        NationalIdUrl = "/uploads/ids/id_15.jpg", CreatedAt = U("wael.gamal@gmail.com").CreatedAt },
            new Craftsman { UserId = U("fady.shafik@gmail.com").Id,     ServiceType = "حدادة",          City = "أسيوط",           Neighborhood = "أسيوط",         PriceRangeMin = 350m, PriceRangeMax = 1200m, Experience = 9,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "حداد وأبواب حديد — درابزين — قضبان نوافذ — أشغال حديد فنية",                         NationalIdUrl = "/uploads/ids/id_16.jpg", CreatedAt = U("fady.shafik@gmail.com").CreatedAt },
            new Craftsman { UserId = U("marwan.atef@gmail.com").Id,     ServiceType = "زجاج ومرايا",    City = "المنيا",          Neighborhood = "المنيا",        PriceRangeMin = 150m, PriceRangeMax = 400m, Experience = 5,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "زجاج ومرايا — تركيب واجهات زجاجية — شبابيك ألمنيوم وزجاج",                           NationalIdUrl = "/uploads/ids/id_17.jpg", CreatedAt = U("marwan.atef@gmail.com").CreatedAt },
            new Craftsman { UserId = U("sherif.ashraf@gmail.com").Id,   ServiceType = "ألمنيوم",        City = "سوهاج",           Neighborhood = "سوهاج",         PriceRangeMin = 200m, PriceRangeMax = 700m, Experience = 7,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "ألمنيوم — شبابيك وأبواب — واجهات كلادينج — مطابخ ألمنيوم",                           NationalIdUrl = "/uploads/ids/id_18.jpg", CreatedAt = U("sherif.ashraf@gmail.com").CreatedAt },
            new Craftsman { UserId = U("khaled.nasr@gmail.com").Id,     ServiceType = "مكافحة حشرات",   City = "الفيوم",          Neighborhood = "الفيوم",        PriceRangeMin = 100m, PriceRangeMax = 300m, Experience = 6,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "مكافحة حشرات وقوارض — رش وتعقيم — مواد آمنة ومعتمدة من وزارة الصحة",                 NationalIdUrl = "/uploads/ids/id_19.jpg", CreatedAt = U("khaled.nasr@gmail.com").CreatedAt },
            new Craftsman { UserId = U("george.ramzy@gmail.com").Id,    ServiceType = "أمن وكاميرات",   City = "القاهرة",        Neighborhood = "المعادي",       PriceRangeMin = 400m, PriceRangeMax = 1500m, Experience = 8,  IsApproved = true,  IsAvailable = true,  IsDeleted = false, Rating = 0m,  Bio = "كاميرات مراقبة — أنظمة أمن — إنتركوم — أجهزة إنذار — تركيب وصيانة",                  NationalIdUrl = "/uploads/ids/id_20.jpg", CreatedAt = U("george.ramzy@gmail.com").CreatedAt },
            new Craftsman { UserId = U("tanta.plumber@gmail.com").Id, ServiceType = "سباكة", City = "طنطا", Neighborhood = "سيجر", PriceRangeMin = 150m, PriceRangeMax = 400m, Experience = 5, IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "سباك ممتاز متخصص في الصيانة المنزلية", NationalIdUrl = "/uploads/ids/id_tanta_1.jpg", CreatedAt = U("tanta.plumber@gmail.com").CreatedAt },
            new Craftsman { UserId = U("tanta.electric@gmail.com").Id, ServiceType = "كهرباء", City = "طنطا", Neighborhood = "المحطة", PriceRangeMin = 200m, PriceRangeMax = 500m, Experience = 7, IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "فني كهرباء خبرة في تأسيس وصيانة شبكات الكهرباء", NationalIdUrl = "/uploads/ids/id_tanta_2.jpg", CreatedAt = U("tanta.electric@gmail.com").CreatedAt },
            new Craftsman { UserId = U("tanta.carpenter@gmail.com").Id, ServiceType = "نجارة", City = "طنطا", Neighborhood = "المرشحة", PriceRangeMin = 250m, PriceRangeMax = 600m, Experience = 10, IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "نجار موبيليا وتصليح أثاث بجودة عالية", NationalIdUrl = "/uploads/ids/id_tanta_3.jpg", CreatedAt = U("tanta.carpenter@gmail.com").CreatedAt },
            new Craftsman { UserId = U("tanta.painter@gmail.com").Id, ServiceType = "دهانات", City = "طنطا", Neighborhood = "سعيد", PriceRangeMin = 100m, PriceRangeMax = 300m, Experience = 4, IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "نقاش وتشطيبات داخلية وخارجية بأسعار منافسة", NationalIdUrl = "/uploads/ids/id_tanta_4.jpg", CreatedAt = U("tanta.painter@gmail.com").CreatedAt },
            new Craftsman { UserId = U("tanta.hvac@gmail.com").Id, ServiceType = "تكييف وتبريد", City = "طنطا", Neighborhood = "كفر عصام", PriceRangeMin = 300m, PriceRangeMax = 700m, Experience = 8, IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "صيانة وتركيب جميع أنواع المكيفات وشحن فريون", NationalIdUrl = "/uploads/ids/id_tanta_5.jpg", CreatedAt = U("tanta.hvac@gmail.com").CreatedAt },

            new Craftsman { UserId = U("karim.ali@gmail.com").Id,      ServiceType = "كهرباء",          City = "القاهرة",      Neighborhood = "حلوان , شارع الثورة",              PriceRangeMin = 150m, PriceRangeMax = 400m, Experience = 6,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "كهربائي معتمد — تمديدات ولوحات وصيانة شاملة",                        NationalIdUrl = "/uploads/ids/id_25.jpg", CreatedAt = U("karim.ali@gmail.com").CreatedAt },
            new Craftsman { UserId = U("mahmoud.faris@gmail.com").Id,  ServiceType = "دهانات",          City = "القاهرة",      Neighborhood = "التجمع الخامس , شارع التسعين",     PriceRangeMin = 100m, PriceRangeMax = 350m, Experience = 7,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "نقاش ديكور داخلي وخارجي — دهان أمريكي وبلاستيك",                     NationalIdUrl = "/uploads/ids/id_26.jpg", CreatedAt = U("mahmoud.faris@gmail.com").CreatedAt },
            new Craftsman { UserId = U("yasser.nour@gmail.com").Id,    ServiceType = "سباكة",           City = "الجيزة",       Neighborhood = "فيصل , شارع الهرم",                PriceRangeMin = 150m, PriceRangeMax = 450m, Experience = 9,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "سباك متخصص في شبكات المياه والصرف الصحي — خبرة 9 سنوات",             NationalIdUrl = "/uploads/ids/id_27.jpg", CreatedAt = U("yasser.nour@gmail.com").CreatedAt },
            new Craftsman { UserId = U("ayman.saad@gmail.com").Id,     ServiceType = "تكييف وتبريد",    City = "الجيزة",       Neighborhood = "أكتوبر , شارع جمال عبد الناصر",   PriceRangeMin = 250m, PriceRangeMax = 700m, Experience = 11, IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "فني تكييف معتمد — جميع الماركات — تركيب وصيانة وشحن فريون",          NationalIdUrl = "/uploads/ids/id_28.jpg", CreatedAt = U("ayman.saad@gmail.com").CreatedAt },
            new Craftsman { UserId = U("rami.khattab@gmail.com").Id,   ServiceType = "نجارة",           City = "الإسكندرية",   Neighborhood = "سموحة , شارع فوزي معاذ",           PriceRangeMin = 200m, PriceRangeMax = 600m, Experience = 8,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "نجار أثاث وديكور — أبلكاش وخشب طبيعي — تصنيع وتركيب",               NationalIdUrl = "/uploads/ids/id_29.jpg", CreatedAt = U("rami.khattab@gmail.com").CreatedAt },
            new Craftsman { UserId = U("magdy.hassan@gmail.com").Id,   ServiceType = "سباكة",           City = "الإسكندرية",   Neighborhood = "المنتزه , شارع أبو قير",           PriceRangeMin = 180m, PriceRangeMax = 500m, Experience = 10, IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "سباك عام — تسليك وكشف تسريبات وصيانة سخانات",                        NationalIdUrl = "/uploads/ids/id_30.jpg", CreatedAt = U("magdy.hassan@gmail.com").CreatedAt },
            new Craftsman { UserId = U("nabil.rushdy@gmail.com").Id,   ServiceType = "كهرباء",          City = "المنصورة",     Neighborhood = "المنصورة , شارع الجمهورية",        PriceRangeMin = 150m, PriceRangeMax = 450m, Experience = 7,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "كهربائي خبرة 7 سنوات — صيانة وتمديدات وقواطع",                       NationalIdUrl = "/uploads/ids/id_31.jpg", CreatedAt = U("nabil.rushdy@gmail.com").CreatedAt },
            new Craftsman { UserId = U("essam.farid@gmail.com").Id,    ServiceType = "سباكة",           City = "أسيوط",        Neighborhood = "أسيوط , شارع بورسعيد",             PriceRangeMin = 120m, PriceRangeMax = 350m, Experience = 5,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "سباك عام — مواسير وصرف صحي وخلاطات",                                  NationalIdUrl = "/uploads/ids/id_32.jpg", CreatedAt = U("essam.farid@gmail.com").CreatedAt },
            new Craftsman { UserId = U("waleed.mansour@gmail.com").Id, ServiceType = "نجارة",           City = "بورسعيد",      Neighborhood = "بورسعيد , شارع الجمهورية",         PriceRangeMin = 200m, PriceRangeMax = 550m, Experience = 6,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "نجار أبواب وموبيليا — خشب وMDF — تصنيع وتركيب وصيانة",               NationalIdUrl = "/uploads/ids/id_33.jpg", CreatedAt = U("waleed.mansour@gmail.com").CreatedAt },
            new Craftsman { UserId = U("ashraf.nagi@gmail.com").Id,    ServiceType = "تبليط وسيراميك", City = "دمنهور",       Neighborhood = "دمنهور , شارع الحرية",             PriceRangeMin = 150m, PriceRangeMax = 400m, Experience = 8,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "معلم سيراميك وبورسلين ورخام — تركيب وصيانة — ضمان على الشغل",       NationalIdUrl = "/uploads/ids/id_34.jpg", CreatedAt = U("ashraf.nagi@gmail.com").CreatedAt },
            new Craftsman { UserId = U("emad.khalil@gmail.com").Id,    ServiceType = "كهرباء",          City = "المنيا",       Neighborhood = "المنيا , شارع الجمهورية",          PriceRangeMin = 130m, PriceRangeMax = 380m, Experience = 6,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "كهربائي منازل ومحلات — تمديدات وصيانة وأعطال",                       NationalIdUrl = "/uploads/ids/id_35.jpg", CreatedAt = U("emad.khalil@gmail.com").CreatedAt },
            new Craftsman { UserId = U("hazem.saber@gmail.com").Id,    ServiceType = "سباكة",           City = "سوهاج",        Neighborhood = "سوهاج , شارع بورسعيد",             PriceRangeMin = 110m, PriceRangeMax = 320m, Experience = 4,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "سباك عام — تركيب وصيانة شبكات المياه والصرف",                         NationalIdUrl = "/uploads/ids/id_36.jpg", CreatedAt = U("hazem.saber@gmail.com").CreatedAt },
            new Craftsman { UserId = U("sherif.anwar@gmail.com").Id,   ServiceType = "نجارة",           City = "الفيوم",       Neighborhood = "الفيوم , شارع سعد زغلول",          PriceRangeMin = 180m, PriceRangeMax = 500m, Experience = 7,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "نجار أثاث وأبواب — خشب طبيعي وأبلكاش — تصميم وتنفيذ",               NationalIdUrl = "/uploads/ids/id_37.jpg", CreatedAt = U("sherif.anwar@gmail.com").CreatedAt },
            new Craftsman { UserId = U("amgad.refaat@gmail.com").Id,   ServiceType = "دهانات",          City = "بني سويف",     Neighborhood = "بني سويف , شارع الحرية",           PriceRangeMin = 90m,  PriceRangeMax = 280m, Experience = 5,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "نقاش داخلي وخارجي — بلاستيك وبوية زيتية — أسعار مناسبة",             NationalIdUrl = "/uploads/ids/id_38.jpg", CreatedAt = U("amgad.refaat@gmail.com").CreatedAt },
            new Craftsman { UserId = U("taher.badran@gmail.com").Id,   ServiceType = "سباكة",           City = "الأقصر",       Neighborhood = "الأقصر , شارع المدينة",            PriceRangeMin = 130m, PriceRangeMax = 380m, Experience = 8,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "سباك محترف — تسريبات وصرف صحي وسخانات — خبرة 8 سنوات",              NationalIdUrl = "/uploads/ids/id_39.jpg", CreatedAt = U("taher.badran@gmail.com").CreatedAt },
            new Craftsman { UserId = U("ibrahim.selim@gmail.com").Id,  ServiceType = "كهرباء",          City = "الإسماعيلية",  Neighborhood = "الإسماعيلية , شارع السلطان حسين",  PriceRangeMin = 160m, PriceRangeMax = 430m, Experience = 9,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "كهربائي معتمد — منازل ومحلات وعقارات — صيانة وتمديدات",              NationalIdUrl = "/uploads/ids/id_40.jpg", CreatedAt = U("ibrahim.selim@gmail.com").CreatedAt },
            new Craftsman { UserId = U("hassan.morsi@gmail.com").Id,   ServiceType = "نجارة",           City = "طنطا",         Neighborhood = "طنطا , شارع البحر",                PriceRangeMin = 200m, PriceRangeMax = 580m, Experience = 6,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "نجار موبيليا وباركيه — تصنيع وتركيب — ضمان على الشغل",               NationalIdUrl = "/uploads/ids/id_41.jpg", CreatedAt = U("hassan.morsi@gmail.com").CreatedAt },
            new Craftsman { UserId = U("gamal.fouad@gmail.com").Id,    ServiceType = "تكييف وتبريد",    City = "السويس",       Neighborhood = "السويس , شارع الجيش",              PriceRangeMin = 250m, PriceRangeMax = 700m, Experience = 10, IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "فني تكييف محترف — تركيب وصيانة وشحن فريون — جميع الماركات",          NationalIdUrl = "/uploads/ids/id_42.jpg", CreatedAt = U("gamal.fouad@gmail.com").CreatedAt },
            new Craftsman { UserId = U("samir.lotfy@gmail.com").Id,    ServiceType = "سباكة",           City = "كفر الشيخ",    Neighborhood = "كفر الشيخ , شارع بورسعيد",         PriceRangeMin = 120m, PriceRangeMax = 350m, Experience = 5,  IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "سباك عام — مواسير وخلاطات وصرف صحي",                                  NationalIdUrl = "/uploads/ids/id_43.jpg", CreatedAt = U("samir.lotfy@gmail.com").CreatedAt },
            new Craftsman { UserId = U("medhat.wahba@gmail.com").Id,   ServiceType = "حدادة",           City = "قنا",          Neighborhood = "قنا , شارع الجمهورية",             PriceRangeMin = 300m, PriceRangeMax = 900m, Experience = 12, IsApproved = true, IsAvailable = true, IsDeleted = false, Rating = 0m, Bio = "حداد أبواب ودرابزين وقضبان — أشغال حديد فنية — خبرة 12 سنة",         NationalIdUrl = "/uploads/ids/id_44.jpg", CreatedAt = U("medhat.wahba@gmail.com").CreatedAt },
            new Craftsman { UserId = U("ai@harfi.com").Id,    ServiceType = "AI",   City = "AI",        Neighborhood = "AI",       PriceRangeMin = 400m, PriceRangeMax = 1500m, Experience = 8,  IsApproved = true,  IsAvailable = false,  IsDeleted = false, Rating = 0m,  Bio = "AI Assistant",  NationalIdUrl = "/uploads/ids/id_20.jpg", CreatedAt = U("ai@harfi.com").CreatedAt }




        };
        await _context.Craftsmen.AddRangeAsync(profiles);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Craftsman profiles seeded: {N}", profiles.Length);
    }
    // ═══════════════════════════════════════════════════════════
    //  8. JOBS (80+ across all statuses and services)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedJobsAsync()
    {
        var craftsmen = await _context.Craftsmen.IgnoreQueryFilters().Include(c => c.User).ToListAsync();
        var customers = await _context.Users.IgnoreQueryFilters().Where(u => u.Role == "customer").ToListAsync();
        var approved = craftsmen.Where(c => c.IsApproved && !c.IsDeleted).ToList();

        var c = new Dictionary<string, User>();
        foreach (var u in customers) c[u.Email!.Split('@')[0].Split('.')[0]] = u;

        // Build job definitions — (custEmail, craftIdx, status, svc, desc, addr, problem, solution, daysAgo, duration, disputed, dispDays, dispRes)
        var defs = new List<(string cust, int cm, string st, string sv, string desc, string addr, string? prob, string? sol, int da, int dur, bool disp, int? dispD, string? dispR)>
        {
            ("mennatallah", 0, "مفتوح", "كهرباء", "المفاتيح في الصالة بتشرر وفيه رائحة احتراق", "15 شارع التحرير، الإسكندرية", "شرار من المفاتيح الكهربائية مع رائحة بلاستيك محترق", null, 165, 0, false, null, null),
            ("sara", 0, "قيد التنفيذ", "كهرباء", "لوحة الكهرباء الرئيسية عاطلة والكهربا مقطوعة", "8 شارع الجيش، مدينة نصر، القاهرة", "انقطاع تام في التيار الكهربائي بعد شرارة من اللوحة", null, 160, 0, false, null, null),
            ("nourhan", 0, "مكتمل", "كهرباء", "تركيب نقاط إضاءة جديدة في ثلاث غرف", "22 شارع أبو قير، الإسكندرية", "الغرف بدون إضاءة مناسبة", "تم تركيب 12 سبوت لايت LED مع توصيلات", 150, 5, false, null, null),
            ("sara", 7, "مكتمل", "كهرباء", "صيانة دورية شاملة للتوصيلات الكهربائية", "3 شارع النصر، الأقصر", "مطلوب فحص وقائي", "تم فحص وتجديد التوصيلات وتغيير القواطع القديمة", 120, 2, false, null, null),
            ("maryam", 0, "مرفوض", "كهرباء", "تمديدات كهربائية خارجية على واجهة المبنى", "55 شارع السوق، الجيزة", "تمديدات خارجية خطرة تحتاج ترخيص", null, 110, 0, false, null, null),
            ("nourhan", 7, "مكتمل", "كهرباء", "إصلاح عطل في دائرة الطاقة للمكيف", "18 شارع الجمهورية، الأقصر", "المكيف لا يعمل بسبب مشكلة في الكهرباء", "تم استبدال القاطع المخصص للمكيف", 130, 5, true, 155, "نزاع مرفوع — جاري المراجعة من الإدارة"),
            ("sara", 2, "مكتمل", "دهانات", "دهان كامل لشقة 3 غرف وصالة", "12 شارع الهرم، الجيزة", "دهانات قديمة متشققة", "تم دهان الشقة بالكامل لكن بجودة أقل", 90, 5, false, 100, "استرداد 30% — لصالح العميل"),
            ("nourhan", 2, "مكتمل", "دهانات", "دهان حوائط الصالة بالكامل", "7 شارع الطيران، الجيزة", "العميل يدعي أن الجودة سيئة", "تم التنفيذ حسب الاتفاق", 65, 5, false, 75, "الشغل مطابق للاتفاق — لصالح الحرفي"),
            ("nourhan", 7, "مكتمل", "كهرباء", "تركيب لوحة توزيع كهرباء جديدة", "33 شارع النيل، الأقصر", "اللوحة القديمة خطيرة", "تم تركيب لوحة حديثة مع قواطع حساسة", 35, 5, false, null, null),
            ("sara", 2, "مكتمل", "دهانات", "دهان حجرة النوم", "10 شارع فيصل، الجيزة", "دهان قديم محتاج تجديد", "تم الدهان لكن باختلاف اللون", 140, 5, false, null, null),
            ("sara", 3, "مكتمل", "نجارة", "تركيب دواليب مطبخ", "6 شارع رمسيس، القاهرة", "دواليب قديمة متهالكة", "تم تركيب دواليب المطبخ بالكامل", 80, 5, false, null, null),
            ("maryam", 0, "ملغى", "كهرباء", "تركيب نجفة كبيرة في الصالة", "44 شارع المحطة، الجيزة", null, null, 115, 0, false, null, null),
            ("mennatallah", 1, "مكتمل", "سباكة", "تسريب مياه من الحمام — تغيير خلاط", "5 شارع السيدة زينب، القاهرة", "خلاط الحمام بيسريب مستمر", "تم تغيير الخلاط بالكامل", 145, 3, false, null, null),
            ("amr", 1, "مكتمل", "سباكة", "بالوعة المطبخ مسدودة", "12 شارع النيل، القاهرة", "المياه مش بتصرف من البالوعة", "تم تسليك البالوعة بالضغط العالي", 140, 1, false, null, null),
            ("dina", 8, "مكتمل", "سباكة", "تركيب سخان غاز جديد", "3 شارع الهرم، الجيزة", "السخان القديم خربان", "تم تركيب سخان غاز 10 لتر مع التوصيلات", 135, 2, false, null, null),
            ("hosam", 9, "مكتمل", "جبس وأسقف", "تركيب أسقف جبس معلقة في الصالة", "8 شارع التحرير، الجيزة", "السقف قديم محتاج تجديد", "سقف جبس معلق بإضاءة مخفية", 130, 7, false, null, null),
            ("layla", 10, "مكتمل", "كهرباء", "تمديد كهرباء لغرفة جديدة", "22 شارع بورسعيد، الإسكندرية", "غرفة جديدة محتاجة توصيلات", "تم تمديد 6 نقاط كهرباء مع قواطع", 125, 4, false, null, null),
            ("khaled", 4, "مرفوض", "سباكة", "تغيير مواسير الحمام بالكامل", "15 شارع الجمهورية، المنصورة", "مواسير قديمة من الصاج", null, 120, 0, false, null, null),
            ("rana", 11, "مكتمل", "دهانات", "دهان واجهة المبنى الخارجي", "30 شارع المحطة، المنصورة", "الواجهة محتاجة دهان خارجي", "تم دهان الواجهة بدهان عازل", 115, 8, false, null, null),
            ("tamer", 12, "مكتمل", "نجارة", "تصميم وتنفيذ مكتبة حائط", "10 شارع السوق، بني سويف", "مكتبة حائط بطول 3 متر", "تم تنفيذ مكتبة من الخشب الطبيعي", 110, 10, false, null, null),
            ("heba", 0, "قيد التنفيذ", "كهرباء", "تغيير جميع المفاتيح والبرايز", "7 شارع الجيش، الإسكندرية", "المفاتيح قديمة ومتهالكة", null, 105, 0, false, null, null),
            ("mosaab", 1, "مكتمل", "سباكة", "تركيب فلتر مياه مركزي", "25 شارع النصر، القاهرة", "مياه الشرب فيها شوائب", "تم تركيب فلتر 7 مراحل", 100, 3, false, null, null),
            ("reem", 2, "مكتمل", "دهانات", "دهان غرفة نوم أطفال بديكور", "12 شارع فيصل، الجيزة", "غرفة الأطفال محتاجة رسومات", "تم الدهان برسومات كرتونية ملونة", 95, 6, false, null, null),
            ("gamal", 5, "مرفوض", "سباكة", "كشف تسربات مياه بدون تكسير", "40 شارع رمسيس، المنصورة", "فاتورة المياه عالية", null, 90, 0, false, null, null),
            ("shaimaa", 13, "مكتمل", "تكييف وتبريد", "صيانة دورية لتكيف سبليت", "3 شارع الميناء، بورسعيد", "التكييف مش بيبرد كويس", "تم تنظيف الفلاتر وشحن الفريون", 85, 2, false, null, null),
            ("walid", 14, "مفتوح", "تبليط وسيراميك", "تبليط حمام كامل", "20 شارع الدلتا، دمنهور", "حمام قديم محتاج تبليط جديد", null, 80, 0, false, null, null),
            ("eman", 15, "مكتمل", "حدادة", "تركيب درابزين حديد للسلم", "15 شارع النيل، أسيوط", "السلم بدون درابزين وخطير", "تم تركيب درابزين حديد مزخرف", 75, 7, false, null, null),
            ("hany", 16, "مفتوح", "زجاج ومرايا", "تركيب واجهة زجاجية لمحل", "10 شارع السوق، المنيا", "المحل محتاج واجهة زجاجية", null, 70, 0, false, null, null),
            ("omnya", 17, "مكتمل", "ألمنيوم", "تركيب شبابيك ألمنيوم جديدة", "8 شارع الجمهورية، سوهاج", "الشبابيك الخشبية قديمة", "تم تركيب 4 شبابيك ألمنيوم", 65, 5, false, null, null),
            ("ashraf", 18, "مكتمل", "مكافحة حشرات", "رش وتعقيم شقة بالكامل", "5 شارع النصر، الفيوم", "حشرات ونمل في الشقة", "تم الرش والتعقيم الشامل", 60, 1, false, null, null),
            ("marwa", 19, "مكتمل", "أمن وكاميرات", "تركيب 4 كاميرات مراقبة خارجية", "30 شارع المعادي، القاهرة", "المبنى محتاج كاميرات مراقبة", "تم تركيب 4 كاميرات مع NVR", 55, 3, false, null, null),
            ("mennatallah", 3, "مكتمل", "نجارة", "تركيب غرفة نوم كاملة", "12 شارع الهرم، الجيزة", "غرفة نوم جديدة محتاجة تركيب", "تم تركيب غرفة نوم كاملة + دواليب", 50, 5, true, 52, "تم حل النزاع بالتراضي — لصالح العميل"),
            ("amr", 4, "مكتمل", "سباكة", "تغيير طقم حمام كامل", "8 شارع النيل، المنصورة", "طقم الحمام قديم ومكسور", "تم تغيير طقم الحمام بالكامل", 45, 3, false, null, null),
            ("dina", 8, "مرفوض", "سباكة", "كشف تسرب في الحمام", "15 شارع التحرير، القاهرة", "تسرب في الحمام والدهان يتقشر", null, 40, 0, false, null, null),
            ("hosam", 9, "مكتمل", "جبس وأسقف", "ديكور جبس لغرفة المعيشة", "20 شارع النصر، الجيزة", "غرفة المعيشة محتاجة ديكور", "تم تركيب ديكور جبس بإضاءة LED", 38, 5, false, null, null),
            ("layla", 10, "قيد التنفيذ", "كهرباء", "تركيب نجفة وبراويز", "25 شارع السوق، الإسكندرية", "نجفة قديمة محتاجة تغيير", null, 35, 0, false, null, null),
            ("khaled", 11, "مكتمل", "دهانات", "دهان غرفتين وصالة", "5 شارع الجيش، المنصورة", "البيت محتاج تجديد دهان", "تم دهان الشقة بألوان مودرن", 32, 5, false, null, null),
            ("rana", 12, "مكتمل", "نجارة", "تركيب باب شقة جديد", "10 شارع الجمهورية، بني سويف", "الباب القديم محتاج تغيير", "تم تركيب باب خشب موسكي", 30, 3, false, null, null),
            ("tamer", 13, "ملغى", "تكييف وتبريد", "تركيب تكييف سبليت جديد", "12 شارع الميناء، بورسعيد", "تكييف جديد محتاج تركيب", null, 28, 0, false, null, null),
            ("heba", 5, "مكتمل", "سباكة", "صيانة طرمبة المياه", "30 شارع النيل، المنصورة", "الطرمبة بتعمل صوت عالي", "تم صيانة الطرمبة وتغيير الوشوش", 25, 2, false, null, null),
            ("mosaab", 14, "مكتمل", "تبليط وسيراميك", "تركيب سيراميك مطبخ", "15 شارع الحرية، دمنهور", "المطبخ محتاج سيراميك جديد", "تم تركيب سيراميك أرضيات وحوائط", 22, 4, false, null, null),
            ("reem", 15, "مكتمل", "حدادة", "تصنيع باب حديد للمدخل", "8 شارع النصر، أسيوط", "المدخل محتاج بوابة حديد", "تم تصنيع وتركيب بوابة حديد مزخرفة", 20, 10, false, null, null),
            ("gamal", 16, "مفتوح", "زجاج ومرايا", "تركيب مراية كبيرة للصالة", "12 شارع السوق، المنيا", "مراية كبيرة بديكور للصالة", null, 18, 0, false, null, null),
            ("shaimaa", 17, "مكتمل", "ألمنيوم", "تركيب مطبخ ألمنيوم", "7 شارع الجمهورية، سوهاج", "مطبخ قديم محتاج تجديد", "تم تركيب مطبخ ألمنيوم بالكامل", 15, 8, false, null, null),
            ("walid", 18, "مكتمل", "مكافحة حشرات", "تعقيم فيلا بالكامل", "25 شارع النصر، الفيوم", "فيلا كبيرة محتاجة تعقيم", "تم تعقيم وتطهير الفيلا بالكامل", 12, 2, false, null, null),
            ("eman", 19, "مكتمل", "أمن وكاميرات", "تركيب نظام إنذار للمحل", "10 شارع المعادي، القاهرة", "المحل محتاج نظام إنذار", "تم تركيب نظام إنذار متكامل", 10, 3, false, null, null),
            ("hany", 0, "قيد التنفيذ", "كهرباء", "إصلاح عطل في فيشة الكهرباء", "12 شارع فيصل، الإسكندرية", "الفيشة بتشرر", null, 8, 0, false, null, null),
            ("omnya", 1, "مفتوح", "سباكة", "تسريب من السخان", "8 شارع السوق، القاهرة", "السخان بيسرب من تحت", null, 6, 0, false, null, null),
            ("ashraf", 2, "مكتمل", "دهانات", "دهان سور البيت الخارجي", "30 شارع الجيش، الجيزة", "سور البيت محتاج دهان خارجي", "تم دهان السور بدهان عازل", 5, 4, false, null, null),
            ("marwa", 3, "مرفوض", "نجارة", "تركيب باركيه لغرفة", "5 شارع النيل، القاهرة", "غرفة محتاجة باركيه خشب", null, 4, 0, false, null, null),
            ("sara", 8, "مكتمل", "سباكة", "إصلاح تسريب في حوض المطبخ", "15 شارع مصر الجديدة، القاهرة", "الحوض بيسرب من تحت", "تم تغيير القطعة التالفة", 42, 2, true, 44, "العميل يدعي أن التسريب لسه موجود"),
            ("nourhan", 9, "مكتمل", "جبس وأسقف", "إصلاح سقف جبس متصدع", "22 شارع فيصل، الجيزة", "السقف الجبس فيه تصدعات", "تم إصلاح التصدعات", 34, 4, true, 36, "نزاع على جودة الإصلاح"),
            ("samer", 10, "مفتوح", "كهرباء", "توصيل كهرباء لغرفة جديدة", "10 شارع الحرية، الإسكندرية", "غرفة ملحقة محتاجة كهرباء", null, 3, 0, false, null, null),
            ("nahed", 11, "مفتوح", "دهانات", "دهان شقة إيجار جديد", "8 شارع الجمهورية، المنصورة", "شقة جديدة محتاجة دهان", null, 2, 0, false, null, null),
            ("sara", 12, "مفتوح", "نجارة", "تركيب رفوف في المخزن", "12 شارع التحرير، القاهرة", "المخزن محتاج رفوف تخزين", null, 1, 0, false, null, null),
            ("nourhan", 13, "قيد التنفيذ", "تكييف وتبريد", "شحن فريون تكييف", "5 شارع المحطة، بورسعيد", "التكييف مش بيبرد خالص", null, 145, 0, false, null, null),
            ("amr", 14, "مكتمل", "تبليط وسيراميك", "تركيب سيراميك حمام ضيوف", "12 شارع النصر، دمنهور", "حمام الضيوف محتاج تجديد", "تم تركيب سيراميك أرضيات وحوائط", 108, 4, false, null, null),
            ("dina", 15, "مكتمل", "حدادة", "صناعة سرير حديد", "8 شارع السوق، أسيوط", "سرير حديد مفرد مقاس 100*190", "تم تصنيع سرير حديد فني", 95, 7, false, null, null),
            ("hosam", 7, "مكتمل", "كهرباء", "تركيب دش مركزي", "15 شارع الحرية، الأقصر", "محتاج دش مركزي في الحمام", "تم تركيب دش مركزي مع خلاط", 82, 3, false, null, null),
            ("layla", 1, "مرفوض", "سباكة", "تغير مواسير حديد قديمة", "20 شارع الجمهورية، الإسكندرية", "المواسير الحديد صدأت", null, 72, 0, false, null, null),
            ("khaled", 12, "مكتمل", "نجارة", "تصنيع دولاب ملابس 6 درف", "14 شارع النيل، بني سويف", "دولاب ملابس مودرن", "تم تصنيع وتركيب دولاب 6 درف", 62, 12, false, null, null),
            ("rana", 2, "مكتمل", "دهانات", "دهان سقف وجدران المكتب", "7 شارع التحرير، الجيزة", "مكتب محتاج دهان شامل", "تم دهان المكتب بالكامل", 48, 3, false, null, null)
        };
        var jobs = new List<Job>();
        foreach (var d in defs)
        {
            if (!c.ContainsKey(d.cust)) continue;
            var customer = c[d.cust];
            var craftsman = d.cm < approved.Count ? approved[d.cm] : approved[0];
            var createdAt = BaseDate.AddDays(d.da);
            var completedAt = d.dur > 0 ? createdAt.AddDays(d.dur) : (DateTime?)null;

            jobs.Add(new Job
            {
                CustomerId = customer.Id,
                CraftsmanId = craftsman.Id,
                Status = d.st,
                ServiceType = d.sv,
                Description = d.desc,
                Address = d.addr,
                ProblemDescription = d.prob,
                SolutionDescription = d.sol,
                CreatedAt = createdAt,
                CompletedAt = completedAt,
                IsDisputed = d.disp || d.dispR != null,
                DisputeRaisedAt = d.disp && d.dispD.HasValue ? BaseDate.AddDays(d.dispD.Value) : null,
                DisputeResolvedAt = !d.disp && d.dispR != null && d.dispD.HasValue ? BaseDate.AddDays(d.dispD.Value) : null,
                DisputeResolution = d.dispR
            });
        }

        await _context.Jobs.AddRangeAsync(jobs);
        await _context.SaveChangesAsync();
        _logger.LogInformation("{Count} jobs seeded.", jobs.Count);
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

    // ═══════════════════════════════════════════════════════════
    //  9. REVIEWS (60+)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedReviewsAsync()
    {
        // Detach all tracked entities to avoid 1:1 relationship conflicts
        _context.ChangeTracker.Clear();
        var allJobs = await _context.Jobs.Where(j => j.CompletedAt != null).ToListAsync();
        if (allJobs.Count == 0) return;

        var reviewData = new List<(string[] keywords, int stars, string comment, bool isDeleted)>
        {
            (new[] { "نقاط إضاءة جديدة" }, 5, "الأستاذ محمد إنسان محترم جداً وشغله نظيف وفي الموعد بالضبط. ركب الـسبوت لايت بأحسن شكل. بوصي بيه بثقة.", false),
            (new[] { "حجرة النوم" }, 1, "الشغل بيفضح. اللون مختلف تماماً عن المتفق عليه ورفض يصلح. ضيعت وقتي وفلوسي.", false),
            (new[] { "دواليب مطبخ" }, 4, "شغل كويس وبالموعد بس كان في بعض التفاصيل الصغيرة محتاج ينتبهلها.", false),
            (new[] { "صيانة دورية شاملة" }, 2, "محتوى مخالف — تم حذفه", true),
            (new[] { "لوحة توزيع كهرباء جديدة" }, 5, "فنان في شغله! ركب اللوحة وشرحلي كل حاجة. محترف جداً.", false),
            (new[] { "دهان كامل لشقة 3 غرف" }, 2, "اضطررت أفتح نزاع لأن الشغل بجودة أقل من المتفق عليه. الإدارة حلّت الموضوع.", false),
            (new[] { "دهان حوائط الصالة" }, 4, "شغل نضيف والحرفي ملتزم. بس في بعض الزوايا محتاجة ضبط.", false),
            (new[] { "تسريب مياه من الحمام" }, 5, "أحسن سباك تعاملت معاه. شغل محترف ونظافة بعد الشغل.", false),
            (new[] { "بالوعة المطبخ مسدودة" }, 5, "جاء بسرعة وحل المشكلة في نص ساعة. سعر معقول جداً. شكراً!", false),
            (new[] { "تركيب سخان غاز جديد" }, 4, "تركيب ممتاز والتوصيلات أمان. بس تأخر شوية عن الموعد.", false),
            (new[] { "أسقف جبس معلقة" }, 5, "شغل جبس رائع وجمال. الصالة بقت تحفة بفضل حسن شحاتة.", false),
            (new[] { "تمديد كهرباء لغرفة جديدة" }, 3, "الشغل كويس لكن الأسعار غالية شوية. عموماً أنجز المطلوب.", false),
            (new[] { "واجهة المبنى الخارجي" }, 4, "دهان الواجهة طلع زي الفل. نفس اللون بالظبط والجودة ممتازة.", false),
            (new[] { "مكتبة حائط" }, 5, "مكتبة رائعة — خشب طبيعي ودقة في التفاصيل. تامر فنان!", false),
            (new[] { "فلتر مياه مركزي" }, 4, "تركيب محترف والفلتر شغال بكفاءة. شرح لي كيفية الصيانة.", false),
            (new[] { "غرفة نوم أطفال بديكور" }, 5, "رسومات كرتونية رائعة — الأطفال فرحوا جداً. شكراً عبدالله!", false),
            (new[] { "تكيف سبليت" }, 4, "صيانة ممتازة — التكييف رجع يبرد زي الأول. سعر مناسب.", false),
            (new[] { "درابزين حديد" }, 5, "درابزين فخم جداً — شغل حدادة ممتاز وتفاصيل مزخرفة جميلة.", false),
            (new[] { "شبابيك ألمنيوم" }, 4, "شبابيك ألمنيوم جودة ممتازة والعزل كويس. سعر معقول.", false),
            (new[] { "رش وتعقيم شقة" }, 5, "خلّصنا من الحشرات والحمد لله. مواد آمنة ورائحة منعشة.", false),
            (new[] { "كاميرات مراقبة خارجية" }, 5, "كاميرات واضحة جداً ونظام التسجيل شغال بكفاءة. جورج محترف.", false),
            (new[] { "غرفة نوم كاملة" }, 3, "التركيب كويس لكن فيه بعض الخدوش في الباب. نبهت الحرفي.", false),
            (new[] { "طقم حمام كامل" }, 5, "تركيب طقم الحمام ممتاز — المواسير مضبوطة والتسريب اختفى.", false),
            (new[] { "ديكور جبس لغرفة المعيشة" }, 5, "ديكور جبس بإضاءة LED — إبداع حقيقي. الصالة بقت تحفة!", false),
            (new[] { "دهان غرفتين وصالة" }, 4, "الشقة تغيرت بالكامل — ألوان جميلة وشغل نضيف.", false),
            (new[] { "باب شقة جديد" }, 4, "باب خشب موسكي ثقيل وشيك جداً. التركيب محترف. بس السعر غالي.", false),
            (new[] { "طرمبة المياه" }, 4, "صيانة طرمبة المياه — الصوت اختفي والحمد لله. شكراً لسرعة الاستجابة.", false),
            (new[] { "سيراميك مطبخ" }, 5, "سيراميك المطبخ ولا غلطة — معلم شاطر وملتزم بشغله جداً.", false),
            (new[] { "باب حديد للمدخل" }, 5, "بوابة حديد رهيبة — فخر وشكل جميل. حدادة فاخرة.", false),
            (new[] { "مطبخ ألمنيوم" }, 4, "مطبخ ألمنيوم عملي وجميل. التركيب محترف والخامات كويسة.", false),
            (new[] { "تعقيم فيلا" }, 5, "تعقيم شامل للفيلا — فريق محترف ومعدات حديثة. أنصح بالتعامل.", false),
            (new[] { "نظام إنذار للمحل" }, 5, "نظام إنذار متكامل وحساسات. اطمنان على المحل. شكراً!", false),
            (new[] { "إصلاح تسريب في حوض المطبخ", "تسريب من السخان" }, 1, "التسريب لسه موجود بعد الإصلاح! خسرت فلوسي على الفاضي.", false),
            (new[] { "إصلاح سقف جبس متصدع" }, 3, "الإصلاح مش متقن — فيه تشققات ظهرت تاني. محتاج يعيد الشغل.", false),
            (new[] { "سيراميك حمام ضيوف" }, 5, "شغل سيراميك ولا أروع — مقاسات مضبوطة قصاد بعضها. شكراً وائل!", false),
            (new[] { "سرير حديد" }, 5, "سرير حديد فني وأصلي — الورشة بتاع فادي بتعمل شغل نضيف جداً.", false),
            (new[] { "دش مركزي" }, 4, "الدش المركزي شيك جداً. التركيب محترف. بس سعره غالي.", false),
            (new[] { "دولاب ملابس 6 درف" }, 5, "دولاب 6 درف مصنوع بعناية — خشب ثقيل وتفصيل دقيق. تامر فنان!", false),
            (new[] { "دهان سقف وجدران المكتب" }, 4, "دهان المكتب بالكامل في وقت قياسي. شغل نظيف وجودة ممتازة.", false),
            (new[] { "تمديد كهرباء" }, 4, "كهربائي شاطر ونضيف. التوصيلات مضبوطة ومرتبة. سعر مناسب.", false),
            (new[] { "أسقف جبس", "ديكور جبس" }, 5, "حسن شحاتة فنان — أسقف جبس رائعة وشغل متقن جداً. أنصح بالتعامل.", false),
            (new[] { "دهان واجهة", "دهان سور", "دهان شقة" }, 4, "دهان بجودة ممتازة — الألوان مضبوطة والالتزام بالمواعيد.", false),
            (new[] { "مفاتيح", "برايز", "نجفة" }, 3, "شغل كهرباء عادي — أنجز المطلوب لكن في تأخير.", false),
            (new[] { "سباك", "تسريب", "خلاط", "مواسير" }, 5, "أفضل سباك تعاملت معه في القاهرة — محترف وأمين.", false),
            (new[] { "دهانات", "نقاشة", "ديكور" }, 3, "دهان متوسط الجودة — مقبول لكن كان ممكن يكون أفضل.", false),
        };

        var reviews = new List<Review>();
        var usedJobIds = new HashSet<int>();
        foreach (var (keywords, stars, comment, isDeleted) in reviewData)
        {
            var match = allJobs.FirstOrDefault(j => !usedJobIds.Contains(j.Id) && keywords.Any(k => j.Description.Contains(k)));
            if (match == null) continue;
            usedJobIds.Add(match.Id);
            bool del = isDeleted;
            reviews.Add(new Review
            {
                JobId = match.Id,
                CustomerId = match.CustomerId,
                CraftsmanId = match.CraftsmanId!.Value,
                Stars = stars,
                Comment = comment,
                IsDeleted = del,
                DeletedAt = del ? match.CompletedAt!.Value.AddDays(3) : (DateTime?)null,
                DeletedByAdminId = del ? 1 : (int?)null,
                DeletionReason = del ? "التقييم يحتوي على ألفاظ مسيئة تخالف شروط الاستخدام" : null,
                CreatedAt = match.CompletedAt!.Value.AddDays(1)
            });
        }

        await _context.Reviews.AddRangeAsync(reviews);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Reviews seeded: {N}", reviews.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  10. RECALCULATE RATINGS
    // ═══════════════════════════════════════════════════════════
    private async Task RecalculateRatingsAsync()
    {
        var craftsmen = await _context.Craftsmen.IgnoreQueryFilters().Include(c => c.Reviews).ToListAsync();
        foreach (var c in craftsmen)
        {
            var active = c.Reviews.Where(r => !r.IsDeleted).ToList();
            if (active.Count > 0)
                c.Rating = Math.Round((decimal)active.Average(r => r.Stars), 2);
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation("Ratings recalculated.");
    }

    // ═══════════════════════════════════════════════════════════
    //  11. CONVERSATIONS (one per job with craftsman)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedConversationsAsync()
    {
        var eligibleJobs = await _context.Jobs.IgnoreQueryFilters()
            .Where(j => j.CraftsmanId != null && j.Status != "ملغى")
            .OrderBy(j => j.Id).Take(50).ToListAsync();

        foreach (var job in eligibleJobs)
        {
            _context.Conversations.Add(new Conversation
            {
                JobId = job.Id,
                CustomerId = job.CustomerId,
                CraftsmanId = job.CraftsmanId!.Value,
                CreatedAt = job.CreatedAt.AddMinutes(5),
                LastMessageAt = job.CreatedAt.AddMinutes(60),
                UpdatedAt = job.CreatedAt.AddMinutes(60)
            });
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation("Conversations seeded: {N}", eligibleJobs.Count);
    }
    // ═══════════════════════════════════════════════════════════
    //  12. MESSAGES (realistic Arabic chat per conversation)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedMessagesAsync()
    {
        var conversations = await _context.Conversations.IgnoreQueryFilters()
            .Include(c => c.Job).Include(c => c.Craftsman)
            .OrderBy(c => c.Id).ToListAsync();

        var craftUserIds = await _context.Craftsmen.IgnoreQueryFilters()
            .ToDictionaryAsync(c => c.Id, c => c.UserId);

        var chatScripts = new[]
        {
            "أهلاً، محتاج حد يصلح التسريب في الحمام بسرعة",
            "أهلاً بيك، شفت الطلب. امتى تقدر تستقبلني؟",
            "النهارده بعد الضهر مناسب؟",
            "تمام خد عندك. هكون عندك الساعة 4 العصر.",
            "أوكي، شكراً جزيلاً",
            "العفو — تحت أمرك في أي وقت",

            "لوحة الكهرباء الرئيسية عاطلة والكهربا مقطوعة خالص!",
            "فهمت المشكلة — لازم أشوف اللوحة قبل ما أعطيك سعر",
            "طيب امتى تقدر تجي؟",
            "بكره الصبح الساعة 9 ينفع؟",
            "ينفع تمام، شكراً",
            "أنا هنا دلوقتي — نزل افتح عشان أشوف اللوحة",

            "محتاج تركيب سبوت لايت في 3 غرف",
            "كام غرفة وكام نقطة تقريباً؟",
            "3 غرف تقريباً 12 نقطة",
            "تمام السعر 600 جنيه شامل المواد",
            "مقبول. يوم الخميس ينفع؟",
            "ينفع تمام — هكون عندكم",

            "مرحبا، مطلوب صيانة دورية للكهرباء",
            "هل عندك وقت الأسبوع الجاي؟",
            "أيوه أي يوم من الأحد للأربع",
            "هيجي فني يوم الاثنين الساعة 11",
            "تمام بنتظركم",

            "الشغل خلص لكن مش تمام — المكيف لسه مش بيبرد",
            "إيه المشكلة بالظبط؟ أنا شغلي مضبوط",
            "جيت بعد الشغل ولسه الحر زي ما هو",
            "أنا كنت عندك واشتغلت على القاطع. محتاج تاني كشف",
            "هفتح نزاع لو مش اتحل",
            "خلي إدارة حرفي تشوف المشكلة وتقرر",

            "الدهان مش بالمواصفات — اللون مختلف!",
            "الشغل اتعمل صح والمواد زي المتفق",
            "اللون مختلف تماماً عن اللي اخترناه",
            "هتكلم الإدارة يشوفوا الموضوع",
            "تمام وأنا تحت أمر الإدارة",

            "محتاج أعمل لوحة توزيع كهرباء جديدة",
            "أنا متاح. كام كيلو واط محتاج؟",
            "مش عارف — ممكن تيجي تكشف؟",
            "أيوه تمام — بكره الصبح",
            "ممتاز! بنتظرك",

            "محتاج نجار تركيب دواليب مطبخ",
            "أنا متخصص — كام متر المطبخ؟",
            "4 متر تقريباً",
            "السعر هيكون 1500 جنيه للمتر شامل التركيب",
            "تمام متفقين — امتى تبدأ؟",
            "أول الأسبوع إن شاء الله",

            "الحمام بيسرب من تحت الحوض",
            "أنا متخصص في السباكة. أبعتي صورة التسريب",
            "تمام هصوره دلوقتي",
            "فهمت المشكلة — تغيير بلاعة الحوض",
            "كام تكلفته؟",
            "250 جنيه كل حاجة شاملة",
            "تمام — اتفقنا",

            "محتاج فني تكييف يصلح مكيف غرفة النوم",
            "أنا موجود — المكيف عامل إيه بالظبط؟",
            "بيشتغل شوية ويوقف — مش بيبرد",
            "غالباً عاوز شحن فريون وتنظيف فلاتر",
            "كام السعر؟",
            "350 ج تشمل الكشف والشحن والتنظيف",
            "ماشي — يلا اتفقنا"
        };

        var allMessages = new List<Message>();
        int scriptIdx = 0;

        foreach (var conv in conversations.Take(15))
        {
            if (!craftUserIds.TryGetValue(conv.CraftsmanId, out var craftUserId))
                continue;

            int msgsInConv = Rng.Next(3, 7);
            DateTime? lastMsgTime = null;

            for (int i = 0; i < msgsInConv && scriptIdx < chatScripts.Length; i++, scriptIdx++)
            {
                bool fromCustomer = i % 2 == 0;
                var senderId = fromCustomer ? conv.CustomerId : craftUserId;
                var sentAt = conv.CreatedAt.AddMinutes(i * Rng.Next(5, 30));
                lastMsgTime = sentAt;

                allMessages.Add(new Message
                {
                    ConversationId = conv.Id,
                    SenderId = senderId,
                    Content = chatScripts[scriptIdx % chatScripts.Length],
                    MessageType = Rng.Next(10) == 0 ? "image" : "text",
                    IsRead = i < msgsInConv - 1 || Rng.Next(2) == 0,
                    SentAt = sentAt
                });
            }

            if (lastMsgTime.HasValue)
            {
                conv.LastMessageAt = lastMsgTime;
                conv.UpdatedAt = lastMsgTime.Value;
            }
        }

        await _context.Messages.AddRangeAsync(allMessages);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Messages seeded: {N}", allMessages.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  13. NOTIFICATIONS (150+)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedNotificationsAsync()
    {
        var craftsmen = await _context.Craftsmen.IgnoreQueryFilters().Include(c => c.User).ToListAsync();
        var customers = await _context.Users.IgnoreQueryFilters().Where(u => u.Role == "customer").ToListAsync();
        var jobs = await _context.Jobs.IgnoreQueryFilters().ToListAsync();
        var conversations = await _context.Conversations.IgnoreQueryFilters().ToListAsync();
        User CU(string email) => customers.First(u => u.Email == email);
        Craftsman CM(string email) => craftsmen.First(c => c.User.Email == email);
        Job J(string frag) => jobs.First(j => j.Description.Contains(frag));

        var notifs = new List<Notification>();

        // Existing 9 notifications
        notifs.AddRange(new[]
        {
            new Notification { UserId = CM("ahmed.ali@gmail.com").UserId, Title = "طلب التسجيل قيد المراجعة", Body = "تم استلام طلب تسجيلك كحرفي وهو قيد المراجعة.", Type = "registration_pending", IsRead = false, CreatedAt = CM("ahmed.ali@gmail.com").CreatedAt.AddMinutes(10) },
            new Notification { UserId = CU("sara.ahmed@gmail.com").Id, Title = "تم قبول طلبك", Body = "قام الحرفي إبراهيم نصر بقبول طلب الخدمة.", Type = "job_accepted", RelatedJobId = J("صيانة دورية شاملة").Id, IsRead = true, CreatedAt = J("صيانة دورية شاملة").CreatedAt.AddHours(2) },
            new Notification { UserId = CM("mohamed.hassan@gmail.com").UserId, Title = "تم اعتمادك كحرفي", Body = "مبروك! تم اعتماد طلبك بنجاح.", Type = "approved", IsRead = true, CreatedAt = CM("mohamed.hassan@gmail.com").CreatedAt.AddDays(2) },
            new Notification { UserId = CM("hussien.reda@gmail.com").UserId, Title = "تم رفض طلب التسجيل", Body = "نأسف لإبلاغك بأنه تم رفض طلب تسجيلك.", Type = "rejected", IsRead = false, CreatedAt = CM("hussien.reda@gmail.com").CreatedAt.AddDays(3) },
            new Notification { UserId = CU("nourhan.mohamed@gmail.com").Id, Title = "تم إنجاز طلبك", Body = "أنهى الحرفي العمل. يمكنك تقييم الخدمة.", Type = "job_completed", RelatedJobId = J("نقاط إضاءة جديدة").Id, IsRead = true, CreatedAt = J("نقاط إضاءة جديدة").CreatedAt.AddHours(2) },
            new Notification { UserId = CM("ibrahim.nasr@gmail.com").UserId, Title = "تم فتح نزاع", Body = "تم فتح نزاع على طلب الخدمة.", Type = "dispute_opened", RelatedJobId = J("إصلاح عطل في دائرة الطاقة").Id, IsRead = false, CreatedAt = J("إصلاح عطل في دائرة الطاقة").CreatedAt.AddHours(2) },
            new Notification { UserId = CU("sara.ahmed@gmail.com").Id, Title = "تم حل النزاع", Body = "تم حل النزاع لصالحك.", Type = "dispute_resolved", RelatedJobId = J("دهان كامل لشقة 3 غرف").Id, IsRead = true, CreatedAt = J("دهان كامل لشقة 3 غرف").CreatedAt.AddDays(1) }
        });

        // Additional notifications from new jobs
        var notifTemplates = new[]
        {
            ("job_accepted", "تم قبول طلب الخدمة", "قام الحرفي بقبول طلب الخدمة الخاص بك."),
            ("job_completed", "تم إنجاز الخدمة", "تم إنجاز الخدمة بنجاح. يرجى تقييم الحرفي."),
            ("new_message", "رسالة جديدة", "لديك رسالة جديدة من الحرفي."),
            ("job_accepted", "تم قبول طلبك", "تم قبول طلبك وجاري التنسيق مع الحرفي."),
            ("dispute_opened", "فتح نزاع", "تم فتح نزاع على طلب الخدمة رقم {0}."),
        };

        int jobIdx = 0;
        for (int i = 0; i < 120 && jobIdx < jobs.Count; i++)
        {
            var job = jobs[jobIdx % jobs.Count];
            var (typ, title, body) = notifTemplates[i % notifTemplates.Length];
            var custNotif = new Notification
            {
                UserId = job.CustomerId,
                Title = title,
                Body = string.Format(body, job.Id),
                Type = typ,
                RelatedJobId = job.Id,
                IsRead = Rng.Next(3) > 0,
                CreatedAt = job.CreatedAt.AddHours(Rng.Next(1, 48))
            };
            notifs.Add(custNotif);

            if (job.CraftsmanId.HasValue)
            {
                var craftUser = craftsmen.FirstOrDefault(c => c.Id == job.CraftsmanId.Value);
                if (craftUser != null)
                {
                    notifs.Add(new Notification
                    {
                        UserId = craftUser.UserId,
                        Title = title,
                        Body = string.Format(body, job.Id),
                        Type = typ,
                        RelatedJobId = job.Id,
                        IsRead = Rng.Next(2) == 0,
                        CreatedAt = job.CreatedAt.AddHours(Rng.Next(2, 72))
                    });
                }
            }
            jobIdx++;
        }

        await _context.Notifications.AddRangeAsync(notifs);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Notifications seeded: {N}", notifs.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  14. ADMIN AUDIT LOGS (30+)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedAdminAuditLogsAsync()
    {
        var admin = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Role == "admin");
        if (admin is null) return;

        var craftsmen = await _context.Craftsmen.IgnoreQueryFilters().Include(c => c.User).ToListAsync();
        var users = await _context.Users.IgnoreQueryFilters().ToListAsync();
        var reviews = await _context.Reviews.IgnoreQueryFilters().ToListAsync();
        var jobs = await _context.Jobs.IgnoreQueryFilters().ToListAsync();
        Craftsman CM(string e) => craftsmen.First(c => c.User.Email == e);

        var actions = new[]
        {
            ("approve_craftsman", "Craftsman", CM("mohamed.hassan@gmail.com").Id, "Approved — verification passed"),
            ("reject_craftsman", "Craftsman", CM("hussien.reda@gmail.com").Id, "Rejected — invalid ID documents"),
            ("deactivate_user", "User", users.First(u => u.Email == "maryam.ali@gmail.com").Id, "Deactivated — platform misuse"),
            ("delete_review", "Review", reviews.FirstOrDefault(r => r.IsDeleted)?.Id ?? 1, "Deleted — inappropriate content"),
            ("flag_dispute", "Job", jobs.First(j => j.IsDisputed).Id, "Flagged dispute — quality complaint"),
            ("resolve_dispute", "Job", jobs.First(j => j.DisputeResolution != null && j.DisputeResolution.Contains("العميل")).Id, "Resolved — customer favored"),
            ("suspend_craftsman", "Craftsman", CM("mostafa.mahmoud@gmail.com").Id, "Suspended — repeated complaints"),
            ("delete_craftsman", "Craftsman", CM("kareem.samy@gmail.com").Id, "Soft-deleted — misconduct"),
            ("approve_craftsman", "Craftsman", CM("youssef.adel@gmail.com").Id, "Approved — all documents valid"),
            ("approve_craftsman", "Craftsman", CM("michael.awad@gmail.com").Id, "Approved — verified via phone"),
            ("approve_craftsman", "Craftsman", CM("hassan.shahat@gmail.com").Id, "Approved — 10yr experience confirmed"),
            ("approve_craftsman", "Craftsman", CM("nader.hamdy@gmail.com").Id, "Approved — engineering degree verified"),
            ("approve_craftsman", "Craftsman", CM("sameh.fawzy@gmail.com").Id, "Approved — portfolio reviewed"),
            ("approve_craftsman", "Craftsman", CM("tamer.nabil@gmail.com").Id, "Approved — client references checked"),
            ("approve_craftsman", "Craftsman", CM("adel.makram@gmail.com").Id, "Approved — certification valid"),
            ("approve_craftsman", "Craftsman", CM("wael.gamal@gmail.com").Id, "Approved — 14yr experience"),
            ("approve_craftsman", "Craftsman", CM("fady.shafik@gmail.com").Id, "Approved — workshop inspection passed"),
            ("approve_craftsman", "Craftsman", CM("marwan.atef@gmail.com").Id, "Approved — valid trade license"),
            ("approve_craftsman", "Craftsman", CM("sherif.ashraf@gmail.com").Id, "Approved — insurance documents OK"),
            ("approve_craftsman", "Craftsman", CM("khaled.nasr@gmail.com").Id, "Approved — health ministry cert"),
            ("approve_craftsman", "Craftsman", CM("george.ramzy@gmail.com").Id, "Approved — security clearance OK"),
            ("resolve_dispute", "Job", jobs.First(j => j.Description.Contains("درابزين")).Id, "Resolved — mutual agreement"),
            ("resolve_dispute", "Job", jobs.First(j => j.DisputeResolution != null && j.DisputeResolution.Contains("نزاع")).Id, "Resolved — refund processed"),
            ("deactivate_user", "User", users.First(u => u.Email == "shaimaa.ahmed@gmail.com").Id, "Deactivated — at user request"),
            ("deactivate_user", "User", users.First(u => u.Email == "marwa.khaled@gmail.com").Id, "Deactivated — suspicious activity"),
            ("update_job_status", "Job", jobs.First(j => j.Description.Contains("صيانة دورية")).Id, "Status updated — forced complete"),
            ("update_job_status", "Job", jobs.First(j => j.Description.Contains("مفاتيح")).Id, "Status updated — reopened"),
            ("update_feature_flag", "FeatureFlag", 0, "Disabled VoiceSearchEnabled — perf issues"),
            ("update_feature_flag", "FeatureFlag", 1, "Enabled RAGSolutionEnabled — production ready"),
            ("config_service_type", "ServiceType", 0, "Added new service type: أمن وكاميرات"),
            ("config_city", "City", 1, "Added new city: الغردقة"),
        };

        var logs = new List<AdminAuditLog>();
        foreach (var (action, targetType, targetId, notes) in actions)
        {
            logs.Add(new AdminAuditLog
            {
                AdminId = admin.Id,
                Action = action,
                TargetType = targetType,
                TargetId = targetId,
                Notes = notes,
                IpAddress = "197.58.112.44",
                CreatedAt = BaseDate.AddDays(Rng.Next(10, 170))
            });
        }

        await _context.AdminAuditLogs.AddRangeAsync(logs);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Admin audit logs seeded: {N}", logs.Count);
    }
    // ═══════════════════════════════════════════════════════════
    //  15. REPORTS (25+)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedReportsAsync()
    {
        var customers = await _context.Users.IgnoreQueryFilters().Where(u => u.Role == "customer").ToListAsync();
        var craftsmen = await _context.Craftsmen.IgnoreQueryFilters().Include(c => c.User).ToListAsync();
        var admin = await _context.Users.IgnoreQueryFilters().FirstAsync(u => u.Role == "admin");
        User CU(string e) => customers.First(u => u.Email == e);
        Craftsman CM(string e) => craftsmen.First(c => c.User.Email == e);

        var reportDefs = new List<(int uid, string ttype, int tid, string reason, string status, int? resolvedBy, string? notes, DateTime? resolvedAt)>
        {
            (CU("sara.ahmed@gmail.com").Id, "Craftsman", CM("mostafa.mahmoud@gmail.com").Id, "الحرفي تأخر كثيراً ولم يُبلغ وطلب مبلغاً إضافياً", "pending", null, null, null),
            (CU("nourhan.mohamed@gmail.com").Id, "Craftsman", CM("abdallah.khaled@gmail.com").Id, "الحرفي استخدم مواد رديئة وخالف شروط العقد", "resolved", admin.Id, "تم التحقق وتوجيه تحذير", BaseDate.AddMonths(2).AddDays(5)),
            (CU("amr.ali@gmail.com").Id, "Craftsman", CM("kareem.samy@gmail.com").Id, "الحرفي لم يكمل العمل واختفى", "resolved", admin.Id, "تم حذف الحساب بعد التحقق", BaseDate.AddMonths(4)),
            (CU("dina.mahmoud@gmail.com").Id, "Job", 1, "الخدمة لم تنفذ بالشكل المتفق عليه", "pending", null, null, null),
            (CU("hosam.adel@gmail.com").Id, "Craftsman", CM("mostafa.mahmoud@gmail.com").Id, "طلب مبلغاً إضافياً بعد البدء في العمل", "pending", null, null, null),
            (CU("layla.karim@gmail.com").Id, "Craftsman", CM("abdallah.khaled@gmail.com").Id, "جودة العمل سيئة جداً ويحتاج إعادة", "pending", null, null, null),
            (CU("khaled.omar@gmail.com").Id, "Review", 1, "التقييم غير لائق ويحتوي على إساءة", "resolved", admin.Id, "تم حذف التقييم", BaseDate.AddDays(100)),
            (CU("rana.adel@gmail.com").Id, "Craftsman", CM("mostafa.mahmoud@gmail.com").Id, "لم يحضر في الموعد المحدد من غير اعتذار", "pending", null, null, null),
            (CU("tamer.hassan@gmail.com").Id, "Craftsman", CM("ibrahim.nasr@gmail.com").Id, "الأسعار المتفق عليها تغيرت بعد بدء العمل", "resolved", admin.Id, "تم حل الموضوع بالتراضي", BaseDate.AddDays(80)),
            (CU("heba.nabil@gmail.com").Id, "Craftsman", CM("kareem.samy@gmail.com").Id, "استخدم قطع غيار مغشوشة", "pending", null, null, null),
            (CU("mosaab.ahmed@gmail.com").Id, "Craftsman", CM("mostafa.mahmoud@gmail.com").Id, "تسبب في تلف جزء من الحائط ولم يصلحه", "pending", null, null, null),
            (CU("reem.yasser@gmail.com").Id, "Job", 2, "المواد المستخدمة غير مطابقة للمواصفات", "resolved", admin.Id, "تم التعويض", BaseDate.AddDays(60)),
            (CU("eman.ali@gmail.com").Id, "Craftsman", CM("abdallah.khaled@gmail.com").Id, "دهان الحوائط تقشر بعد أسبوع", "pending", null, null, null),
            (CU("hany.kamal@gmail.com").Id, "Craftsman", CM("mostafa.mahmoud@gmail.com").Id, "الحرفي غير مؤهل — معرفش يعمل الشغل", "resolved", admin.Id, "تم تعليق الحساب", BaseDate.AddMonths(3).AddDays(10)),
            (CU("omnya.reda@gmail.com").Id, "Craftsman", CM("ibrahim.nasr@gmail.com").Id, "المواعيد غير محترمة — تأخر 3 ساعات", "pending", null, null, null),
            (CU("ashraf.mahmoud@gmail.com").Id, "Craftsman", CM("kareem.samy@gmail.com").Id, "الشغل مش نضيف واتكسفت قدام الضيوف", "pending", null, null, null),
            (CU("samer.fathy@gmail.com").Id, "Craftsman", CM("abdallah.khaled@gmail.com").Id, "رفض إصلاح عيب في الشغل", "pending", null, null, null),
            (CU("nahed.adel@gmail.com").Id, "Craftsman", CM("mostafa.mahmoud@gmail.com").Id, "النقاشة مش مضبوطة والألوان مش مظبوطة", "pending", null, null, null)
        };

        var reports = new List<Report>();
        foreach (var (uid, ttype, tid, reason, status, resolvedBy, notes, resolvedAt) in reportDefs)
        {
            reports.Add(new Report
            {
                ReportedByUserId = uid,
                TargetType = ttype,
                TargetId = tid,
                Reason = reason,
                Status = status,
                ResolvedByAdminId = resolvedBy,
                ResolutionNotes = notes,
                CreatedAt = BaseDate.AddDays(Rng.Next(30, 170)),
                ResolvedAt = resolvedAt
            });
        }

        await _context.Reports.AddRangeAsync(reports);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Reports seeded: {N}", reports.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  16. AI CHAT MESSAGES (50+)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedAIChatMessagesAsync()
    {
        var users = await _context.Users.IgnoreQueryFilters().Where(u => u.Role == "customer" && u.IsActive).ToListAsync();
        if (users.Count == 0) return;

        var sessions = new[]
        {
            "a1b2c3d4-1111-4000-8000-000000000001",
            "a1b2c3d4-1111-4000-8000-000000000002",
            "a1b2c3d4-1111-4000-8000-000000000003",
            "a1b2c3d4-1111-4000-8000-000000000004",
            "a1b2c3d4-1111-4000-8000-000000000005",
            "a1b2c3d4-1111-4000-8000-000000000006",
            "a1b2c3d4-1111-4000-8000-000000000007",
            "a1b2c3d4-1111-4000-8000-000000000008"
        };

        var aiDialogues = new[]
        {
            "عاوز سباك في القاهرة", "فهمت! أنت محتاج سباك في القاهرة. إيه المشكلة بالظبط؟", "الحنفية بتقطر ومش عارف أصلحها", "تمام! أنا هدلك على أفضل السباكين في المنطقة.",
            "عندي مشكلة في الكهرباء", "أهلاً! ممكن توضح أكثر إيه المشكلة؟", "الفيشة بتشرر لما أشغل المكيف", "خطر! ده محتاج كهربائي فوراً. هدلك على أقرب كهربائي.",
            "محتاج حد يصلح تكييف", "أهلاً! التكييف عامل إيه؟", "مش بيبرد خالص وبيسرب مية", "غالباً محتاج شحن فريون. هدلك على فني تكييف محترف.",
            "عاوز أدهن الشقة", "أهلاً! دهان شقة كام غرفة؟", "3 غرف وصالة", "ممتاز! هدلك على أحسن النقاشين في منطقتك بأسعار مناسبة.",
            "عندي مشكلة في الحمام", "إيه المشكلة بالظبط؟", "سقف الحمام بينقط", "ده محتاج سباك يكشف تسريبات. هل جربت تصلحها بنفسك؟",
            "محتاج نجار", "أهلاً! محتاج نجار تعمل إيه؟", "دولاب ملابس 6 درف", "تمام! هدلك على نجارين متخصصين في غرف النوم.",
            "كاميرات مراقبة", "أهلاً! محتاج كاميرات للمنزل ولا للمحل؟", "للمحل — 4 كاميرات", "ممتاز! هدلك على متخصصين في أنظمة الأمن والمراقبة.",
            "مكافحة حشرات", "أهلاً! إيه نوع الحشرات اللي عندك؟", "صراصير ونمل في المطبخ", "ممكن ترش بنفسك أو استعين بمتخصص. هدلك على شركات مكافحة."
        };

        var msgs = new List<AIChatMessage>();
        int userIdx = 0;
        foreach (var sessionId in sessions)
        {
            var user = users[userIdx % users.Count];
            userIdx++;
            for (int i = 0; i < 6 && (i * 2 + 1) < aiDialogues.Length; i++)
            {
                int baseIdx = (Array.IndexOf(sessions, sessionId) * 6 + i) % (aiDialogues.Length / 2) * 2;
                var userMsg = aiDialogues.ElementAtOrDefault(baseIdx);
                var assistantMsg = aiDialogues.ElementAtOrDefault(baseIdx + 1);
                if (userMsg == null || assistantMsg == null) break;

                msgs.Add(new AIChatMessage { UserId = user.Id, SessionId = sessionId, Role = "user", Content = userMsg, TokensUsed = Rng.Next(20, 80), CreatedAt = BaseDate.AddMonths(3).AddMinutes(i * 5) });
                msgs.Add(new AIChatMessage { UserId = user.Id, SessionId = sessionId, Role = "assistant", Content = assistantMsg, ToolUsed = "CraftsmanSearchTool", TokensUsed = Rng.Next(100, 300), CreatedAt = BaseDate.AddMonths(3).AddMinutes(i * 5 + 1) });
            }
        }

        await _context.AIChatMessages.AddRangeAsync(msgs);
        await _context.SaveChangesAsync();
        _logger.LogInformation("AI chat messages seeded: {N}", msgs.Count);
    }
    // ═══════════════════════════════════════════════════════════
    //  17. MEDIA FILES (40+)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedMediaFilesAsync()
    {
        var users = await _context.Users.IgnoreQueryFilters().Where(u => u.IsActive).ToListAsync();
        var craftsmen = await _context.Craftsmen.IgnoreQueryFilters().Where(c => !c.IsDeleted).ToListAsync();
        var jobs = await _context.Jobs.IgnoreQueryFilters().OrderBy(j => j.Id).Take(30).ToListAsync();

        var files = new List<MediaFile>();
        int uid = 0, cid = 0, jid = 0;

        // Profile images for users
        foreach (var u in users.Take(20))
        {
            files.Add(new MediaFile
            {
                FileName = $"profile_{u.Id}.jpg",
                FileUrl = $"https://res.cloudinary.com/harfi/image/upload/profiles/user_{u.Id}.jpg",
                FileType = "image/jpeg",
                EntityType = "user",
                EntityId = u.Id,
                UploadedBy = u.Id,
                CreatedAt = BaseDate.AddDays(Rng.Next(5, 150))
            });
            uid++;
        }

        // National ID images for craftsmen
        foreach (var c in craftsmen.Take(20))
        {
            files.Add(new MediaFile
            {
                FileName = $"nid_{c.Id}.jpg",
                FileUrl = $"https://res.cloudinary.com/harfi/image/upload/nids/craftsman_{c.Id}.jpg",
                FileType = "image/jpeg",
                EntityType = "craftsman",
                EntityId = c.Id,
                UploadedBy = c.UserId,
                CreatedAt = BaseDate.AddDays(Rng.Next(5, 150))
            });
            cid++;
        }

        // Job images (problem photos)
        foreach (var j in jobs.Take(15))
        {
            files.Add(new MediaFile
            {
                FileName = $"job_{j.Id}_problem.jpg",
                FileUrl = $"https://res.cloudinary.com/harfi/image/upload/jobs/{j.Id}.jpg",
                FileType = Rng.Next(3) == 0 ? "image/png" : "image/jpeg",
                EntityType = "job",
                EntityId = j.Id,
                UploadedBy = j.CustomerId,
                CreatedAt = j.CreatedAt
            });
            jid++;
        }

        await _context.MediaFiles.AddRangeAsync(files);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Media files seeded: {N}", files.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  18. REFRESH TOKENS (30+ — active + expired)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedRefreshTokensAsync()
    {
        var users = await _context.Users.IgnoreQueryFilters().Where(u => u.IsActive).ToListAsync();
        var tokens = new List<RefreshToken>();

        foreach (var u in users.Take(25))
        {
            // Active token
            tokens.Add(new RefreshToken
            {
                UserId = u.Id,
                Token = Guid.NewGuid().ToString().Replace("-", "") + "=",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow.AddDays(-Rng.Next(1, 5))
            });
            // Expired token
            tokens.Add(new RefreshToken
            {
                UserId = u.Id,
                Token = Guid.NewGuid().ToString().Replace("-", "") + "=",
                ExpiresAt = DateTime.UtcNow.AddDays(-15),
                IsRevoked = Rng.Next(2) == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            });
        }

        await _context.RefreshTokens.AddRangeAsync(tokens);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Refresh tokens seeded: {N}", tokens.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  19. EMAIL VERIFICATIONS (20+)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedEmailVerificationsAsync()
    {
        var users = await _context.Users.IgnoreQueryFilters().Where(u => !u.IsDeleted).ToListAsync();
        var codes = new List<EmailVerification>();

        foreach (var u in users.Take(20))
        {
            codes.Add(new EmailVerification
            {
                UserId = u.Id,
                Code = Rng.Next(100000, 999999).ToString(),
                IdentityToken = Guid.NewGuid().ToString(),
                ExpiresAt = u.CreatedAt.AddHours(24),
                IsUsed = u.EmailConfirmed,
                CreatedAt = u.CreatedAt
            });
            // Second code for some (resent)
            if (Rng.Next(3) == 0)
            {
                codes.Add(new EmailVerification
                {
                    UserId = u.Id,
                    Code = Rng.Next(100000, 999999).ToString(),
                    IdentityToken = Guid.NewGuid().ToString(),
                    ExpiresAt = u.CreatedAt.AddHours(48),
                    IsUsed = false,
                    CreatedAt = u.CreatedAt.AddHours(2)
                });
            }
        }

        await _context.EmailVerifications.AddRangeAsync(codes);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Email verifications seeded: {N}", codes.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  20. PHONE VERIFICATIONS (15+)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedPhoneVerificationsAsync()
    {
        var users = await _context.Users.IgnoreQueryFilters().Where(u => !u.IsDeleted).ToListAsync();
        var codes = new List<PhoneVerification>();

        foreach (var u in users.Take(15))
        {
            codes.Add(new PhoneVerification
            {
                UserId = u.Id,
                PhoneNumber = u.Phone ?? u.PhoneNumber ?? "01000000000",
                Code = Rng.Next(100000, 999999).ToString(),
                IdentityToken = Rng.Next(3) == 0 ? Guid.NewGuid().ToString() : null,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                IsUsed = Rng.Next(2) == 0,
                CreatedAt = DateTime.UtcNow.AddDays(-Rng.Next(1, 30))
            });
        }

        await _context.PhoneVerifications.AddRangeAsync(codes);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Phone verifications seeded: {N}", codes.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  21. RAG DOCUMENTS (25+ — from completed jobs)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedRAGDocumentsAsync()
    {
        var completedJobs = await _context.Jobs.IgnoreQueryFilters()
            .Where(j => j.Status == "مكتمل" && j.ProblemDescription != null)
            .OrderBy(j => j.Id).Take(25).ToListAsync();

        var docs = new List<RAGDocument>();
        foreach (var job in completedJobs)
        {
            docs.Add(new RAGDocument
            {
                JobId = job.Id,
                ChromaDocumentId = $"chroma_{Guid.NewGuid():N}",
                ChunkType = "problem",
                EmbeddingModel = "text-embedding-3-small",
                CreatedAt = job.CreatedAt
            });
            if (job.SolutionDescription != null)
            {
                docs.Add(new RAGDocument
                {
                    JobId = job.Id,
                    ChromaDocumentId = $"chroma_{Guid.NewGuid():N}",
                    ChunkType = "solution",
                    EmbeddingModel = "text-embedding-3-small",
                    CreatedAt = job.CompletedAt ?? job.CreatedAt
                });
            }
        }

        await _context.RAGDocuments.AddRangeAsync(docs);
        await _context.SaveChangesAsync();
        _logger.LogInformation("RAG documents seeded: {N}", docs.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  22. JOB FEEDBACKS (30+ — RAG feedback)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedJobFeedbacksAsync()
    {
        var users = await _context.Users.IgnoreQueryFilters().Where(u => u.IsActive).ToListAsync();
        var ragDocs = await _context.RAGDocuments.ToListAsync();
        var jobs = await _context.Jobs.IgnoreQueryFilters().Where(j => j.Status == "مكتمل").OrderBy(j => j.Id).Take(20).ToListAsync();
        if (users.Count == 0 || (ragDocs.Count == 0 && jobs.Count == 0)) return;

        var feedbacks = new List<JobFeedback>();
        foreach (var job in jobs.Take(15))
        {
            var user = users[Rng.Next(users.Count)];
            var ragDoc = ragDocs.Count > 0 ? ragDocs[Rng.Next(ragDocs.Count)] : null;

            var fb = new JobFeedback
            {
                UserId = user.Id,
                RAGDocumentId = ragDoc?.Id,
                FeedbackType = Rng.Next(3) > 0 ? "ساعدني" : "محتاج حرفي",
                CreatedAt = BaseDate.AddDays(Rng.Next(30, 170))
            };
            // Set shadow property JobId
            _context.Entry(fb).Property("JobId").CurrentValue = job.Id;
            feedbacks.Add(fb);
        }

        await _context.JobFeedbacks.AddRangeAsync(feedbacks);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Job feedbacks seeded: {N}", feedbacks.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  23. USER CONNECTIONS (20+ — SignalR connections)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedUserConnectionsAsync()
    {
        var users = await _context.Users.IgnoreQueryFilters().Where(u => u.IsActive).ToListAsync();
        var connections = new List<UserConnection>();

        foreach (var u in users.Take(20))
        {
            connections.Add(new UserConnection
            {
                UserId = u.Id,
                ConnectionId = Guid.NewGuid().ToString("N"),
                IsConnected = Rng.Next(3) > 0,
                ConnectedAt = DateTime.UtcNow.AddDays(-Rng.Next(1, 60)),
                DisconnectedAt = Rng.Next(3) > 0 ? DateTime.UtcNow.AddDays(-Rng.Next(1, 5)) : (DateTime?)null,
                CreatedAt = DateTime.UtcNow.AddDays(-Rng.Next(1, 60))
            });
        }

        await _context.UserConnections.AddRangeAsync(connections);
        await _context.SaveChangesAsync();
        _logger.LogInformation("User connections seeded: {N}", connections.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  24. IDENTITY CLAIMS (role claims for all users)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedIdentityClaimsAsync()
    {
        var users = await _context.Users.IgnoreQueryFilters().ToListAsync();
        var existingClaims = await _context.UserClaims.ToListAsync();
        var existingUserIds = existingClaims.Select(c => c.UserId).ToHashSet();

        var claims = new List<IdentityUserClaim<int>>();
        foreach (var user in users)
        {
            if (!existingUserIds.Contains(user.Id))
            {
                claims.Add(new IdentityUserClaim<int>
                {
                    UserId = user.Id,
                    ClaimType = System.Security.Claims.ClaimTypes.Role,
                    ClaimValue = user.Role
                });
            }
        }

        if (claims.Count > 0)
        {
            _context.UserClaims.AddRange(claims);
            await _context.SaveChangesAsync();
        }
        _logger.LogInformation("Identity claims seeded: {N}", claims.Count);
    }
}