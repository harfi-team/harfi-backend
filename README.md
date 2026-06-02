# 🔨 Harfi API — مشروع حرفي

> Arabic-first platform connecting Egyptian customers with verified craftsmen.

---

## 📁 Project Structure (N-Tier Layered)

```
Harfi/
├── Harfi.API/                  ← Entry point — Controllers, Middleware, Program.cs
│   ├── Controllers/            ← HTTP endpoints (one file per feature)
│   ├── Middleware/             ← Global error handling
│   ├── Extensions/             ← DI registration helpers
│   ├── Program.cs              ← App setup & DI config
│   └── appsettings.json        ← Config (never commit secrets!)
│
├── Harfi.Services/             ← Business Logic
│   ├── Interfaces/             ← IAuthService, ICraftsmanService, etc.
│   └── Implementations/        ← AuthService, CraftsmanService, etc.
│
├── Harfi.Repositories/         ← Data Access
│   ├── Data/
│   │   ├── AppDbContext.cs     ← EF Core DbContext (all tables + relations)
│   │   └── Migrations/         ← Auto-generated — DO NOT edit manually
│   ├── Interfaces/             ← IGenericRepository, IUserRepository, etc.
│   └── Implementations/        ← GenericRepository, UserRepository, etc.
│
├── Harfi.Models/               ← Database Entities (pure C# classes)
│   └── Entities/               ← User, Craftsman, Job, Review, etc.
│
└── Harfi.DTOs/                 ← Request/Response shapes (no DB logic)
    ├── Auth/                   ← RegisterDto, LoginDto, AuthResponseDto
    ├── Craftsman/              ← CraftsmanDto, CreateCraftsmanDto
    └── Job/                    ← JobDto, CreateJobDto
```

### Dependency Flow (top → bottom only):
```
API  →  Services  →  Repositories  →  Models
 ↘                          ↗
        DTOs ──────────────
```

---

## 🚀 Quick Start (First Time Setup)

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server (or Docker — see below)
- Git

### 1. Clone the repo
```bash
git clone https://github.com/harfi-team/harfi-backend.git
```


### 2. Configure & Run

> 📄 Follow the full setup guide in **[SETUP.md](./SETUP.md)**  
> It covers connection string, Gmail app password, migrations, and how to verify everything works.
> ⚠️ Never commit real API keys or connection strings to Git!
> ⚠️ **Team members:** never run `migrations add` — see [Migration Rules](#️-migration-rules) below.


### 3. Open Swagger UI
Navigate to: **https://localhost:5000** (or **http://localhost:5108**)

---

## 🗄️ Database Tables

| Table              | Purpose                                          |
|--------------------|--------------------------------------------------|
| Users              | All users (customers, craftsmen, admins)         |
| Craftsmen          | Craftsman profile extension of Users             |
| Jobs               | Service requests                                 |
| Conversations      | Chat container for Customer ↔ Craftsman          |
| Messages           | Individual messages in a Conversation            |
| Reviews            | Job ratings (1 per job, only when done)          |
| Notifications      | Persisted notifications (SignalR backup)         |
| RefreshTokens      | JWT refresh token management                     |
| AIChatMessages     | AI Agent conversation history (Semantic Kernel)  |
| RAGDocuments       | ChromaDB document references                     |
| MediaFiles         | Cloudinary file tracking                         |
| JobFeedbacks       | AI guidance feedback (helpful/not)               |
| UserConnections    | SignalR Hub connection tracking                  |
| EmailVerifications | Email verification codes and expiry tracking     |
| AspNetUserClaims   | Identity user claims (added by Identity)         |
| AspNetUserLogins   | Identity external logins (added by Identity)     |
| AspNetUserTokens   | Identity auth tokens (added by Identity)         |
 
---

## 📋 Team Coding Rules

### 1. Naming Conventions
```csharp
// Controllers  → plural noun + Controller
UsersController, JobsController

// Services     → interface starts with I
IAuthService, ICraftsmanService

// Repositories → interface starts with I
IUserRepository, ICraftsmanRepository

// DTOs         → purpose + Dto
CreateJobDto, CraftsmanResponseDto
```

### 2. Never put business logic in Controllers
```csharp
// ❌ WRONG — logic in controller
[HttpPost]
public async Task<IActionResult> Register(RegisterDto dto)
{
    var hash = BCrypt.HashPassword(dto.Password); // ← NO!
    ...
}

// ✅ CORRECT — delegate to service
[HttpPost]
public async Task<IActionResult> Register(RegisterDto dto)
{
    var result = await _authService.RegisterAsync(dto); // ← YES
    return Ok(result);
}
```

### 3. All endpoints must have [Authorize] unless public
```csharp
[Authorize]                          // requires any valid JWT
[Authorize(Roles = "admin")]         // requires admin role
[Authorize(Roles = "craftsman")]     // requires craftsman role
[AllowAnonymous]                     // explicitly public
```

### 4. Always use DTOs — never return raw entities
```csharp
// ❌ WRONG
return Ok(user); // exposes PasswordHash!

// ✅ CORRECT
return Ok(_mapper.Map<UserResponseDto>(user));
```

### 5. Async all the way
```csharp
// ❌ WRONG
public User GetUser(int id) => _repo.GetById(id);

// ✅ CORRECT
public async Task<User?> GetUserAsync(int id) => await _repo.GetByIdAsync(id);
```

---

## 🔒 Security Notes

- Passwords are hashed by **ASP.NET Core Identity PasswordHasher (PBKDF2)** — never store plain text
- JWT tokens expire after **60 minutes** — use refresh tokens for long sessions
- Secrets go in **.NET User Secrets** locally and **Azure Key Vault** in production — never in `appsettings.json`
- Soft delete everywhere — `IsActive = false` instead of deleting rows

---

## 👥 Team & Assignments

| Name    | Role              | Assignment                                                |
|---------|-------------------|-----------------------------------------------------------|
| Esraa   | Backend Lead      | Project Structure Setup + ASP.NET Core Identity + JWT Auth |
| Hadeer  | Backend           | Phase 2: Profiles, Administrative Controls & Filters      |
| Habiba  | Backend           | Phase 3: Booking System & State Machinery                 |
| Mazen   | Backend           | Phase 4: Reviews & Closures                               |
| Ebrahim | Backend           | Phase 5: Real-time Comms                                  |
| Ahmed   | Backend           | Phase 6: Orchestration Agent                              |

---

## 🗂️ Who Works Where

> Each member owns their files end-to-end: Model → Repository → Service → Controller → DTOs.  
> **Never edit someone else's files without telling them first.**

---

### Esraa — Project Structure + ASP.NET Core Identity + JWT Auth ✅
```
Harfi.API/
├── Program.cs
├── appsettings.json
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Extensions/
│   └── ServiceExtensions.cs
└── Controllers/
    └── AuthController.cs

Harfi.Services/
├── Interfaces/IAuthService.cs
├── Interfaces/IEmailService.cs
├── Implementations/AuthService.cs
└── Implementations/EmailService.cs

Harfi.Repositories/
├── Data/AppDbContext.cs
├── Data/AppDbContextFactory.cs
├── Data/DataSeeder.cs
├── Data/Migrations/
├── Interfaces/IGenericRepository.cs
├── Interfaces/IUserRepository.cs
├── Implementations/GenericRepository.cs
└── Implementations/UserRepository.cs

Harfi.Models/Entities/
├── User.cs
├── RefreshToken.cs
└── EmailVerification.cs

Harfi.DTOs/Auth/
├── RegisterDto.cs
├── LoginDto.cs
├── VerifyEmailDto.cs
├── ResendCodeDto.cs
├── RefreshTokenRequestDto.cs
└── AuthResponseDto.cs
```

---

### Hadeer — Phase 2: Profiles, Administrative Controls & Filters
```
Harfi.API/Controllers/
├── UsersController.cs
├── CraftsmenController.cs
└── AdminController.cs

Harfi.Services/
├── Interfaces/
│   ├── IUserService.cs
│   ├── ICraftsmanService.cs
│   └── IAdminService.cs
└── Implementations/
    ├── UserService.cs
    ├── CraftsmanService.cs
    └── AdminService.cs

Harfi.Repositories/
├── Interfaces/ICraftsmanRepository.cs
└── Implementations/CraftsmanRepository.cs

Harfi.Models/Entities/
└── Craftsman.cs                ← already created by Esraa ✅ do not recreate

Harfi.DTOs/
├── User/
│   ├── UserProfileDto.cs
│   └── UpdateUserDto.cs
└── Craftsman/
    ├── CraftsmanDto.cs
    ├── CreateCraftsmanDto.cs
    └── CraftsmanFilterDto.cs
```

---

### Habiba — Phase 3: Booking System & State Machinery
```
Harfi.API/Controllers/
└── JobsController.cs

Harfi.Services/
├── Interfaces/IJobService.cs
└── Implementations/JobService.cs

Harfi.Repositories/
├── Interfaces/IJobRepository.cs
└── Implementations/JobRepository.cs

Harfi.Models/Entities/
└── Job.cs                      ← already created by Esraa ✅ do not recreate

Harfi.DTOs/Job/
├── CreateJobDto.cs
├── JobDto.cs
├── UpdateJobStatusDto.cs
└── JobResponseDto.cs
```

---

### Mazen — Phase 4: Reviews & Closures
```
Harfi.API/Controllers/
└── ReviewsController.cs

Harfi.Services/
├── Interfaces/
│   ├── IReviewService.cs
│   └── IJobFeedbackService.cs
└── Implementations/
    ├── ReviewService.cs
    └── JobFeedbackService.cs

Harfi.Repositories/
├── Interfaces/
│   ├── IReviewRepository.cs
│   └── IJobFeedbackRepository.cs
└── Implementations/
    ├── ReviewRepository.cs
    └── JobFeedbackRepository.cs

Harfi.Models/Entities/
├── Review.cs                   ← already created by Esraa ✅ do not recreate
└── JobFeedback.cs              ← already created by Esraa ✅ do not recreate

Harfi.DTOs/Review/
├── CreateReviewDto.cs
└── ReviewResponseDto.cs
```

---

### Ebrahim — Phase 5: Real-time Comms
```
Harfi.API/
├── Controllers/
│   ├── ConversationsController.cs
│   └── NotificationsController.cs
└── Hubs/                          ← create this folder
    ├── ChatHub.cs
    └── NotificationHub.cs

Harfi.Services/
├── Interfaces/
│   ├── IConversationService.cs
│   ├── IMessageService.cs
│   └── INotificationService.cs
└── Implementations/
    ├── ConversationService.cs
    ├── MessageService.cs
    └── NotificationService.cs

Harfi.Repositories/
├── Interfaces/
│   ├── IConversationRepository.cs
│   ├── IMessageRepository.cs
│   └── INotificationRepository.cs
└── Implementations/
    ├── ConversationRepository.cs
    ├── MessageRepository.cs
    └── NotificationRepository.cs

Harfi.Models/Entities/
├── Conversation.cs             ← already created by Esraa ✅ do not recreate
├── Message.cs                  ← already created by Esraa ✅ do not recreate
├── Notification.cs             ← already created by Esraa ✅ do not recreate
└── UserConnection.cs           ← already created by Esraa ✅ do not recreate

Harfi.DTOs/Chat/
├── SendMessageDto.cs
├── MessageDto.cs
└── ConversationDto.cs
```

---

### Ahmed — Phase 6: Orchestration Agent
```
Harfi.API/Controllers/
└── AIController.cs

Harfi.Services/
├── Interfaces/
│   ├── IAIAgentService.cs
│   └── IRAGService.cs
└── Implementations/
    ├── AIAgentService.cs
    └── RAGService.cs

Harfi.Repositories/
├── Interfaces/
│   ├── IAIChatRepository.cs
│   └── IRAGRepository.cs
└── Implementations/
    ├── AIChatRepository.cs
    └── RAGRepository.cs

Harfi.Models/Entities/
├── AIChatMessage.cs            ← already created by Esraa ✅ do not recreate
└── RAGDocument.cs              ← already created by Esraa ✅ do not recreate

Harfi.DTOs/AI/
├── AIChatRequestDto.cs
├── AIChatResponseDto.cs
└── RAGDocumentDto.cs
```

---

## ⚠️ Migration Rules — Read Before Anything

### ❌ Never Do This
- Never run `dotnet ef migrations add` — not even to test
- Never edit any file inside `Harfi.Repositories/Data/Migrations/`
- Never modify `AppDbContext.cs` without discussing it in the group first

### ✅ The Only Thing You Should Run
```bash
# Only after git pull origin dev
git pull origin dev
dotnet ef database update --project Harfi.Repositories --startup-project Harfi.API
dotnet run --project Harfi.API
```

### If You Need a Database Schema Change
Open a GitHub issue or send Esraa a message describing the change you need.
Esraa is the only one who creates migrations and pushes them.

> 📌 **Note:** These rules exist purely to avoid merge conflicts and keep the database
> in sync across all machines — not to impose authority on anyone.
> Everyone's input on schema changes is welcome, just route it through one person.