using Harfi.Models.Constants;
using Harfi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Harfi.Repositories.Data;

/// <summary>
/// Seeds realistic Arabic production-ready test data covering ALL business states.
/// Runs once on startup when the Users table is empty.
/// Seeds in strict FK dependency order.
/// </summary>
public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DataSeeder> _logger;

    // ── Base timestamps spread over last 6 months ──────────────
    private static readonly DateTime BaseDate =
        DateTime.UtcNow.AddMonths(-6);

    public DataSeeder(
        AppDbContext context,
        UserManager<User> userManager,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    //  ENTRY POINT
    // ═══════════════════════════════════════════════════════════
    public async Task SeedAsync()
    {
        bool hasUsers = await _context.Users.IgnoreQueryFilters().AnyAsync();

        // Config tables — always seed independently, never skip
        if (!await _context.ServiceTypes.AnyAsync())
            await SeedServiceTypesAsync();

        if (!await _context.Cities.AnyAsync())
            await SeedCitiesAsync();

        if (!await _context.FeatureFlags.AnyAsync())
            await SeedFeatureFlagsAsync();

        // Main data — skip if users already exist
        if (hasUsers)
        {
            _logger.LogInformation("Seed skipped — data already exists.");
            return;
        }

        _logger.LogInformation("Starting full data seed...");

        await SeedRolesAsync();
        await SeedAdminAsync();
        await SeedCustomerUsersAsync();
        await SeedCraftsmanUsersAsync();
        await SeedCraftsmanProfilesAsync();
        await SeedJobsAsync();
        await SeedReviewsAsync();
        await RecalculateRatingsAsync();
        await SeedConversationsAsync();
        await SeedMessagesAsync();
        await SeedNotificationsAsync();
        await SeedAdminAuditLogsAsync();
        await SeedReportsAsync();

        _logger.LogInformation("✅ Full seed completed successfully.");
    }

    // ═══════════════════════════════════════════════════════════
    //  1. ROLES (ASP.NET Core Identity)
    // ═══════════════════════════════════════════════════════════
    private Task SeedRolesAsync()
    {
        // Harfi uses a custom Role string on User, not IdentityRole.
        // Nothing to seed here — kept for future if AddRoles<> is enabled.
        return Task.CompletedTask;
    }

    // ═══════════════════════════════════════════════════════════
    //  2. SERVICE TYPES (6+)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedServiceTypesAsync()
    {
        var serviceTypes = new[]
        {
            new ServiceType { NameAr = "سباكة",       NameEn = "Plumbing",       Icon = "🔧", IsActive = true },
            new ServiceType { NameAr = "كهرباء",      NameEn = "Electrical",     Icon = "⚡", IsActive = true },
            new ServiceType { NameAr = "نجارة",       NameEn = "Carpentry",      Icon = "🪚", IsActive = true },
            new ServiceType { NameAr = "دهانات",      NameEn = "Painting",       Icon = "🎨", IsActive = true },
            new ServiceType { NameAr = "تكييف",       NameEn = "AC & Cooling",   Icon = "❄️", IsActive = true },
            new ServiceType { NameAr = "تبليط",       NameEn = "Tiling",         Icon = "🪟", IsActive = true },
            new ServiceType { NameAr = "حدادة",       NameEn = "Ironwork",       Icon = "⚒️", IsActive = true },
            new ServiceType { NameAr = "صيانة عامة",  NameEn = "General Maintenance", Icon = "🛠️", IsActive = false }
        };

        await _context.ServiceTypes.AddRangeAsync(serviceTypes);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Service types seeded: {N}", serviceTypes.Length);
    }

    // ═══════════════════════════════════════════════════════════
    //  3. CITIES (8 Egyptian cities)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedCitiesAsync()
    {
        var cities = new[]
        {
            new City { NameAr = "القاهرة",      NameEn = "Cairo",        Governorate = "القاهرة",     IsActive = true },
            new City { NameAr = "الإسكندرية",   NameEn = "Alexandria",   Governorate = "الإسكندرية",  IsActive = true },
            new City { NameAr = "الجيزة",       NameEn = "Giza",         Governorate = "الجيزة",      IsActive = true },
            new City { NameAr = "المنصورة",     NameEn = "Mansoura",     Governorate = "الدقهلية",    IsActive = true },
            new City { NameAr = "طنطا",         NameEn = "Tanta",        Governorate = "الغربية",     IsActive = true },
            new City { NameAr = "أسيوط",        NameEn = "Assiut",       Governorate = "أسيوط",       IsActive = true },
            new City { NameAr = "الإسماعيلية",  NameEn = "Ismailia",     Governorate = "الإسماعيلية", IsActive = true },
            new City { NameAr = "الأقصر",       NameEn = "Luxor",        Governorate = "الأقصر",      IsActive = true }
        };

        await _context.Cities.AddRangeAsync(cities);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Cities seeded: {N}", cities.Length);
    }

    // ═══════════════════════════════════════════════════════════
    //  4. FEATURE FLAGS
    // ═══════════════════════════════════════════════════════════
    private async Task SeedFeatureFlagsAsync()
    {
        var flags = new[]
        {
            new FeatureFlag { Key = "SelfFixGuideEnabled",  IsEnabled = true,  UpdatedAt = BaseDate.AddMonths(3) },
            new FeatureFlag { Key = "VoiceSearchEnabled",   IsEnabled = false, UpdatedAt = BaseDate.AddMonths(2) },
            new FeatureFlag { Key = "AIMatchingEnabled",    IsEnabled = true,  UpdatedAt = BaseDate.AddMonths(1) }
        };

        await _context.FeatureFlags.AddRangeAsync(flags);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Feature flags seeded.");
    }

    // ═══════════════════════════════════════════════════════════
    //  5. ADMIN USER
    // ═══════════════════════════════════════════════════════════
    private async Task SeedAdminAsync()
    {
        var admin = new User
        {
            UserName = "admin@harfi.com",
            Email = "admin@harfi.com",
            Name = "مدير النظام",
            Role = "admin",
            Phone = "01000000000",
            IsActive = true,
            IsVerified = true,
            EmailConfirmed = true,
            CreatedAt = BaseDate
        };

        var result = await _userManager.CreateAsync(admin, "Admin@Harfi2024!");
        if (!result.Succeeded)
            _logger.LogWarning("Admin seed failed: {E}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
        else
            _logger.LogInformation("Admin seeded: admin@harfi.com");
    }

    // ═══════════════════════════════════════════════════════════
    //  6. CUSTOMER USERS (5 covering all states)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedCustomerUsersAsync()
    {
        // Business states:
        // C1 — active, verified
        // C2 — active, verified
        // C3 — inactive / deactivated by admin
        // C4 — soft-deleted by admin
        // C5 — new, unverified (just registered, no email confirmation yet)
        var customers = new[]
        {
            new {
                Email="sara.ahmed@gmail.com",     Name="سارة أحمد",
                Phone="01245678901", IsActive=true,  IsVerified=true,
                IsDeleted=false, EmailConfirmed=true,
                CreatedAt=BaseDate.AddDays(5)
            },
            new {
                Email="nourhan.mohamed@gmail.com", Name="نورهان محمد",
                Phone="01567890124", IsActive=true,  IsVerified=true,
                IsDeleted=false, EmailConfirmed=true,
                CreatedAt=BaseDate.AddDays(15)
            },
            new {
                Email="maryam.ali@gmail.com",     Name="مريم علي",
                Phone="01067890123", IsActive=false, IsVerified=true,
                IsDeleted=false, EmailConfirmed=true,
                CreatedAt=BaseDate.AddDays(20)
            },
            new {
                Email="fatma.hassan@gmail.com",   Name="فاطمة حسن",
                Phone="01178901234", IsActive=false, IsVerified=true,
                IsDeleted=true,  EmailConfirmed=true,
                CreatedAt=BaseDate.AddDays(25)
            },
            new {
                Email="mennatallah.khaled@gmail.com", Name="منة الله خالد",
                Phone="01289012345", IsActive=true,  IsVerified=false,
                IsDeleted=false, EmailConfirmed=false,
                CreatedAt=BaseDate.AddMonths(5).AddDays(10)
            }
        };

        foreach (var d in customers)
        {
            var user = new User
            {
                UserName = d.Email,
                Email = d.Email,
                Name = d.Name,
                Role = "customer",
                Phone = d.Phone,
                IsActive = d.IsActive,
                IsVerified = d.IsVerified,
                IsDeleted = d.IsDeleted,
                EmailConfirmed = d.EmailConfirmed,
                CreatedAt = d.CreatedAt
            };

            if (d.IsDeleted)
            {
                user.DeletedAt = d.CreatedAt.AddMonths(2);
                user.DeletionReason = "انتهاك شروط الاستخدام - تقارير متعددة من حرفيين";
                user.DeletedByAdminId = 1;
            }

            var result = await _userManager.CreateAsync(user, "Customer@2024");
            if (!result.Succeeded)
                _logger.LogWarning("Customer seed failed {E}: {Err}",
                    d.Email, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("5 customer users seeded.");
    }

    // ═══════════════════════════════════════════════════════════
    //  7. CRAFTSMAN USERS (8 covering all states)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedCraftsmanUsersAsync()
    {
        var craftsmen = new[]
        {
            // CM1 — pending approval
            new { Email="ahmed.ali@gmail.com",        Name="أحمد علي",       Phone="01012345678", CreatedAt=BaseDate.AddDays(10) },
            // CM2 — approved + available (high rating)
            new { Email="mohamed.hassan@gmail.com",   Name="محمد حسن",       Phone="01123456789", CreatedAt=BaseDate.AddDays(8)  },
            // CM3 — approved + available (low rating)
            new { Email="abdallah.khaled@gmail.com",  Name="عبدالله خالد",   Phone="01234567890", CreatedAt=BaseDate.AddDays(30) },
            // CM4 — approved + suspended (IsAvailable=false)
            new { Email="mostafa.mahmoud@gmail.com",  Name="مصطفى محمود",    Phone="01512345678", CreatedAt=BaseDate.AddDays(7)  },
            // CM5 — rejected (IsApproved=false, IsDeleted=true)
            new { Email="hussien.reda@gmail.com",     Name="حسين رضا",       Phone="01098765432", CreatedAt=BaseDate.AddDays(40) },
            // CM6 — soft-deleted after being active
            new { Email="kareem.samy@gmail.com",      Name="كريم سامي",      Phone="01156789012", CreatedAt=BaseDate.AddDays(6)  },
            // CM7 — approved + available (no reviews yet)
            new { Email="youssef.adel@gmail.com",     Name="يوسف عادل",      Phone="01234561234", CreatedAt=BaseDate.AddMonths(5).AddDays(1)  },
            // CM8 — approved + available (standard)
            new { Email="ibrahim.nasr@gmail.com",     Name="إبراهيم نصر",    Phone="01567890123", CreatedAt=BaseDate.AddDays(12) },
        };

        foreach (var d in craftsmen)
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
                CreatedAt = d.CreatedAt
            };

            var result = await _userManager.CreateAsync(user, "Craftsman@2024");
            if (!result.Succeeded)
                _logger.LogWarning("Craftsman user seed failed {E}: {Err}",
                    d.Email, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        _logger.LogInformation("8 craftsman users seeded.");
    }

    // ═══════════════════════════════════════════════════════════
    //  8. CRAFTSMAN PROFILES (8 — one per business state)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedCraftsmanProfilesAsync()
    {
        var users = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.Role == "craftsman")
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        if (users.Count < 8)
        {
            _logger.LogWarning("Expected 8 craftsman users, found {N}", users.Count);
            return;
        }

        // Helper: look up by email
        User U(string email) => users.First(u => u.Email == email);

        var profiles = new Craftsman[]
        {
            // CM1 — Pending approval (submitted, not yet reviewed)
            new()
            {
                UserId         = U("ahmed.ali@gmail.com").Id,
                ServiceType    = "سباكة",
                City           = "القاهرة",
                Neighborhood   = "مدينة نصر",
                PriceRangeMin  = 150m,
                PriceRangeMax  = 400m,
                Experience     = 3,
                IsApproved     = false,
                IsAvailable    = true,
                IsDeleted      = false,
                Rating         = 0m,
                Bio            = "سباك متخصص في تركيب وصيانة شبكات المياه والصرف الصحي",
                NationalIdUrl  = "/uploads/ids/id_1.jpg",
                CreatedAt      = U("ahmed.ali@gmail.com").CreatedAt
            },

            // CM2 — Approved + Available (high rating 4.8)
            new()
            {
                UserId         = U("mohamed.hassan@gmail.com").Id,
                ServiceType    = "كهرباء",
                City           = "الإسكندرية",
                Neighborhood   = "سيدي بشر",
                PriceRangeMin  = 200m,
                PriceRangeMax  = 600m,
                Experience     = 12,
                IsApproved     = true,
                IsAvailable    = true,
                IsDeleted      = false,
                Rating         = 4.8m,
                Bio            = "مهندس كهربائي خبرة 12 سنة في تمديد الكهرباء والصيانة الشاملة للمنازل والمصانع",
                NationalIdUrl  = "/uploads/ids/id_2.jpg",
                CreatedAt      = U("mohamed.hassan@gmail.com").CreatedAt
            },

            // CM3 — Approved + Available (low rating 2.1)
            new()
            {
                UserId         = U("abdallah.khaled@gmail.com").Id,
                ServiceType    = "دهانات",
                City           = "الجيزة",
                Neighborhood   = "الهرم",
                PriceRangeMin  = 100m,
                PriceRangeMax  = 300m,
                Experience     = 2,
                IsApproved     = true,
                IsAvailable    = true,
                IsDeleted      = false,
                Rating         = 2.1m,
                Bio            = "نقاش دهانات بسعر مناسب",
                NationalIdUrl  = "/uploads/ids/id_3.jpg",
                CreatedAt      = U("abdallah.khaled@gmail.com").CreatedAt
            },

            // CM4 — Approved + Suspended (IsAvailable=false, hidden from search)
            new()
            {
                UserId         = U("mostafa.mahmoud@gmail.com").Id,
                ServiceType    = "نجارة",
                City           = "القاهرة",
                Neighborhood   = "العباسية",
                PriceRangeMin  = 300m,
                PriceRangeMax  = 800m,
                Experience     = 8,
                IsApproved     = true,
                IsAvailable    = false,    // ← suspended
                IsDeleted      = false,
                Rating         = 4.2m,
                Bio            = "نجار موبيليا وباركيه خبرة 8 سنوات",
                NationalIdUrl  = "/uploads/ids/id_4.jpg",
                CreatedAt      = U("mostafa.mahmoud@gmail.com").CreatedAt
            },

            // CM5 — Rejected (IsApproved=false, IsDeleted=true, Arabic rejection reason)
            new()
            {
                UserId         = U("hussien.reda@gmail.com").Id,
                ServiceType    = "تكييف",
                City           = "طنطا",
                Neighborhood   = null,
                PriceRangeMin  = 250m,
                PriceRangeMax  = 700m,
                Experience     = 1,
                IsApproved     = false,
                IsAvailable    = false,
                IsDeleted      = true,     // ← rejected = soft-deleted
                DeletedAt      = U("hussien.reda@gmail.com").CreatedAt.AddDays(3),
                DeletedByAdminId = 1,
                RejectionReason  = "بيانات الهوية الوطنية غير واضحة وغير مطابقة للاسم المسجل",
                DeletionReason   = "رفض طلب التسجيل: بيانات غير صحيحة",
                Rating         = 0m,
                Bio            = "فني تكييف",
                NationalIdUrl  = "/uploads/ids/id_5.jpg",
                CreatedAt      = U("hussien.reda@gmail.com").CreatedAt
            },

            // CM6 — Approved then soft-deleted by admin (misconduct)
            new()
            {
                UserId         = U("kareem.samy@gmail.com").Id,
                ServiceType    = "سباكة",
                City           = "المنصورة",
                Neighborhood   = "المنصورة",
                PriceRangeMin  = 180m,
                PriceRangeMax  = 500m,
                Experience     = 6,
                IsApproved     = true,
                IsAvailable    = false,
                IsDeleted      = true,     // ← deleted AFTER approval
                DeletedAt      = U("kareem.samy@gmail.com").CreatedAt.AddMonths(2),
                DeletedByAdminId = 1,
                DeletionReason   = "تلقي شكاوى متعددة من العملاء ورفض الرد على طلبات التواصل",
                RejectionReason  = null,
                Rating         = 3.5m,
                Bio            = "سباك عام",
                NationalIdUrl  = "/uploads/ids/id_6.jpg",
                CreatedAt      = U("kareem.samy@gmail.com").CreatedAt
            },

            // CM7 — Approved + Available (no reviews yet, new craftsman)
            new()
            {
                UserId         = U("youssef.adel@gmail.com").Id,
                ServiceType    = "تبليط",
                City           = "الإسماعيلية",
                Neighborhood   = "الإسماعيلية",
                PriceRangeMin  = 200m,
                PriceRangeMax  = 600m,
                Experience     = 5,
                IsApproved     = true,
                IsAvailable    = true,
                IsDeleted      = false,
                Rating         = 0m,       // ← no reviews yet
                Bio            = "فني تبليط متخصص في السيراميك والرخام والبورسلين",
                NationalIdUrl  = "/uploads/ids/id_7.jpg",
                CreatedAt      = U("youssef.adel@gmail.com").CreatedAt
            },

            // CM8 — Approved + Available (standard, many reviews)
            new()
            {
                UserId         = U("ibrahim.nasr@gmail.com").Id,
                ServiceType    = "كهرباء",
                City           = "الأقصر",
                Neighborhood   = "الأقصر",
                PriceRangeMin  = 150m,
                PriceRangeMax  = 450m,
                Experience     = 9,
                IsApproved     = true,
                IsAvailable    = true,
                IsDeleted      = false,
                Rating         = 4.5m,
                Bio            = "كهربائي معتمد خبرة 9 سنوات في المنازل والمحلات التجارية",
                NationalIdUrl  = "/uploads/ids/id_8.jpg",
                CreatedAt      = U("ibrahim.nasr@gmail.com").CreatedAt
            }
        };

        await _context.Craftsmen.AddRangeAsync(profiles);
        await _context.SaveChangesAsync();
        _logger.LogInformation("8 craftsman profiles seeded.");
    }

    // ═══════════════════════════════════════════════════════════
    //  9. JOBS (one per status + dispute states)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedJobsAsync()
    {
        // Load active (non-deleted) craftsmen and customers
        var craftsmen = await _context.Craftsmen
            .IgnoreQueryFilters()
            .Include(c => c.User)
            .ToListAsync();

        var customers = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.Role == "customer")
            .ToListAsync();

        Craftsman CM(string email) => craftsmen.First(c => c.User.Email == email);
        User CU(string email) => customers.First(u => u.Email == email);

        var approvedCM2 = CM("mohamed.hassan@gmail.com");  // كهربائي — high rating
        var approvedCM3 = CM("abdallah.khaled@gmail.com");  // دهانات — low rating
        var approvedCM8 = CM("ibrahim.nasr@gmail.com");      // كهربائي — standard
        var suspendedCM4 = CM("mostafa.mahmoud@gmail.com");  // نجار — suspended

        var C1 = CU("sara.ahmed@gmail.com");
        var C2 = CU("nourhan.mohamed@gmail.com");
        var C3 = CU("maryam.ali@gmail.com");         // inactive
        var C4 = CU("fatma.hassan@gmail.com");        // deleted
        var C5 = CU("mennatallah.khaled@gmail.com"); // unverified

        var jobs = new List<Job>
        {
            // J1 — Open (created, waiting for craftsman acceptance)
            new()
            {
                CustomerId       = C5.Id,
                CraftsmanId      = approvedCM2.Id,
                Status           = JobStatusConstants.Open,
                ServiceType      = "كهرباء",
                Description      = "المفاتيح في الصالة بتشرر وفيه رائحة احتراق",
                Address          = "15 شارع التحرير، الإسكندرية",
                PreferredDate    = DateTime.UtcNow.AddDays(2),
                ProblemDescription = "شرار من المفاتيح الكهربائية مع رائحة بلاستيك محترق",
                CreatedAt        = BaseDate.AddMonths(5).AddDays(15),
                UpdatedAt        = BaseDate.AddMonths(5).AddDays(15)
            },

            // J2 — InProgress (accepted by craftsman)
            new()
            {
                CustomerId       = C1.Id,
                CraftsmanId      = approvedCM2.Id,
                Status           = JobStatusConstants.InProgress,
                ServiceType      = "كهرباء",
                Description      = "لوحة الكهرباء الرئيسية عاطلة والكهربا مقطوعة على الشقة كلها",
                Address          = "8 شارع الجيش، مدينة نصر، القاهرة",
                ProblemDescription = "انقطاع تام في التيار الكهربائي بعد شرارة من اللوحة الرئيسية",
                CreatedAt        = BaseDate.AddMonths(5).AddDays(20),
                UpdatedAt        = BaseDate.AddMonths(5).AddDays(21)
            },

            // J3 — Completed (finished, review submitted)
            new()
            {
                CustomerId       = C2.Id,
                CraftsmanId      = approvedCM2.Id,
                Status           = JobStatusConstants.Done,
                ServiceType      = "كهرباء",
                Description      = "تركيب نقاط إضاءة جديدة في ثلاث غرف",
                Address          = "22 شارع أبو قير، الإسكندرية",
                ProblemDescription = "الغرف بدون إضاءة مناسبة - محتاج تركيب سبوت لايت",
                SolutionDescription = "تم تركيب 12 سبوت لايت LED موفر للطاقة مع توصيلات كاملة",
                CompletedAt      = BaseDate.AddMonths(3).AddDays(5),
                CreatedAt        = BaseDate.AddMonths(3),
                UpdatedAt        = BaseDate.AddMonths(3).AddDays(5)
            },

            // J4 — Completed (finished, NO review yet)
            new()
            {
                CustomerId       = C1.Id,
                CraftsmanId      = approvedCM8.Id,
                Status           = JobStatusConstants.Done,
                ServiceType      = "كهرباء",
                Description      = "صيانة دورية شاملة للتوصيلات الكهربائية في الشقة",
                Address          = "3 شارع النصر، الأقصر",
                ProblemDescription = "مطلوب فحص وصيانة وقائية لجميع التوصيلات",
                SolutionDescription = "تم فحص وتجديد التوصيلات وتغيير القواطع القديمة",
                CompletedAt      = BaseDate.AddMonths(4).AddDays(2),
                CreatedAt        = BaseDate.AddMonths(4),
                UpdatedAt        = BaseDate.AddMonths(4).AddDays(2)
            },

            // J5 — Rejected by craftsman
            new()
            {
                CustomerId       = C3.Id,
                CraftsmanId      = approvedCM2.Id,
                Status           = JobStatusConstants.Rejected,
                ServiceType      = "كهرباء",
                Description      = "محتاج تمديدات كهربائية خارجية على واجهة المبنى",
                Address          = "55 شارع السوق، الجيزة",
                ProblemDescription = "تمديدات خارجية خطرة تحتاج ترخيص",
                CreatedAt        = BaseDate.AddMonths(4).AddDays(10),
                UpdatedAt        = BaseDate.AddMonths(4).AddDays(10)
            },

            // J6 — Disputed (IsDisputed=true, active dispute)
            new()
            {
                CustomerId       = C2.Id,
                CraftsmanId      = approvedCM8.Id,
                Status           = JobStatusConstants.Done,
                ServiceType      = "كهرباء",
                Description      = "إصلاح عطل في دائرة الطاقة للمكيف",
                Address          = "18 شارع الجمهورية، الأقصر",
                ProblemDescription = "المكيف لا يعمل بسبب مشكلة في الكهرباء",
                SolutionDescription = "تم استبدال القاطع المخصص للمكيف",
                IsDisputed       = true,
                DisputeRaisedAt  = BaseDate.AddMonths(5),
                CompletedAt      = BaseDate.AddMonths(4).AddDays(25),
                CreatedAt        = BaseDate.AddMonths(4).AddDays(20),
                UpdatedAt        = BaseDate.AddMonths(5)
            },

            // J7 — Dispute resolved, favored party = Customer
            new()
            {
                CustomerId       = C1.Id,
                CraftsmanId      = approvedCM3.Id,
                Status           = JobStatusConstants.Done,
                ServiceType      = "دهانات",
                Description      = "دهان كامل لشقة 3 غرف وصالة",
                Address          = "12 شارع الهرم، الجيزة",
                ProblemDescription = "دهانات قديمة متشققة ومتقشرة",
                SolutionDescription = "تم دهان الشقة بالكامل لكن بجودة أقل من المتفق عليه",
                IsDisputed       = false,
                DisputeRaisedAt  = BaseDate.AddMonths(3).AddDays(20),
                DisputeResolvedAt = BaseDate.AddMonths(4),
                DisputeResolution = "Resolution: إعادة دهان الأجزاء المخالفة أو استرداد 30% من المبلغ. Favored: Customer",
                CompletedAt      = BaseDate.AddMonths(3).AddDays(15),
                CreatedAt        = BaseDate.AddMonths(3).AddDays(10),
                UpdatedAt        = BaseDate.AddMonths(4)
            },

            // J8 — Dispute resolved, favored party = Craftsman
            new()
            {
                CustomerId       = C2.Id,
                CraftsmanId      = approvedCM3.Id,
                Status           = JobStatusConstants.Done,
                ServiceType      = "دهانات",
                Description      = "دهان حوائط الصالة بالكامل",
                Address          = "7 شارع الطيران، الجيزة",
                ProblemDescription = "العميل يدعي أن جودة الدهان سيئة رغم استخدام مواد جيدة",
                SolutionDescription = "تم تنفيذ الدهان بمواد عالية الجودة حسب الاتفاق",
                IsDisputed       = false,
                DisputeRaisedAt  = BaseDate.AddMonths(2).AddDays(5),
                DisputeResolvedAt = BaseDate.AddMonths(2).AddDays(20),
                DisputeResolution = "Resolution: الشغل مطابق للاتفاق والمواد المستخدمة معتمدة. Favored: Craftsman",
                CompletedAt      = BaseDate.AddMonths(2),
                CreatedAt        = BaseDate.AddMonths(2).AddDays(-5),
                UpdatedAt        = BaseDate.AddMonths(2).AddDays(20)
            },

            // J9 — Completed, used for 5-star review
            new()
            {
                CustomerId       = C2.Id,
                CraftsmanId      = approvedCM8.Id,
                Status           = JobStatusConstants.Done,
                ServiceType      = "كهرباء",
                Description      = "تركيب لوحة توزيع كهرباء جديدة",
                Address          = "33 شارع النيل، الأقصر",
                ProblemDescription = "اللوحة الأصلية قديمة وخطيرة",
                SolutionDescription = "تم تركيب لوحة توزيع حديثة مع قواطع ذات حساسية عالية",
                CompletedAt      = BaseDate.AddMonths(1).AddDays(5),
                CreatedAt        = BaseDate.AddMonths(1),
                UpdatedAt        = BaseDate.AddMonths(1).AddDays(5)
            },

            // J10 — Completed, used for 1-star review (complaint)
            new()
            {
                CustomerId       = C1.Id,
                CraftsmanId      = approvedCM3.Id,
                Status           = JobStatusConstants.Done,
                ServiceType      = "دهانات",
                Description      = "دهان حجرة النوم",
                Address          = "10 شارع فيصل، الجيزة",
                ProblemDescription = "دهان قديم يحتاج تجديد",
                SolutionDescription = "تم الدهان لكن مع اختلاف اللون عن المتفق عليه",
                CompletedAt      = BaseDate.AddDays(45),
                CreatedAt        = BaseDate.AddDays(40),
                UpdatedAt        = BaseDate.AddDays(45)
            },

            // J11 — Job on suspended craftsman (historical)
            new()
            {
                CustomerId       = C1.Id,
                CraftsmanId      = suspendedCM4.Id,
                Status           = JobStatusConstants.Done,
                ServiceType      = "نجارة",
                Description      = "تركيب دواليب مطبخ",
                Address          = "6 شارع رمسيس، القاهرة",
                SolutionDescription = "تم تركيب دواليب المطبخ بالكامل",
                CompletedAt      = BaseDate.AddMonths(2).AddDays(10),
                CreatedAt        = BaseDate.AddMonths(2).AddDays(5),
                UpdatedAt        = BaseDate.AddMonths(2).AddDays(10)
            },

            // J12 — Cancelled by customer
            new()
            {
                CustomerId       = C3.Id,
                CraftsmanId      = approvedCM2.Id,
                Status           = JobStatusConstants.Cancelled,
                ServiceType      = "كهرباء",
                Description      = "تركيب نجفة كبيرة في الصالة",
                Address          = "44 شارع المحطة، الجيزة",
                CreatedAt        = BaseDate.AddMonths(4).AddDays(5),
                UpdatedAt        = BaseDate.AddMonths(4).AddDays(6)
            }
        };

        await _context.Jobs.AddRangeAsync(jobs);
        await _context.SaveChangesAsync();
        _logger.LogInformation("{N} jobs seeded.", jobs.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  10. REVIEWS
    // ═══════════════════════════════════════════════════════════
    private async Task SeedReviewsAsync()
    {
        var jobs = await _context.Jobs.ToListAsync();

        // Helper: find job by description fragment
        Job J(string fragment) => jobs.First(j => j.Description.Contains(fragment));

        var reviews = new List<Review>
        {
            // 5-star review with detailed Arabic comment
            new()
            {
                JobId       = J("نقاط إضاءة جديدة").Id,
                CustomerId  = J("نقاط إضاءة جديدة").CustomerId,
                CraftsmanId = J("نقاط إضاءة جديدة").CraftsmanId!.Value,
                Stars       = 5,
                Comment     = "الأستاذ محمد إنسان محترم جداً وشغله نظيف وفي الموعد بالضبط. " +
                              "ركّب الـ سبوت لايت بأحسن شكل ونضف ورائه الأوضة كاملة. " +
                              "بوصي بيه بثقة تامة لأي حد محتاج كهربائي.",
                CreatedAt   = J("نقاط إضاءة جديدة").CompletedAt!.Value.AddDays(1),
                IsDeleted   = false
            },

            // 1-star review with complaint
            new()
            {
                JobId       = J("حجرة النوم").Id,
                CustomerId  = J("حجرة النوم").CustomerId,
                CraftsmanId = J("حجرة النوم").CraftsmanId!.Value,
                Stars       = 1,
                Comment     = "الشغل بيفضح. اللون اللي جابه مختلف تماماً عن اللي اتفقنا عليه " +
                              "ورفض يصلح. ضيّعت وقتي وفلوسي. مش هينصحه لحد أبداً.",
                CreatedAt   = J("حجرة النوم").CompletedAt!.Value.AddDays(2),
                IsDeleted   = false
            },

            // Review on a completed job (craftsman now suspended — historical)
            new()
            {
                JobId       = J("دواليب مطبخ").Id,
                CustomerId  = J("دواليب مطبخ").CustomerId,
                CraftsmanId = J("دواليب مطبخ").CraftsmanId!.Value,
                Stars       = 4,
                Comment     = "شغل كويس وبالموعد بس كان في بعض التفاصيل الصغيرة محتاج ينتبهلها.",
                CreatedAt   = J("دواليب مطبخ").CompletedAt!.Value.AddDays(1),
                IsDeleted   = false
            },

            // Review that has been soft-deleted by admin (with deletion reason)
            new()
            {
                JobId       = J("صيانة دورية شاملة").Id,
                CustomerId  = J("صيانة دورية شاملة").CustomerId,
                CraftsmanId = J("صيانة دورية شاملة").CraftsmanId!.Value,
                Stars       = 2,
                Comment     = "محتوى مخالف للشروط تم حذفه من قبل الإدارة",
                CreatedAt   = J("صيانة دورية شاملة").CompletedAt!.Value.AddDays(1),
                IsDeleted   = true,
                DeletedAt   = J("صيانة دورية شاملة").CompletedAt!.Value.AddDays(3),
                DeletedByAdminId = 1,
                DeletionReason   = "التقييم يحتوي على ألفاظ مسيئة وتهديدات شخصية تخالف شروط الاستخدام"
            },

            // 5-star review on CM8 (لوحة توزيع)
            new()
            {
                JobId       = J("لوحة توزيع كهرباء جديدة").Id,
                CustomerId  = J("لوحة توزيع كهرباء جديدة").CustomerId,
                CraftsmanId = J("لوحة توزيع كهرباء جديدة").CraftsmanId!.Value,
                Stars       = 5,
                Comment     = "فنان في شغله! ركّب اللوحة وشرحلي إيه اللي اتغير وليه. محترف جداً.",
                CreatedAt   = J("لوحة توزيع كهرباء جديدة").CompletedAt!.Value.AddDays(1),
                IsDeleted   = false
            },

            // Review on J7 (dispute resolved — customer favored)
            new()
            {
                JobId       = J("دهان كامل لشقة 3 غرف").Id,
                CustomerId  = J("دهان كامل لشقة 3 غرف").CustomerId,
                CraftsmanId = J("دهان كامل لشقة 3 غرف").CraftsmanId!.Value,
                Stars       = 2,
                Comment     = "اضطررت أفتح نزاع مع الإدارة لأن الشغل كان بجودة أقل من المتفق عليه. " +
                              "الإدارة حلّت الموضوع وأسترجعت جزء من فلوسي.",
                CreatedAt   = J("دهان كامل لشقة 3 غرف").CompletedAt!.Value.AddDays(10),
                IsDeleted   = false
            }
        };

        await _context.Reviews.AddRangeAsync(reviews);
        await _context.SaveChangesAsync();
        _logger.LogInformation("{N} reviews seeded.", reviews.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  11. RECALCULATE RATINGS (from actual reviews)
    // ═══════════════════════════════════════════════════════════
    private async Task RecalculateRatingsAsync()
    {
        var craftsmen = await _context.Craftsmen
            .IgnoreQueryFilters()
            .Include(c => c.Reviews)
            .ToListAsync();

        foreach (var c in craftsmen)
        {
            var activeReviews = c.Reviews.Where(r => !r.IsDeleted).ToList();
            c.Rating = activeReviews.Count > 0
                ? Math.Round((decimal)activeReviews.Average(r => r.Stars), 2)
                : c.Rating; // preserve seeded rating if no reviews
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Ratings recalculated.");
    }

    // ═══════════════════════════════════════════════════════════
    //  12. CONVERSATIONS
    // ═══════════════════════════════════════════════════════════
    private async Task SeedConversationsAsync()
    {
        // Only create conversations for jobs that have a CraftsmanId
        var eligibleJobs = await _context.Jobs
            .IgnoreQueryFilters()
            .Where(j => j.CraftsmanId != null
                     && j.Status != JobStatusConstants.Cancelled)
            .OrderBy(j => j.Id)
            .Take(8)
            .ToListAsync();

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
        _logger.LogInformation("Conversations seeded.");
    }

    // ═══════════════════════════════════════════════════════════
    //  13. MESSAGES (realistic Arabic chat per conversation)
    // ═══════════════════════════════════════════════════════════
    private async Task SeedMessagesAsync()
    {
        var conversations = await _context.Conversations
            .IgnoreQueryFilters()
            .Include(c => c.Job)
            .Include(c => c.Craftsman)
            .OrderBy(c => c.Id)
            .ToListAsync();

        var craftsmanUserIds = await _context.Craftsmen
            .IgnoreQueryFilters()
            .ToDictionaryAsync(c => c.Id, c => c.UserId);

        // Conversation message scripts — index matches conversation order
        var scripts = new (string content, bool fromCustomer, int minutesOffset)[][]
        {
            // Conversation 0 — open job, initial contact
            [
                ("أهلاً، أنا بحتاج حد يصلح التسريب في الحمام بسرعة", true, 0),
                ("أهلاً بيك، أنا شفت الطلب. امتى تقدر تستقبلني؟", false, 8),
                ("النهارده بعد الضهر مناسب؟", true, 20),
                ("تمام خد عندك. هكون عندك الساعة 4 العصر.", false, 35),
                ("أوكي، شكراً جزيلاً", true, 40)
            ],
            // Conversation 1 — inProgress
            [
                ("أحتاج إصلاح لوحة الكهرباء الرئيسية، الكهرباء مقطوعة خالص", true, 0),
                ("فهمت المشكلة، لازم أشوف اللوحة قبل ما أعطيك سعر", false, 15),
                ("طيب امتى تقدر تجي؟", true, 25),
                ("بكره الصبح الساعة 9، ينفع؟", false, 30),
                ("ينفع تمام، شكراً", true, 45),
                ("أنا هنا دلوقتي، نزل افتح", false, 840)
            ],
            // Conversation 2 — completed job, active chat
            [
                ("محتاج تركيب سبوت لايت في 3 غرف", true, 0),
                ("كام غرفة وكام نقطة تقريباً؟", false, 10),
                ("3 غرف، تقريباً 12 نقطة", true, 18),
                ("تمام السعر هيكون 600 جنيه شامل المواد", false, 25),
                ("مقبول. يوم الخميس ينفع؟", true, 35),
                ("ينفع تمام", false, 40),
                ("الشغل اتعمل زي الفل، شكراً جداً", true, 2880)
            ],
            // Conversation 3 — simple exchange
            [
                ("مرحبا، مطلوب صيانة دورية", true, 0),
                ("هل عندك وقت الأسبوع الجاي؟", false, 15),
                ("أيوه أي يوم من الأحد للأربع", true, 30),
                ("هيجي فني يوم الأثنين الساعة 11", false, 45)
            ],
            // Conversation 4 — disputed job, locked conversation
            [
                ("الشغل خلص لكن مش تمام", true, 0),
                ("إيه المشكلة بالظبط؟", false, 20),
                ("المكيف لسه مش بيبرد زي ما قبل", true, 35),
                ("أنا كنت عندك واشتغلت، محتاج تاني كشف", false, 50),
                ("هفتح نزاع لو مش اتحل", true, 120),
                ("خلي إدارة حرفي تشوف المشكلة", false, 135)
            ],
            // Conversation 5 — resolved dispute
            [
                ("الدهان اللي عملته مش بالمواصفات المتفق عليها", true, 0),
                ("الشغل اتعمل صح والمواد زي المتفق", false, 30),
                ("اللون مختلف تماماً", true, 45),
                ("هتكلم الإدارة يشوفوا", false, 60)
            ],
            // Conversation 6 — unread messages (customer hasn't read yet)
            [
                ("محتاج أعمل معك لوحة توزيع جديدة", true, 0),
                ("أنا متاح، كام كيلو واط محتاج؟", false, 12),
                ("مش عارف، ممكن تيجي تكشف؟", true, 25),
                ("أيوه تمام، بكره الصبح", false, 40),
                ("ممتاز! بنتظرك", true, 50)
            ],
            // Conversation 7 — another completed job
            [
                ("نجار محتاج تركيب دواليب مطبخ", true, 0),
                ("أنا متخصص في ده، كام متر المطبخ؟", false, 10),
                ("4 متر تقريباً", true, 20),
                ("السعر هيكون 1500 جنيه كل متر", false, 30),
                ("تمام متفقين", true, 45)
            ]
        };

        var allMessages = new List<Message>();

        for (int i = 0; i < Math.Min(conversations.Count, scripts.Length); i++)
        {
            var conv = conversations[i];
            var craftUserId = craftsmanUserIds.GetValueOrDefault(conv.CraftsmanId, 0);
            var script = scripts[i];
            DateTime? lastMsgTime = null;

            foreach (var (content, fromCustomer, minutesOffset) in script)
            {
                var senderId = fromCustomer ? conv.CustomerId : craftUserId;
                var sentAt = conv.CreatedAt.AddMinutes(minutesOffset);
                lastMsgTime = sentAt;

                // For conversation 6 (unread messages): craftsman messages are unread
                bool isRead = (i == 6 && !fromCustomer) ? false : true;

                allMessages.Add(new Message
                {
                    ConversationId = conv.Id,
                    SenderId = senderId,
                    Content = content,
                    MessageType = "text",
                    IsRead = isRead,
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
        _logger.LogInformation("{N} messages seeded.", allMessages.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  14. NOTIFICATIONS
    // ═══════════════════════════════════════════════════════════
    private async Task SeedNotificationsAsync()
    {
        var craftsmen = await _context.Craftsmen
            .IgnoreQueryFilters()
            .Include(c => c.User)
            .ToListAsync();

        var customers = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.Role == "customer")
            .ToListAsync();

        var jobs = await _context.Jobs.IgnoreQueryFilters().ToListAsync();

        User CU(string email) => customers.First(u => u.Email == email);
        Craftsman CM(string email) => craftsmen.First(c => c.User.Email == email);
        Job J(string fragment) => jobs.First(j => j.Description.Contains(fragment));

        var notifications = new List<Notification>
        {
            // Unread notification — craftsman approval
            new()
            {
                UserId       = CM("ahmed.ali@gmail.com").UserId,
                Title        = "طلب التسجيل قيد المراجعة",
                Body         = "تم استلام طلب تسجيلك كحرفي وهو قيد المراجعة من قبل الإدارة. سنُبلغك بالنتيجة خلال 48 ساعة.",
                Type         = "registration_pending",
                RelatedJobId = null,
                IsRead       = false,
                CreatedAt    = CM("ahmed.ali@gmail.com").CreatedAt.AddMinutes(10)
            },

            // Read notification — job accepted
            new()
            {
                UserId       = CU("sara.ahmed@gmail.com").Id,
                Title        = "تم قبول طلبك",
                Body         = "قام الحرفي إبراهيم نصر بقبول طلب الخدمة الخاص بك. سيحضر في الموعد المتفق عليه.",
                Type         = "job_accepted",
                RelatedJobId = J("صيانة دورية شاملة").Id,
                IsRead       = true,
                CreatedAt    = J("صيانة دورية شاملة").CreatedAt.AddHours(2)
            },

            // Notification for approved craftsman
            new()
            {
                UserId       = CM("mohamed.hassan@gmail.com").UserId,
                Title        = "تم اعتمادك كحرفي",
                Body         = "مبروك! تم اعتماد طلبك بنجاح. يمكنك الآن استقبال طلبات العملاء والبدء في العمل على المنصة.",
                Type         = "approved",
                RelatedJobId = null,
                IsRead       = true,
                CreatedAt    = CM("mohamed.hassan@gmail.com").CreatedAt.AddDays(2)
            },

            // Notification for rejected craftsman
            new()
            {
                UserId       = CM("hussien.reda@gmail.com").UserId,
                Title        = "تم رفض طلب التسجيل",
                Body         = "نأسف لإبلاغك بأنه تم رفض طلب تسجيلك. السبب: بيانات الهوية الوطنية غير واضحة وغير مطابقة للاسم المسجل. يمكنك إعادة التقديم بعد تصحيح البيانات.",
                Type         = "rejected",
                RelatedJobId = null,
                IsRead       = false,
                CreatedAt    = CM("hussien.reda@gmail.com").CreatedAt.AddDays(3)
            },

            // Notification — job completed (customer to review)
            new()
            {
                UserId       = CU("nourhan.mohamed@gmail.com").Id,
                Title        = "تم إنجاز طلبك",
                Body         = "أنهى الحرفي العمل. يمكنك الآن تقييم الخدمة ومشاركة تجربتك مع العملاء الآخرين.",
                Type         = "job_completed",
                RelatedJobId = J("نقاط إضاءة جديدة").Id,
                IsRead       = true,
                CreatedAt    = J("نقاط إضاءة جديدة").CompletedAt!.Value.AddMinutes(30)
            },

            // Notification — new message (unread)
            new()
            {
                UserId       = CU("mennatallah.khaled@gmail.com").Id,
                Title        = "رسالة جديدة من محمد حسن",
                Body         = "أهلاً بيك، أنا شفت الطلب. امتى تقدر تستقبلني؟",
                Type         = "new_message",
                RelatedJobId = null,
                IsRead       = false,
                CreatedAt    = BaseDate.AddMonths(5).AddDays(15).AddMinutes(13)
            },

            // Notification — dispute opened
            new()
            {
                UserId       = CM("ibrahim.nasr@gmail.com").UserId,
                Title        = "تم فتح نزاع على طلبك",
                Body         = "قامت إدارة حرفي بفتح نزاع على طلب الخدمة رقم 6. سيتواصل معك فريق الدعم خلال 24 ساعة.",
                Type         = "dispute_opened",
                RelatedJobId = J("إصلاح عطل في دائرة الطاقة").Id,
                IsRead       = false,
                CreatedAt    = J("إصلاح عطل في دائرة الطاقة").DisputeRaisedAt!.Value.AddMinutes(5)
            },

            // Notification — dispute resolved
            new()
            {
                UserId       = CU("sara.ahmed@gmail.com").Id,
                Title        = "تم حل النزاع لصالحك",
                Body         = "تم حل النزاع الخاص بطلب الدهان. القرار: إعادة دهان الأجزاء المخالفة أو استرداد 30% من المبلغ.",
                Type         = "dispute_resolved",
                RelatedJobId = J("دهان كامل لشقة 3 غرف").Id,
                IsRead       = true,
                CreatedAt    = J("دهان كامل لشقة 3 غرف").DisputeResolvedAt!.Value.AddMinutes(30)
            }
        };

        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();
        _logger.LogInformation("{N} notifications seeded.", notifications.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  15. ADMIN AUDIT LOGS
    // ═══════════════════════════════════════════════════════════
    private async Task SeedAdminAuditLogsAsync()
    {
        var adminUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Role == "admin");

        if (adminUser is null)
        {
            _logger.LogWarning("Admin not found for audit log seeding.");
            return;
        }

        var craftsmen = await _context.Craftsmen.IgnoreQueryFilters()
            .Include(c => c.User).ToListAsync();
        var users = await _context.Users.IgnoreQueryFilters().ToListAsync();
        var reviews = await _context.Reviews.IgnoreQueryFilters().ToListAsync();
        var jobs = await _context.Jobs.IgnoreQueryFilters().ToListAsync();

        Craftsman CM(string email) => craftsmen.First(c => c.User.Email == email);
        User CU(string email) => users.First(u => u.Email == email);

        var logs = new List<AdminAuditLog>
        {
            // Log: craftsman approval
            new()
            {
                AdminId    = adminUser.Id,
                Action     = "approve_craftsman",
                TargetType = "Craftsman",
                TargetId   = CM("mohamed.hassan@gmail.com").Id,
                Notes      = "Approved craftsman محمد حسن after verifying national ID",
                IpAddress  = "197.58.112.44",
                CreatedAt  = CM("mohamed.hassan@gmail.com").CreatedAt.AddDays(2)
            },
            // Log: craftsman rejection
            new()
            {
                AdminId    = adminUser.Id,
                Action     = "reject_craftsman",
                TargetType = "Craftsman",
                TargetId   = CM("hussien.reda@gmail.com").Id,
                Notes      = "Rejected craftsman حسين رضا. Reason: بيانات الهوية الوطنية غير واضحة وغير مطابقة للاسم المسجل",
                IpAddress  = "197.58.112.44",
                CreatedAt  = CM("hussien.reda@gmail.com").CreatedAt.AddDays(3)
            },
            // Log: user deactivation
            new()
            {
                AdminId    = adminUser.Id,
                Action     = "deactivate_user",
                TargetType = "User",
                TargetId   = CU("maryam.ali@gmail.com").Id,
                Notes      = "Deactivated user مريم علي. Reason: إساءة استخدام المنصة وتقديم بيانات مزورة",
                IpAddress  = "197.58.112.44",
                CreatedAt  = BaseDate.AddMonths(3)
            },
            // Log: review deletion
            new()
            {
                AdminId    = adminUser.Id,
                Action     = "delete_review",
                TargetType = "Review",
                TargetId   = reviews.First(r => r.IsDeleted).Id,
                Notes      = "Soft-deleted review. Reason: التقييم يحتوي على ألفاظ مسيئة وتهديدات شخصية تخالف شروط الاستخدام",
                IpAddress  = "197.58.112.44",
                CreatedAt  = reviews.First(r => r.IsDeleted).DeletedAt!.Value
            },
            // Log: dispute flagging
            new()
            {
                AdminId    = adminUser.Id,
                Action     = "flag_dispute",
                TargetType = "Job",
                TargetId   = jobs.First(j => j.IsDisputed).Id,
                Notes      = "Flagged dispute. Reason: طلب المستخدم مراجعة جودة العمل المنجز",
                IpAddress  = "197.58.112.44",
                CreatedAt  = jobs.First(j => j.IsDisputed).DisputeRaisedAt!.Value
            },
            // Log: dispute resolution
            new()
            {
                AdminId    = adminUser.Id,
                Action     = "resolve_dispute",
                TargetType = "Job",
                TargetId   = jobs.First(j => j.DisputeResolution != null && j.DisputeResolution.Contains("Customer")).Id,
                Notes      = "Resolved dispute. Resolution: استرداد 30% من المبلغ. Favored: Customer",
                IpAddress  = "197.58.112.44",
                CreatedAt  = jobs.First(j => j.DisputeResolution != null && j.DisputeResolution.Contains("Customer")).DisputeResolvedAt!.Value
            },
            // Log: craftsman suspension
            new()
            {
                AdminId    = adminUser.Id,
                Action     = "suspend_craftsman",
                TargetType = "Craftsman",
                TargetId   = CM("mostafa.mahmoud@gmail.com").Id,
                Notes      = "Suspended craftsman مصطفى محمود. Reason: شكاوى متكررة من العملاء",
                IpAddress  = "197.58.112.44",
                CreatedAt  = BaseDate.AddMonths(4)
            },
            // Log: deleted craftsman after approval
            new()
            {
                AdminId    = adminUser.Id,
                Action     = "delete_craftsman",
                TargetType = "Craftsman",
                TargetId   = CM("kareem.samy@gmail.com").Id,
                Notes      = "Soft-deleted craftsman كريم سامي. Reason: تلقي شكاوى متعددة من العملاء ورفض الرد على طلبات التواصل",
                IpAddress  = "197.58.112.44",
                CreatedAt  = CM("kareem.samy@gmail.com").DeletedAt!.Value
            }
        };

        await _context.AdminAuditLogs.AddRangeAsync(logs);
        await _context.SaveChangesAsync();
        _logger.LogInformation("{N} admin audit logs seeded.", logs.Count);
    }

    // ═══════════════════════════════════════════════════════════
    //  16. REPORTS
    // ═══════════════════════════════════════════════════════════
    private async Task SeedReportsAsync()
    {
        var customers = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.Role == "customer")
            .ToListAsync();

        var craftsmen = await _context.Craftsmen
            .IgnoreQueryFilters()
            .Include(c => c.User)
            .ToListAsync();

        var adminUser = await _context.Users.IgnoreQueryFilters()
            .FirstAsync(u => u.Role == "admin");

        User CU(string email) => customers.First(u => u.Email == email);
        Craftsman CM(string email) => craftsmen.First(c => c.User.Email == email);

        var reports = new List<Report>
        {
            // Pending report — customer reporting suspended craftsman
            new()
            {
                ReportedByUserId = CU("sara.ahmed@gmail.com").Id,
                TargetType       = "Craftsman",
                TargetId         = CM("mostafa.mahmoud@gmail.com").Id,
                Reason           = "الحرفي تأخر كثيراً ولم يُبلغ بالتأخير وطلب مبلغاً إضافياً غير متفق عليه",
                Status           = "pending",
                CreatedAt        = BaseDate.AddMonths(3).AddDays(20)
            },
            // Resolved report
            new()
            {
                ReportedByUserId  = CU("nourhan.mohamed@gmail.com").Id,
                TargetType        = "Craftsman",
                TargetId          = CM("abdallah.khaled@gmail.com").Id,
                Reason            = "الحرفي استخدم مواد رديئة الجودة وخالف شروط العقد",
                Status            = "resolved",
                ResolvedByAdminId = adminUser.Id,
                ResolutionNotes   = "تم التحقق من الشكوى وتوجيه تحذير رسمي للحرفي. (Action: warning_issued)",
                CreatedAt         = BaseDate.AddMonths(2),
                ResolvedAt        = BaseDate.AddMonths(2).AddDays(5)
            }
        };

        await _context.Reports.AddRangeAsync(reports);
        await _context.SaveChangesAsync();
        _logger.LogInformation("{N} reports seeded.", reports.Count);
    }
}
