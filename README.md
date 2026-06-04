<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8"/>
  <img src="https://img.shields.io/badge/EF_Core-8.0-512BD4?logo=entity-framework&logoColor=white" alt="EF Core 8"/>
  <img src="https://img.shields.io/badge/Auth-JWT_+_Identity-FF6F00?logo=json-web-tokens&logoColor=white" alt="JWT + Identity"/>
  <img src="https://img.shields.io/badge/Real--Time-SignalR-FF7F50?logo=signalr&logoColor=white" alt="SignalR"/>
  <img src="https://img.shields.io/badge/Vector_DB-Qdrant-FF4500" alt="Qdrant"/>
  <img src="https://img.shields.io/badge/LLM-Groq_(Llama_3.3)-00C853?logo=meta&logoColor=white" alt="Groq Llama"/>
  <img src="https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger&logoColor=white" alt="Swagger"/>
  <img src="https://img.shields.io/badge/Language-Arabic_%7C_C%23-239120?logo=c-sharp&logoColor=white" alt="Arabic/C#"/>
  <img src="https://img.shields.io/badge/Status-In_Development-blue" alt="Status"/>
</p>

<h1 align="center">🔨 Harfi API — حرفي</h1>

<p align="center">
  <em>Arabic-first platform connecting Egyptian customers with verified craftsmen. Built with .NET 8, powered by AI.</em>
  <br/>
  <strong>ربط العملاء المصريين بالحرفيين الموثوقين — بذكاء اصطناعي</strong>
</p>

---

## 📖 Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation & Setup](#installation--setup)
- [Environment Variables](#environment-variables)
- [Usage / Running the Project](#usage--running-the-project)
- [API Reference](#api-reference)
- [Testing](#testing)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgements](#acknowledgements)

---

## 📋 Overview

**Harfi (حرفي)** is a full-featured backend platform designed to bridge the gap between Egyptian homeowners and skilled craftsmen. It provides a trusted marketplace where customers can find, book, review, and communicate with verified professionals — from plumbers and electricians to carpenters and painters.

The platform follows a **clean N-Tier architecture** with ASP.NET Core 8 at its core, Identity + JWT for robust authentication, Entity Framework Core + SQL Server for persistence, and **SignalR** for real-time chat and notifications. What sets Harfi apart is its **AI-powered assistant** — an intelligent chat agent that uses Retrieval-Augmented Generation (RAG) to understand user problems in Arabic, offer DIY fix-it steps, and recommend the best craftsmen by performing semantic search against a Qdrant vector database.

This project is built by a team of six backend engineers as part of a structured phased delivery: auth & identity → profiles & admin controls → booking state machine → reviews & closures → real-time communications → AI orchestration agent.

---

## ✨ Key Features

- **🔐 JWT Authentication with Email & Phone Verification** — Full registration/login flow with email OTP verification (MailKit + Gmail SMTP) and phone number confirmation via 6-digit codes. Refresh token rotation for secure sessions.
- **👤 Role-Based Access Control** — Three distinct roles (`admin`, `craftsman`, `customer`) with middleware-enforced authorization per endpoint.
- **🔧 Craftsman Profiles & Admin Approval** — Craftsmen submit applications with National ID upload; admins review and approve/reject. Soft-delete and availability toggling.
- **📋 Job Booking State Machine** — Customers create jobs → craftsmen accept/reject → work in progress → completion with solution description. Arabic status flow: `مفتوح → قيد التنفيذ → مكتمل`.
- **⭐ Reviews & Ratings** — One review per completed job (1–5 stars + comment). Aggregated ratings displayed on craftsman profiles.
- **💬 Real-Time Chat** — SignalR-powered conversations between customer and craftsman per job. Typing indicators, read receipts, message persistence.
- **🔔 Push-Style Notifications** — Persisted notifications for job events (accepted, rejected, completed) and new messages, delivered via SignalR and stored for offline retrieval.
- **🤖 AI Assistant (RAG-Powered)** — Arabic conversational agent built on Groq (Llama 3.3 70B) + VoyageAI embeddings + Qdrant vector DB. Detects user intent (find a craftsman / get DIY steps), extracts service type, city, and count via LLM, then semantically searches craftsmen or job-solution history. Multi-turn follow-up for problem clarification and step-by-step fix guides.
- **🔍 Semantic Search via Qdrant** — Craftsman profiles and completed job solutions are chunked, embedded (1024-dim), and indexed in Qdrant for fast cosine-similarity retrieval. Expands to nearby governorates when local results are insufficient.
- **📧 Email Verification** — HTML email templates with 6-digit codes sent via Gmail SMTP. Rate-limiting and code expiry (10 minutes).
- **🗄️ Comprehensive SQL Schema** — 15+ tables covering users, craftsmen, jobs, conversations, messages, reviews, notifications, refresh tokens, AI interactions, RAG documents, media files, feedback, user connections, email/phone verifications.

---

## 🛠️ Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Runtime** | .NET | 8.0 |
| **Language** | C# | 12 |
| **Web Framework** | ASP.NET Core | 8.0 |
| **ORM** | Entity Framework Core | 8.0.15 |
| **Database** | SQL Server | — |
| **Authentication** | ASP.NET Core Identity + JWT Bearer | 8.0.11 |
| **API Documentation** | Swagger / Swashbuckle | 6.5.0 |
| **Real-Time** | SignalR | (built-in) |
| **Email** | MailKit | 4.16.0 |
| **Vector Database** | Qdrant | HTTP API |
| **LLM Provider** | Groq (Llama 3.3-70b) | — |
| **Embeddings** | VoyageAI (voyage-3) | — |
| **DI & HTTP Clients** | `IHttpClientFactory` + `Microsoft.Extensions.Http` | 8.0.0 |
| **CORS** | Custom policy (`HarfiCors`) | — |
| **Build** | .NET SDK / Visual Studio 2022 | — |
| **Package Manager** | NuGet | — |

---

## 🏗️ Project Structure

```
Harfi/                              # Solution root (Harfi.slnx)
├── Harfi.API/                      # 🚀 Entry point — Controllers, Middleware, Hubs, DI
│   ├── Controllers/                # HTTP endpoints (one per feature)
│   │   ├── AuthController.cs       #     Register, login, refresh, email/phone verify
│   │   ├── UsersController.cs      #     User profile CRUD
│   │   ├── CraftsmenController.cs  #     Craftsman registration, search, profiles
│   │   ├── AdminController.cs      #     Admin: approve/reject craftsmen
│   │   ├── JobsController.cs       #     Job lifecycle (create, accept, complete...)
│   │   ├── ReviewsController.cs    #     Submit reviews + AI feedback
│   │   ├── ConversationsController.cs  # Chat conversations CRUD
│   │   ├── NotificationsController.cs  # User notification management
│   │   └── AIController.cs         #     🤖 AI chat + ingestion endpoints
│   ├── Hubs/                       # SignalR real-time hubs
│   │   ├── ChatHub.cs              #     Real-time messaging hub
│   │   └── NotificationHub.cs      #     Real-time notification delivery
│   ├── Middleware/
│   │   └── GlobalExceptionMiddleware.cs  # Unified error handling → JSON
│   ├── Extensions/
│   │   ├── ServiceExtensions.cs    #     DI registration (DB, auth, Swagger, CORS)
│   │   └── RagServiceExtensions.cs #     RAG pipeline DI (Voyage, Groq, Qdrant clients)
│   ├── Program.cs                  # App bootstrap & middleware pipeline
│   └── appsettings.json            # Configuration (secrets via User Secrets)
│
├── Harfi.Services/                 # 📐 Business Logic Layer
│   ├── Interfaces/                 # Service contracts
│   │   ├── IAuthService.cs
│   │   ├── IUserService.cs
│   │   ├── ICraftsmanService.cs
│   │   ├── IAdminService.cs
│   │   ├── IJobService.cs
│   │   ├── IReviewService.cs
│   │   ├── IJobFeedbackService.cs
│   │   ├── IConversationService.cs
│   │   ├── IMessageService.cs
│   │   ├── INotificationService.cs
│   │   ├── IEmailService.cs
│   │   └── ISolutionService.cs
│   └── Implementations/           # Concrete implementations
│       ├── AuthService.cs         #     Identity + JWT token management
│       ├── EmailService.cs         #     MailKit SMTP email sender
│       ├── UserService.cs          #     Profile read/update
│       ├── CraftsmanService.cs     #     Registration, profiles, search
│       ├── AdminService.cs         #     Pending list, approve, reject
│       ├── JobService.cs           #     Job state machine + notifications
│       ├── ReviewService.cs        #     Submit + aggregate reviews
│       ├── JobFeedbackService.cs   #     AI guidance feedback
│       ├── ConversationService.cs  #     Conversation CRUD + participant check
│       ├── MessageService.cs       #     Message persistence + pagination
│       ├── NotificationService.cs  #     Create, mark read, count
│       ├── RAGService.cs           #     🤖 RAG query: ingest, search, LLM rerank
│       ├── IntentService.cs        #     LLM-based intent extraction (service, city, count)
│       ├── SolutionService.cs      #     DIY solution steps from vector DB + LLM
│       ├── ChunkingService.cs      #     Craftsman → text chunk for embedding
│       ├── EmbeddingService.cs     #     VoyageAI API wrapper
│       ├── VectorDbService.cs      #     Qdrant HTTP client (upsert, search, count)
│       ├── GroqRotatingClient.cs   #     Multi-API-key Groq client with rotation
│       └── ServiceResult.cs        #     Generic result pattern (Ok/Fail)
│
├── Harfi.Repositories/            # 💾 Data Access Layer
│   ├── Data/
│   │   ├── AppDbContext.cs         #     EF Core DbContext — all DbSets + Fluent API config
│   │   ├── AppDbContextFactory.cs  #     Design-time factory for migrations
│   │   └── DataSeeder.cs           #     Seeds default admin user
│   ├── Interfaces/                 # Repository contracts
│   │   ├── IGenericRepository.cs   #     Generic CRUD interface
│   │   ├── ICraftsmanRepository.cs
│   │   ├── IJobRepository.cs
│   │   ├── IReviewRepository.cs
│   │   ├── IJobFeedbackRepository.cs
│   │   ├── IConversationRepository.cs
│   │   ├── IMessageRepository.cs
│   │   └── INotificationRepository.cs
│   ├── Implementations/           # Concrete repository implementations
│   │   ├── GenericRepository.cs    #     Base CRUD (GetById, Find, Add, Update, Remove...)
│   │   ├── CraftsmanRepository.cs  #     Filtered search, user include, delete
│   │   ├── JobRepository.cs        #     By-customer, by-craftsman queries
│   │   ├── ReviewRepository.cs     #     By-craftsman, existence check
│   │   ├── JobFeedbackRepository.cs
│   │   ├── ConversationRepository.cs
│   │   ├── MessageRepository.cs
│   │   └── NotificationRepository.cs
│   └── Migrations/                 # ⚠️ Auto-generated — DO NOT edit
│
├── Harfi.Models/                   # 🧱 Entity Models Layer
│   ├── Entities/
│   │   ├── User.cs                #     IdentityUser<int> — base user
│   │   ├── Craftsman.cs           #     Extended craftsman profile (1:1 with User)
│   │   ├── Job.cs                 #     Service request with state
│   │   ├── Conversation.cs        #     Chat container (1:1 with Job)
│   │   ├── Message.cs             #     Individual chat message
│   │   ├── Review.cs              #     Job review (1 per job)
│   │   ├── Notification.cs        #     Persisted notification
│   │   ├── RefreshToken.cs        #     JWT refresh token
│   │   ├── AIChatMessage.cs       #     AI agent conversation history
│   │   ├── RAGDocument.cs         #     SQL reference for vectors indexed in Qdrant
│   │   ├── MediaFile.cs           #     Polymorphic file reference (Cloudinary)
│   │   ├── JobFeedback.cs         #     AI guide feedback (helpful/not)
│   │   ├── UserConnection.cs      #     SignalR connection tracking
│   │   ├── EmailVerification.cs   #     Email OTP codes
│   │   └── PhoneVerification.cs   #     Phone OTP codes
│   └── Constants/
│       ├── JobStatusConstants.cs   #     Arabic status strings
│       └── FeedbackTypes.cs       #     "ساعدني" / "محتاج حرفي"
│
├── Harfi.DTOs/                     # 📦 Data Transfer Objects
│   ├── Auth/                      # RegisterDto, LoginDto, AuthResponseDto, etc.
│   ├── User/                      # UserProfileDto, UpdateUserDto
│   ├── Craftsman/                 # CraftsmanDto, CreateCraftsmanDto, CraftsmanFilterDto
│   ├── Job/                       # CreateJobDto, JobResponseDto, UpdateJobStatusDto
│   ├── Review/                    # CreateReviewDto, ReviewResponseDto, etc.
│   ├── Chat/                      # MessageDto, ConversationDto, SendMessageDto
│   ├── AI/                        # RAGDtos (QueryRequest, Chat3Request/Response, Qdrant DTOs)
│
├── docs/                           # 📚 Project documentation
│   ├── ARCHITECTURE.md            # Architecture decision records
│   ├── AUTH_ENDPOINTS.md          # Full auth endpoint reference
│   ├── CHANGELOG.md               # Release changelog
│   ├── MIGRATION_AUDIT.md         # Migration history log
│   ├── README_old.md              # Previous README
│   └── SETUP.md                   # Step-by-step onboarding guide
│
├── .github/workflows/             # CI/CD (to be configured)
├── .gitignore                     # Visual Studio + .NET standard ignores
└── Harfi.slnx                     # Solution file (VS 2022)
```

### Dependency Flow

```
        ┌─────────────────────────────────────────────────────┐
        │  API  ───→  Services  ───→  Repositories  ───→  Models  │
        │   ↘                                        ↗            │
        │         DTOs ──────────────────────────────             │
        └─────────────────────────────────────────────────────┘
```

---

## 📋 Prerequisites

| Requirement | Version | Notes |
|------------|---------|-------|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8) | **8.0.x** | Includes runtime, ASP.NET Core, EF Core tools |
| [SQL Server](https://www.microsoft.com/sql-server) | 2019+ / LocalDB | Or use Docker image |
| [Qdrant](https://qdrant.tech/documentation/quick-start/) | Latest | Vector database (Docker: `docker run -p 6400:6333 qdrant/qdrant`) |
| [Visual Studio 2022](https://visualstudio.microsoft.com/) | 17.8+ | Or VS Code + C# Dev Kit |
| [Git](https://git-scm.com/) | Latest | — |
| [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) | Latest | `dotnet tool install --global dotnet-ef` |

---

## 🚀 Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/harfi-team/harfi-backend.git
cd harfi-backend
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Configure User Secrets (⚠️ Required)

Never commit secrets to `appsettings.json`. Use .NET User Secrets:

```bash
# Initialize
dotnet user-secrets init --project Harfi.API

# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_PC\SQLEXPRESS;Database=HarfiDB;Trusted_Connection=True;TrustServerCertificate=True;" --project Harfi.API

# Set JWT secret (minimum 32 characters)
dotnet user-secrets set "JwtSettings:SecretKey" "Harfi@SuperSecret_2024_JWT_Key!XYZ" --project Harfi.API

# Set Gmail credentials for email verification
dotnet user-secrets set "EmailSettings:SenderEmail" "your-team-email@gmail.com" --project Harfi.API
dotnet user-secrets set "EmailSettings:AppPassword" "xxxx xxxx xxxx xxxx" --project Harfi.API

# Set AI API keys (optional — needed for AI features)
dotnet user-secrets set "Voyage:ApiKey" "your-voyage-api-key" --project Harfi.API
dotnet user-secrets set "Groq:ApiKey" "your-groq-api-key" --project Harfi.API
```

> 📘 **Full Gmail App Password guide** → [docs/SETUP.md](./docs/SETUP.md#2--get-your-gmail-app-password)

### 4. Apply Database Migrations

```bash
dotnet ef database update --project Harfi.Repositories --startup-project Harfi.API
```

This creates the `HarfiDB` database with all tables (15+ tables including Identity tables).

### 5. (Optional) Start Qdrant for AI Features

```bash
docker run -d -p 6400:6333 qdrant/qdrant
```

### 6. Run the Project

```bash
dotnet run --project Harfi.API
```

The Swagger UI opens at `http://localhost:5108` (or `https://localhost:5000`).

An admin user is seeded automatically on first run:
- **Email:** `admin@harfi.com`
- **Password:** `Admin@1234`

### Docker Setup (Alternative)

```bash
# Build image
docker build -t harfi-api -f Harfi.API/Dockerfile .

# Run with SQL Server container
docker-compose up
```

> ⚠️ A `docker-compose.yml` is not yet included in the repo — create one as needed.

---

## 🔐 Environment Variables

All sensitive values are managed via **.NET User Secrets** (Development) or environment variables / Azure Key Vault (Production).

| Key | Description | Required | Default |
|-----|-------------|----------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | ✅ | — |
| `JwtSettings:SecretKey` | HMAC-SHA256 signing key (≥32 chars) | ✅ | — |
| `JwtSettings:Issuer` | JWT issuer | ❌ | `HarfiAPI` |
| `JwtSettings:Audience` | JWT audience | ❌ | `HarfiClient` |
| `JwtSettings:AccessTokenExpiryMinutes` | Access token lifetime | ❌ | `60` |
| `JwtSettings:RefreshTokenExpiryDays` | Refresh token lifetime | ❌ | `30` |
| `EmailSettings:Host` | SMTP host | ❌ | `smtp.gmail.com` |
| `EmailSettings:Port` | SMTP port | ❌ | `587` |
| `EmailSettings:SenderEmail` | Gmail address for sending | ✅ | — |
| `EmailSettings:SenderName` | Display sender name | ❌ | `Harfi Platform` |
| `EmailSettings:AppPassword` | Gmail app password | ✅ | — |
| `Voyage:ApiKey` | VoyageAI embedding API key | ❌* | — |
| `Voyage:EmbeddingModel` | Embedding model name | ❌ | `voyage-3` |
| `Groq:ApiKey` | Groq LLM API key (single) | ❌* | — |
| `Groq:ApiKeys` | Array of Groq API keys (rotation) | ❌* | — |
| `Groq:ChatModel` | LLM model name | ❌ | `llama-3.3-70b-versatile` |
| `Qdrant:BaseUrl` | Qdrant server URL | ❌ | `http://localhost:6400/` |
| `AllowedOrigins` | CORS allowed origins (array) | ❌ | `["http://localhost:4200"]` |

> *Required only for AI features (RAG, chat assistant, intent extraction).

---

## 🏃 Usage / Running the Project

### Development

```bash
dotnet run --project Harfi.API
# OR
dotnet watch run --project Harfi.API   # hot reload
```

The API is available at:
- **HTTP:** `http://localhost:5108`
- **HTTPS:** `https://localhost:5000`

### Production Build

```bash
dotnet publish -c Release -o ./publish
./publish/Harfi.API.exe
```

### Swagger UI

Navigate to `http://localhost:5108` — Swagger is served at the root path and includes JWT authorization support.

---

## 📡 API Reference

### Auth Endpoints

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| `POST` | `/api/auth/register` | ❌ | Guest | Register customer or craftsman |
| `POST` | `/api/auth/login` | ❌ | Guest | Login → JWT + refresh token |
| `POST` | `/api/auth/refresh` | ❌ | Guest | Rotate refresh token |
| `POST` | `/api/auth/logout` | ✅ | Any | Revoke refresh token |
| `POST` | `/api/auth/verify-email` | ❌ | Guest | Verify email with 6-digit code |
| `POST` | `/api/auth/resend-code` | ❌ | Guest | Resend email OTP |
| `POST` | `/api/auth/send-phone-code` | ✅ | Any | Send phone OTP |
| `POST` | `/api/auth/verify-phone` | ✅ | Any | Verify phone with 6-digit code |
| `POST` | `/api/auth/resend-phone-code` | ✅ | Any | Resend phone OTP |

#### Register Example

```json
POST /api/auth/register
{
  "name": "أحمد علي",
  "email": "ahmed@example.com",
  "password": "Customer@1234",
  "confirmPassword": "Customer@1234",
  "role": "customer",
  "phone": "01012345678"
}
```

**Response** (201 Created):
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4e5f6...",
  "expiresAt": "2026-06-04T12:00:00Z",
  "user": {
    "id": 1,
    "name": "أحمد علي",
    "email": "ahmed@example.com",
    "role": "customer",
    "phone": "01012345678",
    "profileImageUrl": null
  }
}
```

### User Endpoints

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| `GET` | `/api/users/profile/{id}` | ✅ | Any | Get user profile |
| `PUT` | `/api/users/profile/{id}` | ✅ | Any | Update user profile |

### Craftsman Endpoints

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| `POST` | `/api/craftsmen/register` | ❌ | Guest | Submit craftsman application |
| `GET` | `/api/craftsmen/{id}` | ❌ | Public | Get craftsman profile |
| `GET` | `/api/craftsmen/search` | ❌ | Public | Search/filter craftsmen |
| `PUT` | `/api/craftsmen/{id}` | ✅ | Craftsman | Update profile |

**Search Example:**
```
GET /api/craftsmen/search?serviceType=سباك&city=القاهرة&minRating=3&minExperience=2
```

### Job Endpoints

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| `POST` | `/api/jobs` | ✅ | Customer | Create a job |
| `PUT` | `/api/jobs/{id}/accept` | ✅ | Craftsman | Accept job |
| `PUT` | `/api/jobs/{id}/reject` | ✅ | Craftsman | Reject job |
| `PUT` | `/api/jobs/{id}/complete` | ✅ | Craftsman | Mark job as done |
| `GET` | `/api/jobs/customer/{id}` | ✅ | Any | List customer's jobs |
| `GET` | `/api/jobs/craftsman/{id}` | ✅ | Any | List craftsman's jobs |

### Review Endpoints

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| `POST` | `/api/reviews` | ✅ | Customer | Submit review (1–5 stars) |
| `GET` | `/api/reviews/craftsman/{id}` | ❌ | Public | Get craftsman reviews |
| `POST` | `/api/reviews/rag-feedback` | ✅ | Any | Feedback on AI guide |

### Conversation Endpoints

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| `POST` | `/api/conversations` | ✅ | Any | Create/get conversation |
| `GET` | `/api/conversations` | ✅ | Any | List user's conversations |
| `GET` | `/api/conversations/{id}` | ✅ | Any | Get conversation details |
| `GET` | `/api/conversations/{id}/messages` | ✅ | Any | Get paginated messages |
| `PUT` | `/api/conversations/{id}/read` | ✅ | Any | Mark as read |

### Notification Endpoints

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| `GET` | `/api/notifications` | ✅ | Any | List user's notifications |
| `GET` | `/api/notifications/unread-count` | ✅ | Any | Get unread count |
| `PUT` | `/api/notifications/{id}/read` | ✅ | Any | Mark single as read |
| `PUT` | `/api/notifications/read-all` | ✅ | Any | Mark all as read |

### Admin Endpoints

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| `GET` | `/api/admin/pending-craftsmen` | ✅ | Admin | List pending applications |
| `PUT` | `/api/admin/approve/{id}` | ✅ | Admin | Approve craftsman |
| `DELETE` | `/api/admin/reject/{id}` | ✅ | Admin | Reject craftsman |

### AI Endpoints

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| `GET` | `/api/AI/welcome` | ❌ | Public | Get welcome message |
| `POST` | `/api/AI/chat3` | ❌ | Public | 🤖 Multi-turn AI chat |
| `POST` | `/api/AI/ingest/craftsmen` | ✅ | Any | Ingest craftsmen → Qdrant |
| `POST` | `/api/AI/ingest/jobs` | ✅ | Any | Ingest completed jobs → Qdrant |
| `GET` | `/api/AI/vectors/count` | ✅ | Any | Get vector count |

### SignalR Hubs

| Hub | Path | Auth | Description |
|-----|------|------|-------------|
| `ChatHub` | `/hubs/chat` | ✅ JWT (query string) | Real-time messaging, typing, read receipts |
| `NotificationHub` | `/hubs/notifications` | ✅ JWT (query string) | Real-time notification delivery |

> SignalR authenticates via `?access_token=` in the query string — configured in `JwtBearerEvents`.

---

## 🧪 Testing

No test project is currently configured in this repository. Tests will be added in a future phase.

To manually verify the API after setup:

```bash
# Login as seeded admin
curl -X POST http://localhost:5108/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@harfi.com","password":"Admin@1234"}'
```

The old README (moved to `docs/README_old.md`) and [docs/AUTH_ENDPOINTS.md](./docs/AUTH_ENDPOINTS.md) contain detailed endpoint-by-endpoint test instructions.

---

## 🚢 Deployment

### Docker (Future)

The API is ready for containerization. A sample `Dockerfile` can be built from `Harfi.API`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Harfi.API -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Harfi.API.dll"]
```

### Infrastructure Notes

- **Database:** SQL Server (Azure SQL, AWS RDS, or on-prem)
- **Vector DB:** Qdrant (self-hosted or Qdrant Cloud)
- **AI APIs:** Groq + VoyageAI (HTTPS)
- **Static Files:** Cloudinary (referenced via URL in `MediaFile` entity)
- **Secrets:** Azure Key Vault recommended for production (replace User Secrets)
- **CI/CD:** `.github/workflows/` directory exists and is ready for GitHub Actions pipelines

---

## 🤝 Contributing

### Team Rules

1. **Naming conventions:** Controllers → plural noun (`UsersController`), Services → `I` prefix (`IAuthService`), DTOs → purpose suffix (`CreateJobDto`).
2. **No business logic in Controllers** — always delegate to services.
3. **All endpoints must have `[Authorize]`** unless explicitly `[AllowAnonymous]`.
4. **Always use DTOs** — never expose raw entities (prevent password hash leaks).
5. **Async all the way** — avoid `.Result` or `.Wait()`.
6. **Soft delete** — use `IsActive = false` instead of DELETE.
7. **Migration-free zone** — never run `dotnet ef migrations add`. Only one person creates and pushes migrations.

### Git Workflow

```bash
git checkout dev
git pull origin dev
git checkout -b yourname-phase
# ... work ...
git add .
git commit -m "feat: description of change"
git push origin yourname-phase
```

Create a Pull Request to the `dev` branch when ready.

---

## 📄 License

This project is developed as part of an ITI (Information Technology Institute) graduation project. All rights reserved to the Harfi Team.

> No LICENSE file is present in the repository. If you intend to open-source this project, add a license (e.g., MIT, GPL-3.0) and update this section.

---

## 🙏 Acknowledgements

- **ITI (Information Technology Institute)** — for the structured learning environment and mentorship.
- **ASP.NET Core / .NET Team** — for the incredible open-source framework.
- **Groq** — for blazing-fast LLM inference (Llama 3.3 70B).
- **VoyageAI** — for high-quality Arabic embeddings.
- **Qdrant** — for the performant vector database.
- **The Harfi Team:**
  - **Esraa** (Lead) — Project architecture, Identity + JWT auth, email verification
  - **Hadeer** — User/craftsman profiles, admin controls, search filters
  - **Habiba** — Job booking system & state machine
  - **Mazen** — Reviews system & closures
  - **Ebrahim** — Real-time communications (SignalR)
  - **Ahmed** — AI orchestration agent (RAG, intent extraction, solution finder)

---

<p align="center">
  <strong>حرفي</strong> — <em>because every home deserves a master craftsman 🛠️</em>
</p>
