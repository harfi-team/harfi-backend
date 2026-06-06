# Harfi Database Seeding Guide

**Audience:** Backend team members setting up the demo database.
**Purpose:** Documents what `DataSeeder.cs` does, how to run it, and what data it produces.

---

## Section 1 — Overview

`DataSeeder.cs` populates the Harfi database with realistic Egyptian-Arabic demo data so the team can test all features without manually creating records. It wipes the relevant tables on every startup and inserts fresh data: users, craftsmen profiles, completed jobs, reviews, conversations, and chat messages.

---

## Section 2 — Prerequisites

- .NET SDK **8.0** (matching `net8.0` in all `.csproj` files)
- SQL Server instance running (local or remote)
- A valid connection string set via **User Secrets** (not in `appsettings.json`):
  ```bash
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=HarfiDb;Trusted_Connection=True;TrustServerCertificate=True"
  ```
- All required secrets for the app to start (JWT secret, Voyage API key, SMTP credentials), otherwise the middleware will not throw — the seeder itself does not depend on external APIs

---

## Section 3 — How to run

```bash
dotnet run --project Harfi.API
```

That is the only command needed. The seeder runs automatically on every startup inside `Program.cs` (after `app.Build()`).

---

## Section 4 — What happens on startup

1. **Auto-migrate** — EF Core applies any pending migrations to the database.
2. **Clear all seeded data** — deletes rows in FK-safe order (children first):
   - `Messages`
   - `Conversations`
   - `Reviews`
   - `RAGDocuments`
   - `Jobs`
   - `Craftsmen`
   - `Users` (via `UserManager.DeleteAsync` — cascades to `RefreshTokens`, `Notifications`, `AIChatMessages`, `UserConnections`, `EmailVerifications`, `PhoneVerifications`)
3. **Seed admin user** — 1 admin account.
4. **Seed craftsman users** — 10 user accounts with role `craftsman`.
5. **Seed customer users** — 5 user accounts with role `customer`.
6. **Seed craftsman profiles** — 10 profiles linked to the craftsman users.
7. **Seed jobs** — 20 completed jobs (status `مكتمل`), 2 per craftsman.
8. **Seed reviews** — 15 reviews (one per job, for the first 15 jobs).
9. **Recalculate ratings** — updates each craftsman's `Rating` column to the average of their reviews.
10. **Seed conversations** — 10 conversations (one per job, for the first 10 jobs).
11. **Seed messages** — 40 messages (4 per conversation, alternating customer/craftsman).

---

## Section 5 — Seeded data reference

### Users (16 total)

| Name | Email | Password | Role |
|------|-------|----------|------|
| Harfi Admin | `admin@harfi.com` | `Admin@1234` | admin |
| أحمد علي | `ahmed.ali@gmail.com` | `Harfi@2024` | craftsman |
| محمد حسن | `mohamed.hassan@gmail.com` | `Harfi@2024` | craftsman |
| عبدالله خالد | `abdallah.khaled@gmail.com` | `Harfi@2024` | craftsman |
| مصطفى محمود | `mostafa.mahmoud@gmail.com` | `Harfi@2024` | craftsman |
| حسين رضا | `hussien.reda@gmail.com` | `Harfi@2024` | craftsman |
| كريم سامي | `kareem.samy@gmail.com` | `Harfi@2024` | craftsman |
| يوسف عادل | `youssef.adel@gmail.com` | `Harfi@2024` | craftsman |
| إبراهيم نصر | `ibrahim.nasr@gmail.com` | `Harfi@2024` | craftsman |
| عمرو شريف | `amr.sherif@gmail.com` | `Harfi@2024` | craftsman |
| خالد أحمد | `khaled.ahmed@gmail.com` | `Harfi@2024` | craftsman |
| سارة أحمد | `sara.ahmed@gmail.com` | `Harfi@2024` | customer |
| نورهان محمد | `nourhan.mohamed@gmail.com` | `Harfi@2024` | customer |
| مريم علي | `maryam.ali@gmail.com` | `Harfi@2024` | customer |
| فاطمة حسن | `fatma.hassan@gmail.com` | `Harfi@2024` | customer |
| منة الله خالد | `mennatallah.khaled@gmail.com` | `Harfi@2024` | customer |

### Craftsmen (10 profiles)

| Name | Service | City | Price Min | Price Max | Exp | Rating | Bio |
|------|---------|------|-----------|-----------|-----|--------|-----|
| أحمد علي | سباك | مدينة نصر | 150 | 300 | 8 | 4.7 | سباك محترف خبرة 8 سنوات في تركيب وصيانة جميع أنواع السباكة |
| محمد حسن | سباك | المعادي | 200 | 400 | 12 | 4.5 | معلم سباكة خبرة 12 سنة في حل مشاكل التسربات وتركيب السخانات |
| عبدالله خالد | سباك | الزيتون | 100 | 250 | 5 | 4.2 | سباك عام بأسعار مناسبة وجودة عالية في الشغل |
| مصطفى محمود | كهربائي | شبرا | 200 | 500 | 10 | 4.8 | مهندس كهربائي خبرة 10 سنوات في توصيلات الكهرباء والصيانة |
| حسين رضا | كهربائي | مصر الجديدة | 250 | 450 | 7 | 4.3 | فني كهرباء منازل ومحلات - تركيب وصيانة جميع الأعمال الكهربائية |
| كريم سامي | نجار | العباسية | 300 | 600 | 15 | 4.9 | نجار موبيليا وباركيه خبرة 15 سنة في صناعة وتركيب الأثاث |
| يوسف عادل | نجار | المقطم | 200 | 500 | 6 | 4.0 | نجار عام - تركيب مطابخ وغرف نوم وأبواب وشبابيك |
| إبراهيم نصر | فني تكييف | حلوان | 300 | 700 | 9 | 4.6 | فني تكييف متخصص في تركيب وصيانة جميع أنواع المكيفات |
| عمرو شريف | فني تكييف | الدقي | 350 | 800 | 11 | 4.4 | متخصص في صيانة وتركيب التكييفات بأسعار تنافسية |
| خالد أحمد | نقاش | الهرم | 150 | 400 | 4 | 3.8 | نقاش دهانات وجبس بورد - شغل نضيف وبسعر معقول |

*Note: ratings above are the initial seeded values. After reviews are inserted, the seeder recalculates all ratings as averages.*

### Jobs (20 completed)

| # | Craftsman | Customer | Service | Description |
|---|-----------|----------|---------|-------------|
| 1 | أحمد علي | سارة أحمد | سباك | الحنفية بتنقط في الحمام وفيه تسريب تحت الحوض |
| 2 | أحمد علي | نورهان محمد | سباك | المواسير في المطبخ مسدودة والمياه مش بتصرف |
| 3 | محمد حسن | مريم علي | سباك | سخان المياه مش بيسخن كويس وبيطفي فجأة |
| 4 | محمد حسن | فاطمة حسن | سباك | طرمبة المياه في العمارة عطلانة والمياه مش بتوصل للدور الرابع |
| 5 | عبدالله خالد | منة الله خالد | سباك | فيه ريحة في الحمام والمياه بتتسرب من السيفون |
| 6 | عبدالله خالد | سارة أحمد | سباك | بانيو الحمام مسدود والمياه واقفة |
| 7 | مصطفى محمود | نورهان محمد | كهربائي | المفاتيح في الأوضة الكبيرة وقفت وفيه شرارة في اللوحة |
| 8 | مصطفى محمود | مريم علي | كهربائي | اللمبات في الشقة كلها بتطفي وتفضل لماعة |
| 9 | حسين رضا | فاطمة حسن | كهربائي | المراوح في البيت مش بتشتغل وفصل التيار باستمرار |
| 10 | حسين رضا | منة الله خالد | كهربائي | تكييف الهواء مش شغال في الصالة والمفتاح الكهربائي سخن |
| 11 | كريم سامي | سارة أحمد | نجار | باب الأوضة كسر من المفصلة وعايز تغيير كامل |
| 12 | كريم سامي | نورهان محمد | نجار | دولاب المطبخ واقع من الحائط والأدراج مكسورة |
| 13 | يوسف عادل | مريم علي | نجار | السرير في الغرفة النوم مكسور من القاعدة |
| 14 | يوسف عادل | فاطمة حسن | نجار | الشباك خشب متآكل وعايز تغيير الإطارات |
| 15 | إبراهيم نصر | منة الله خالد | فني تكييف | التكييف مش بيبرد وبيعمل صوت عالي أثناء الشغل |
| 16 | إبراهيم نصر | سارة أحمد | فني تكييف | التكييف بيشقط مية من الوحدة الداخلية |
| 17 | عمرو شريف | نورهان محمد | فني تكييف | الريموت بتاع التكييف مش شغال والتكييف مش بيستجيب |
| 18 | عمرو شريف | مريم علي | فني تكييف | تكييفين في الشقة محتاجين صيانة وتنظيف شاملة |
| 19 | خالد أحمد | فاطمة حسن | نقاش | عايز دهان كامل للشقة 3 أوض وريسبشن |
| 20 | خالد أحمد | منة الله خالد | نقاش | حوائط الصالة فيها تشققات وعايزه تليس ودهان جديد |

### Reviews (15 — one per job, first 15 jobs)

| Job # | Stars | Comment |
|-------|-------|---------|
| 1 | 5 | شغل ممتاز ونظيف جداً، الأستاذ محترم وسريع في الشغل |
| 2 | 4 | شغل كويس بس أتأخر شوية على الموعد |
| 3 | 5 | أحسن سباك تعاملت معاه، شغل نضيف وفي الموعد |
| 4 | 4 | الأستاذ خلص الشغل زي ما اتفقنا، جودة ممتازة |
| 5 | 3 | الشغل اتعمل بس كان في بعض المشاكل في الأول |
| 6 | 5 | ممتاز جداً، أنصح بالتعامل معاه بثقة |
| 7 | 4 | شغل محترم وسعر مناسب، هكلمه تاني أكيد |
| 8 | 5 | فنان في شغله، تعامل محترم ونظيف |
| 9 | 3 | محتاج يهتم شوية بالتفاصيل لكن في النهاية تمام |
| 10 | 5 | أخلاق عالية وشغل هايل، ربنا يبارك له |
| 11 | 4 | ممتاز، التزم بالوقت والسعر المتفق عليه |
| 12 | 2 | الشغل ماشي لكن في حاجات ناقصة محتاج يرجع يظبطها |
| 13 | 5 | أفضل حرفي اشتغلت معاه، محترف وشغله نضيف |
| 14 | 4 | خلص الشغل بسرعة وجودة كويسة الحمد لله |
| 15 | 5 | شغل فخم الصراحة، أسعاره مناسبة جداً |

### Conversations (10 — one per job for jobs 1–10)

Each conversation links one job to its customer and craftsman. All have `LastMessageAt` set to the timestamp of the last message.

### Messages (40 total, 4 per conversation)

| Conv # | Job # | Messages |
|--------|-------|----------|
| 1 | 1 (حنفية بتنقط) | Customer asks for plumber → Craftsman asks when → Customer says tomorrow morning → Craftsman confirms at 10 AM |
| 2 | 2 (مواسير مسدودة) | Customer says pipes clogged → Craftsman asks if tried anything → Customer says yes, no use → Craftsman will come after afternoon |
| 3 | 3 (سخان مياه) | Customer says heater not working → Craftsman says might be the heater element → Customer asks when he can come → Craftsman says tomorrow morning |
| 4 | 4 (طرمبة مياه) | Customer says water pump stopped → Craftsman needs to inspect → Customer asks about inspection cost → Craftsman says free inspection, price depends |
| 5 | 5 (سيفون) | Customer says bad smell in bathroom → Craftsman says siphon might be broken → Customer asks about cost → Craftsman says will inspect first |
| 6 | 6 (بانيو مسدود) | Customer says bathtub clogged → Craftsman asks about chemicals → Customer says tried everything → Craftsman will use high pressure |
| 7 | 7 (مفاتيح بتشرر) | Customer says switches sparking → Craftsman says cut power immediately → Customer says done → Craftsman will come within an hour |
| 8 | 8 (لمبات بتطفي) | Customer says lights flickering → Craftsman says general circuit issue → Customer asks about time → Craftsman says 2-3 hours |
| 9 | 9 (مراوح وقفت) | Customer says fans stopped, power cuts → Craftsman says needs to check → Customer asks about cost → Craftsman says depends on fault |
| 10 | 10 (مفتاح التكييف سخن) | Customer says AC switch hot, power out in living room → Craftsman warns not to touch it → Customer asks when he can come → Craftsman says after sunset today |

---

## Section 6 — Demo credentials

| Role | Name | Email | Password |
|------|------|-------|----------|
| admin | Harfi Admin | `admin@harfi.com` | `Admin@1234` |
| craftsman | أحمد علي | `ahmed.ali@gmail.com` | `Harfi@2024` |
| craftsman | محمد حسن | `mohamed.hassan@gmail.com` | `Harfi@2024` |
| craftsman | عبدالله خالد | `abdallah.khaled@gmail.com` | `Harfi@2024` |
| craftsman | مصطفى محمود | `mostafa.mahmoud@gmail.com` | `Harfi@2024` |
| craftsman | حسين رضا | `hussien.reda@gmail.com` | `Harfi@2024` |
| craftsman | كريم سامي | `kareem.samy@gmail.com` | `Harfi@2024` |
| craftsman | يوسف عادل | `youssef.adel@gmail.com` | `Harfi@2024` |
| craftsman | إبراهيم نصر | `ibrahim.nasr@gmail.com` | `Harfi@2024` |
| craftsman | عمرو شريف | `amr.sherif@gmail.com` | `Harfi@2024` |
| craftsman | خالد أحمد | `khaled.ahmed@gmail.com` | `Harfi@2024` |
| customer | سارة أحمد | `sara.ahmed@gmail.com` | `Harfi@2024` |
| customer | نورهان محمد | `nourhan.mohamed@gmail.com` | `Harfi@2024` |
| customer | مريم علي | `maryam.ali@gmail.com` | `Harfi@2024` |
| customer | فاطمة حسن | `fatma.hassan@gmail.com` | `Harfi@2024` |
| customer | منة الله خالد | `mennatallah.khaled@gmail.com` | `Harfi@2024` |

---

## Section 7 — Verify seeding worked

Run this SQL query in SSMS connected to your Harfi database:

```sql
SELECT 'Users' AS TableName, COUNT(*) AS RowCount FROM Users
UNION ALL
SELECT 'Craftsmen', COUNT(*) FROM Craftsmen
UNION ALL
SELECT 'Jobs', COUNT(*) FROM Jobs
UNION ALL
SELECT 'Reviews', COUNT(*) FROM Reviews
UNION ALL
SELECT 'Conversations', COUNT(*) FROM Conversations
UNION ALL
SELECT 'Messages', COUNT(*) FROM Messages
ORDER BY TableName;
```

Expected output:

| TableName | RowCount |
|-----------|----------|
| Conversations | 10 |
| Craftsmen | 10 |
| Jobs | 20 |
| Messages | 40 |
| Reviews | 15 |
| Users | 16 |

---

## Section 8 — Important rules for the team

- **Full wipe on every run** — the seeder calls `ClearAllDataAsync()` which deletes all rows from the seeded tables before inserting fresh data. Any manually created test data in these tables will be lost. Do not run the app in production with this seeder enabled.
- **Adding a new seeded table** — if you add a new entity to the seed, you must also add its `ExecuteDeleteAsync()` call to `ClearAllDataAsync()` in FK-safe order (children before parents).
- **Delete order matters** — the current delete order is: `Messages` → `Conversations` → `Reviews` → `RAGDocuments` → `Jobs` → `Craftsmen` → `Users`. If you add a table with foreign keys to any of these, add its delete before its parent.
- **Seader file location** — `Harfi.Repositories/Data/DataSeeder.cs`.
- **No real credentials** — never commit real passwords, API keys, or phone numbers to the seeder. The passwords `Harfi@2024` and `Admin@1234` are demo-only and must be changed before any production-like deployment.
- **Tables not seeded** — the seeder does not touch these tables (they remain as-is): `Notifications`, `RefreshTokens`, `AIChatMessages`, `MediaFiles`, `JobFeedbacks`, `UserConnections`, `RAGDocuments`, `EmailVerifications`, `PhoneVerifications`, and the ASP.NET Identity system tables (`AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`). Users are deleted via `UserManager.DeleteAsync` which cascades to related entities.
