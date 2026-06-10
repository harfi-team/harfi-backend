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
        await ClearAllDataAsync();

        await SeedAdminAsync();
        await SeedCraftsmanUsersAsync();
        await SeedCustomerUsersAsync();
        await SeedCraftsmanProfilesAsync();
        await SeedJobsAsync();
        await SeedReviewsAsync();
        await RecalculateRatingsAsync();
        await SeedConversationsAsync();
        await SeedMessagesAsync();
    }

    // ───────────────────────────────────────────────────────────
    //  CLEAR ALL DATA (FK-safe order, children first)
    // ───────────────────────────────────────────────────────────
    private async Task ClearAllDataAsync()
    {
        _logger.LogInformation("Clearing all existing data...");

        await _context.Messages.ExecuteDeleteAsync();
        await _context.Conversations.ExecuteDeleteAsync();
        await _context.Reviews.ExecuteDeleteAsync();
        await _context.RAGDocuments.ExecuteDeleteAsync();
        await _context.Jobs.ExecuteDeleteAsync();
        await _context.Craftsmen.ExecuteDeleteAsync();

        // Use UserManager for Identity users to respect all ASP.NET Identity cascade rules
        var allUsers = await _context.Users.ToListAsync();
        foreach (var user in allUsers)
            await _userManager.DeleteAsync(user);

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
    //  CRAFTSMAN USERS (10)
    // ───────────────────────────────────────────────────────────
    private async Task SeedCraftsmanUsersAsync()
    {
        var craftsmanData = new[]
        {
            (name: "أحمد علي",        phone: "01012345678", email: "ahmed.ali@gmail.com"),
            (name: "محمد حسن",        phone: "01123456789", email: "mohamed.hassan@gmail.com"),
            (name: "عبدالله خالد",    phone: "01234567890", email: "abdallah.khaled@gmail.com"),
            (name: "مصطفى محمود",     phone: "01512345678", email: "mostafa.mahmoud@gmail.com"),
            (name: "حسين رضا",        phone: "01098765432", email: "hussien.reda@gmail.com"),
            (name: "كريم سامي",       phone: "01156789012", email: "kareem.samy@gmail.com"),
            (name: "يوسف عادل",       phone: "01234561234", email: "youssef.adel@gmail.com"),
            (name: "إبراهيم نصر",     phone: "01567890123", email: "ibrahim.nasr@gmail.com"),
            (name: "عمرو شريف",       phone: "01023456789", email: "amr.sherif@gmail.com"),
            (name: "خالد أحمد",       phone: "01134567890", email: "khaled.ahmed@gmail.com")
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

        _logger.LogInformation("10 craftsman users seeded.");
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

    // ───────────────────────────────────────────────────────────
    //  CRAFTSMAN PROFILES (10)
    // ───────────────────────────────────────────────────────────
    private async Task SeedCraftsmanProfilesAsync()
    {
        var users = await _context.Users
            .Where(u => u.Role == "craftsman")
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        var profiles = new (int userId, string serviceType, string city, string? neighborhood,
            decimal? min, decimal? max, int exp, decimal rating, string? bio, string photoId)[]
        {
            (users[0].Id, "سباك",        "مدينة نصر",   null,            150m, 300m, 8,  4.7m,
             "سباك محترف خبرة 8 سنوات في تركيب وصيانة جميع أنواع السباكة", "1"),
            (users[1].Id, "سباك",        "المعادي",     "المعادي",       200m, 400m, 12, 4.5m,
             "معلم سباكة خبرة 12 سنة في حل مشاكل التسربات وتركيب السخانات", "2"),
            (users[2].Id, "سباك",        "الزيتون",     "الزيتون",       100m, 250m, 5,  4.2m,
             "سباك عام بأسعار مناسبة وجودة عالية في الشغل", "3"),
            (users[3].Id, "كهربائي",     "شبرا",        "شبرا",          200m, 500m, 10, 4.8m,
             "مهندس كهربائي خبرة 10 سنوات في توصيلات الكهرباء والصيانة", "4"),
            (users[4].Id, "كهربائي",     "مصر الجديدة", "مصر الجديدة",   250m, 450m, 7,  4.3m,
             "فني كهرباء منازل ومحلات - تركيب وصيانة جميع الأعمال الكهربائية", "5"),
            (users[5].Id, "نجار",        "العباسية",    null,            300m, 600m, 15, 4.9m,
             "نجار موبيليا وباركيه خبرة 15 سنة في صناعة وتركيب الأثاث", "6"),
            (users[6].Id, "نجار",        "المقطم",      "المقطم",        200m, 500m, 6,  4.0m,
             "نجار عام - تركيب مطابخ وغرف نوم وأبواب وشبابيك", "7"),
            (users[7].Id, "فني تكييف",   "حلوان",       "حلوان",         300m, 700m, 9,  4.6m,
             "فني تكييف متخصص في تركيب وصيانة جميع أنواع المكيفات", "8"),
            (users[8].Id, "فني تكييف",   "الدقي",       "الدقي",         350m, 800m, 11, 4.4m,
             "متخصص في صيانة وتركيب التكييفات بأسعار تنافسية", "9"),
            (users[9].Id, "نقاش",        "الهرم",       "الهرم",         150m, 400m, 4,  3.8m,
             "نقاش دهانات وجبس بورد - شغل نضيف وبسعر معقول", "10")
        };

        foreach (var p in profiles)
        {
            _context.Craftsmen.Add(new Craftsman
            {
                UserId = p.userId,
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
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("10 craftsman profiles seeded.");
    }

    // ───────────────────────────────────────────────────────────
    //  JOBS (20) — all completed
    // ───────────────────────────────────────────────────────────
    private async Task SeedJobsAsync()
    {
        var craftsmen = await _context.Craftsmen.OrderBy(c => c.Id).ToListAsync();
        var customers = await _context.Users
            .Where(u => u.Role == "customer")
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        var baseDate = DateTime.UtcNow.AddMonths(-3);

        var jobData = new (int craftsmanIdx, int customerIdx, string description, string address,
            string? problemDesc, string? solutionDesc)[]
        {
            (0, 0, "الحنفية بتنقط في الحمام وفيه تسريب تحت الحوض",
             "15 شارع الجيش، مدينة نصر، القاهرة",
             "تسريب مياه من الحنفية وتحت الحوض", "تم تغيير الحنفية وإصلاح التسريب"),

            (0, 1, "المواسير في المطبخ مسدودة والمياه مش بتصرف",
             "8 شارع الطيران، مدينة نصر، القاهرة",
             "انسداد كامل في مواسير المطبخ", "تم تسليك المواسير بالضغط العالي"),

            (1, 2, "سخان المياه مش بيسخن كويس وبيطفي فجأة",
             "22 شارع 9، المعادي، القاهرة",
             "السخان لا يعمل بكفاءة وينطفئ", "تم تنظيف الترموستات واستبدال الهيتر"),

            (1, 3, "طرمبة المياه في العمارة عطلانة والمياه مش بتوصل للدور الرابع",
             "4 شارع النصر، المعادي، القاهرة",
             "طرمبة المياه لا تعمل نهائياً", "تم تغيير الطرمبة بأخري جديدة"),

            (2, 4, "فيه ريحة في الحمام والمياه بتتسرب من السيفون",
             "12 شارع أبو بكر، الزيتون، القاهرة",
             "تسريب من سيفون الحمام", "تم تغيير السيفون بالكامل"),

            (2, 0, "بانيو الحمام مسدود والمياه واقفة",
             "3 شارع الترعة، الزيتون، القاهرة",
             "انسداد في مصرف البانيو", "تم تسليك البانيو وإزالة الدهون المتراكمة"),

            (3, 1, "المفاتيح في الأوضة الكبيرة وقفت وفيه شرارة في اللوحة",
             "18 شارع شبرا، شبرا، القاهرة",
             "تماس كهربائي في المفاتيح واللوحة", "تم تغيير المفاتيح وتجديد اللوحة"),

            (3, 2, "اللمبات في الشقة كلها بتطفي وتفضل لماعة",
             "7 شارع أحمد حلمي، شبرا، القاهرة",
             "عطل في الدائرة الكهربائية العامة", "تم إصلاح العطل وتغيير القواطع"),

            (4, 3, "المراوح في البيت مش بتشتغل وفصل التيار باستمرار",
             "25 شارع الحجاز، مصر الجديدة، القاهرة",
             "انقطاع متكرر في التيار الكهربائي", "تم تغيير الأسلاك وتدعيم الدائرة"),

            (4, 4, "تكييف الهواء مش شغال في الصالة والمفتاح الكهربائي سخن",
             "10 شارع الميرغني، مصر الجديدة، القاهرة",
             "ارتفاع درجة حرارة المفتاح الكهربائي للتكييف", "تم استبدال المفتاح الكهربائي"),

            (5, 0, "باب الأوضة كسر من المفصلة وعايز تغيير كامل",
             "6 شارع العباسية، العباسية، القاهرة",
             "باب خشب مكسور من المفصلات", "تم تركيب باب جديد بمفصلات قوية"),

            (5, 1, "دولاب المطبخ واقع من الحائط والأدراج مكسورة",
             "14 شارع رمسيس، العباسية، القاهرة",
             "سقوط الدولاب من الحائط", "تم تثبيت الدولاب على الحائط وتغيير الأدراج"),

            (6, 2, "السرير في الغرفة النوم مكسور من القاعدة",
             "30 شارع المقطم، المقطم، القاهرة",
             "قاعدة السرير الخشبية مكسورة", "تم تصنيع قاعدة جديدة وتركيبها"),

            (6, 3, "الشباك خشب متآكل وعايز تغيير الإطارات",
             "5 شارع اللبيني، المقطم، القاهرة",
             "إطارات الشبابيك متآكلة بسبب الرطوبة", "تم تغيير إطارات الشبابيك بالكامل"),

            (7, 4, "التكييف مش بيبرد وبيعمل صوت عالي أثناء الشغل",
             "20 شارع حلوان، حلوان، القاهرة",
             "التكييف لا يبرد ويصدر ضوضاء", "تم تنظيف الفلاتر وشحن الفريون"),

            (7, 0, "التكييف بيشقط مية من الوحدة الداخلية",
             "11 شارع الملك فيصل، حلوان، القاهرة",
             "تسريب مياه من الوحدة الداخلية للتكييف", "تم تنظيف صرف التكييف وإزالة الانسداد"),

            (8, 1, "الريموت بتاع التكييف مش شغال والتكييف مش بيستجيب",
             "9 شارع التحرير، الدقي، القاهرة",
             "عدم استجابة التكييف للريموت", "تم استبدال الريموت وإصلاح وحدة التحكم"),

            (8, 2, "تكييفين في الشقة محتاجين صيانة وتنظيف شاملة",
             "16 شارع الدقي، الدقي، القاهرة",
             "تراكم الأتربة في الفلاتر", "تم عمل صيانة شاملة وتنظيف التكييفين"),

            (9, 3, "عايز دهان كامل للشقة 3 أوض وريسبشن",
             "28 شارع الهرم، الهرم، الجيزة",
             "دهانات قديمة متشققة ومتقشرة", "تم دهان الشقة بالكامل بدهان حديث"),

            (9, 4, "حوائط الصالة فيها تشققات وعايزه تليس ودهان جديد",
             "35 شارع فيصل، الهرم، الجيزة",
             "تشققات في حوائط الصالة", "تم تلييس ودهان الصالة بالكامل")
        };

        for (int i = 0; i < jobData.Length; i++)
        {
            var j = jobData[i];
            var jobDate = baseDate.AddDays(i * 4);
            var completedDays = (i % 7) + 1;

            _context.Jobs.Add(new Job
            {
                CustomerId = customers[j.customerIdx].Id,
                CraftsmanId = craftsmen[j.craftsmanIdx].Id,
                Status = JobStatusConstants.Done,
                ServiceType = craftsmen[j.craftsmanIdx].ServiceType,
                Description = j.description,
                Address = j.address,
                ProblemDescription = j.problemDesc,
                SolutionDescription = j.solutionDesc,
                CreatedAt = jobDate,
                CompletedAt = jobDate.AddDays(completedDays)
            });
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("20 jobs seeded.");
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
            var conv = conversations[convIdx];
            var custId = conv.CustomerId;
            var craftUserId = craftsmanUserIds[conv.CraftsmanId];

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