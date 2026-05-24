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
git clone https://github.com/your-org/harfi.git
cd harfi/Harfi
```

### 2. Run the setup script (Windows)
```powershell
.\setup.ps1
```

### 3. Configure your connection string
Open `Harfi.API/appsettings.json` and update:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=HarfiDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```
> ⚠️ Never commit real API keys or connection strings to Git!

### 4. Run migrations & start
```bash
cd Harfi.API
dotnet ef migrations add InitialCreate --project ../Harfi.Repositories
dotnet ef database update --project ../Harfi.Repositories
dotnet run
```

### 5. Open Swagger UI
Navigate to: **http://localhost:5000**

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

- Passwords are hashed with **BCrypt** — never store plain text
- JWT tokens expire after **60 minutes** — use refresh tokens for long sessions
- All secrets go in `appsettings.json` locally and **Azure Key Vault** in production
- Soft delete everywhere — `IsActive = false` instead of deleting rows

---

## 👥 Team & Assignments

| Name    | Track         | Phase 1 Task                   |
|---------|---------------|--------------------------------|
| Ahmed   | Backend       | .NET Project Setup + EF Migrations |
| Ibrahim | Backend       | JWT Auth — Register/Login/Roles |
| Mazen   | Backend       | Docker + DevOps                |
| Esraa   | Frontend      | Angular Setup + RTL + i18n     |
| Habiba  | AI            | AI Agent + Semantic Kernel     |
| Hadeer  | Admin/QA      | Testing & Admin Panel          |

---

## 📞 Need Help?
Open a ClickUp task or ask in the team channel.
