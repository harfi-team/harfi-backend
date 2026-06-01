# Changelog — Harfi Backend

## [Unreleased] — Migrated to ASP.NET Core Identity

### Overview

Migrated the entire custom authentication system to **ASP.NET Core Identity** with
JWT Bearer tokens. All existing API routes, DTO shapes, and response contracts are
preserved — no breaking changes to frontend clients.

---

### Files Modified

#### `Harfi.Models/Entities/User.cs`

| What | Before | After |
|---|---|---|
| Base class | Plain `class User` (POCO) | `User : IdentityUser<int>` |
| `Id` | Explicit `[Key] int Id` | Inherited from `IdentityUser<int>` |
| `Email` | Custom `string Email` with `[Required][MaxLength(200)][EmailAddress]` | Inherited `string? Email` (nullable in Identity) |
| `PasswordHash` | Custom `string PasswordHash` with `[Required][MaxLength(500)]` | Inherited from `IdentityUser<int>` |
| `UserName` | Did not exist | Added: mapped to email on registration |
| New Identity columns | — | Inherits `SecurityStamp`, `ConcurrencyStamp`, `EmailConfirmed`, `PhoneNumber`, `LockoutEnd`, `AccessFailedCount`, etc. |
| Custom fields kept | `Name`, `Role`, `Phone`, `IsActive`, `IsVerified`, `ProfileImageUrl`, `CreatedAt` | All kept unchanged |
| Navigation properties | 8 nav properties | All kept unchanged |

**Why:** `IdentityUser<int>` provides the base for Identity's UserManager, password
hashing, security stamps, lockout, and all other Identity features. Using `int` as
the key type preserves the existing `int Id` schema.

---

#### `Harfi.Models/Harfi.Models.csproj`

- **Added:** `Microsoft.Extensions.Identity.Stores` 8.0.11
- **Why:** Provides the `IdentityUser<TKey>` base class that `User` now extends.

---

#### `Harfi.Repositories/Data/AppDbContext.cs`

| What | Before | After |
|---|---|---|
| Base class | `DbContext` | `IdentityUserContext<User, int>` |
| `DbSet<User> Users` | Explicit declaration | Removed (provided by `IdentityUserContext`) |
| `e.ToTable("Users")` | Not present | Added to prevent Identity from renaming to `AspNetUsers` |
| All other `DbSet<>` | 13 custom DbSets | All kept unchanged |
| All Fluent API config | 12 entity configurations | All kept unchanged |
| `base.OnModelCreating()` | Called (empty base) | Still called — Identity configures its tables here |

**Why:** `IdentityUserContext` sets up the Identity store schema (Users,
UserClaims, UserLogins, UserTokens) without adding role tables. Roles are
managed via the custom `Role` column on User. `e.ToTable("Users")` preserves
the existing table name.

---

#### `Harfi.Repositories/Harfi.Repositories.csproj`

- **Added:** `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.11
- **Removed:** `BCrypt.Net-Next` (no longer used anywhere in the project)
- **Why:** The new package provides `IdentityUserContext<TUser, TKey>`. BCrypt was
  removed because password hashing is now handled by `UserManager`.

---

#### `Harfi.Repositories/Data/DataSeeder.cs`

| What | Before | After |
|---|---|---|
| Method signature | `SeedAsync(AppDbContext context)` | `SeedAsync(UserManager<User> userManager)` |
| User creation | `new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword(...) }` then `context.Users.AddAsync(admin)` | `new User { UserName = email, ... }` then `userManager.CreateAsync(admin, "Admin@1234")` |
| Email uniqueness | Checked via `context.Users.AnyAsync(u => u.Role == "admin")` | `userManager.FindByEmailAsync("admin@harfi.com")` |
| Error handling | None | Checks `result.Succeeded` and logs errors |

**Why:** The admin must be created through `UserManager.CreateAsync()` so
Identity's password hasher stores the password in a format it can later verify.
Direct BCrypt hashing would make the admin password unverifiable after migration.

---

#### `Harfi.Repositories/Migrations/20260525143725_MigrateToIdentity.cs`

- **New migration** that:
  - Alters `PasswordHash` from `nvarchar(500)` → `nvarchar(max)` (nullable)
  - Alters `Email` from `nvarchar(200)` → `nvarchar(256)` (nullable)
  - Adds Identity columns: `AccessFailedCount`, `ConcurrencyStamp`,
    `EmailConfirmed`, `LockoutEnabled`, `LockoutEnd`, `NormalizedEmail`,
    `NormalizedUserName`, `PhoneNumber`, `PhoneNumberConfirmed`,
    `SecurityStamp`, `TwoFactorEnabled`, `UserName`
  - Creates new tables: `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`
  - Creates indexes: `EmailIndex` (NormalizedEmail), `UserNameIndex`
    (NormalizedUserName), `IX_Users_Email` (unique filtered)

---

#### `Harfi.Services/Harfi.Services.csproj`

- **Removed:** `BCrypt.Net-Next` (no longer referenced in Services layer)
- **Why:** `AuthService` now uses `UserManager` instead of BCrypt directly.

---

#### `Harfi.Services/Implementations/AuthService.cs`

| Concern | Before | After |
|---|---|---|
| Password hashing | `BCrypt.Net.BCrypt.HashPassword()` / `BCrypt.Net.BCrypt.Verify()` | `UserManager.CreateAsync(user, password)` / `UserManager.CheckPasswordAsync(user, password)` |
| User lookup | `_userRepo.ExistsAsync()` / `_userRepo.FirstOrDefaultAsync()` / `_userRepo.GetByIdAsync()` | `UserManager.FindByEmailAsync()` / `UserManager.FindByIdAsync()` |
| User updates | `_userRepo.Update(user)` + `_userRepo.SaveChangesAsync()` | `UserManager.UpdateAsync(user)` |
| User creation | Manual `PasswordHash` set + `_userRepo.AddAsync()` | `UserManager.CreateAsync(user, password)` — sets `PasswordHash` and `SecurityStamp` automatically |
| `UserName` | Not set | Now set to email on registration |
| Constructor dependency | `IGenericRepository<User> _userRepo` | `UserManager<User> _userManager` |
| Email null-safety | — | `user.Email!` null-forgiving operator + guard in `GenerateJwtToken` |
| All other logic | JWT generation, refresh rotation, email verification, resend code | **Identical** — same claims, same structure, same DTO shapes |

**Why:** `UserManager` is Identity's primary API for user operations. It handles
password hashing (PBKDF2 with salt), security stamp management, and validation.
The manual BCrypt calls were replaced while keeping the custom JWT generation
and refresh token rotation logic fully intact.

---

#### `Harfi.API/Extensions/ServiceExtensions.cs`

| What | Before | After |
|---|---|---|
| Identity registration | Not present | `AddIdentityCore<User>()` with password options, `AddEntityFrameworkStores<AppDbContext>()`, `AddDefaultTokenProviders()`, `AddSignInManager<SignInManager<User>>()` |
| JWT Bearer config | Same JWT validation parameters | **Unchanged** — same `SecretKey`, `Issuer`, `Audience`, `ClockSkew` |
| SignalR support | `OnMessageReceived` for query string tokens | **Unchanged** |
| Swagger JWT config | Same security definition | **Unchanged** |
| CORS config | Same origins | **Unchanged** |
| Cookie auth | — | **Not added** — `AddIdentityCore` skips cookie middleware |

**Why:** `AddIdentityCore` registers only the services needed for JWT-based APIs
(UserManager, SignInManager, stores) without adding cookie authentication
middleware. Password options are relaxed (8-char minimum, no complexity rules)
to match the existing registration behavior. `RequireConfirmedEmail = false`
because email verification is handled by the custom `EmailVerification` flow.

---

#### `Harfi.API/Program.cs`

| What | Before | After |
|---|---|---|
| Usings | `Harfi.API.Extensions`, `Harfi.API.Middleware`, `Harfi.Repositories.Data`, `Microsoft.EntityFrameworkCore` | Added `Harfi.Models.Entities`, `Microsoft.AspNetCore.Identity` |
| Seeder call | `DataSeeder.SeedAsync(db)` with `AppDbContext` | `DataSeeder.SeedAsync(userManager)` with `UserManager<User>` |

**Why:** The seeder now needs `UserManager` instead of the raw `DbContext`.

---

#### `Harfi.API/Properties/launchSettings.json`

- Changed `launchUrl` from `"swagger"` → `""` (empty string)
- Changed `https` profile URL from `https://localhost:7222;http://localhost:5108` → `https://localhost:5000`
- **Why:** Swagger UI is configured at the root path (`RoutePrefix = string.Empty`),
  so the browser should open at `/` not `/swagger`. Port 5000 is a single
  predictable dev URL.

---

### NuGet Packages Summary

| Package | Action | Project |
|---|---|---|
| `Microsoft.Extensions.Identity.Stores` 8.0.11 | **Added** | `Harfi.Models` |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.11 | **Added** | `Harfi.Repositories` |
| `BCrypt.Net-Next` | **Removed** | `Harfi.Services`, `Harfi.Repositories` |

---

### Files Unchanged (verified, no changes needed)

| File | Reason |
|---|---|
| `Harfi.Services/Interfaces/IAuthService.cs` | Abstract interface — no implementation details |
| `Harfi.API/Controllers/AuthController.cs` | Only depends on `IAuthService`, DTOs, `[Authorize]` |
| `Harfi.API/Controllers/JobsController.cs` | No auth-related changes needed |
| `Harfi.API/Middleware/GlobalExceptionMiddleware.cs` | Untouched per migration rules |
| All DTOs (`Harfi.DTOs/Auth/*.cs`) | All response/request shapes preserved |
| `appsettings.json` | `JwtSettings` values unchanged |

---

### Route & Response Contract Preservation

All 15 API routes remain identical:
- Same HTTP methods and paths
- Same `[Authorize]` / `[AllowAnonymous]` / `[Authorize(Roles = "...")]` attributes
- Same JSON response shapes (field names, nesting, types)
- Same error response format via `GlobalExceptionMiddleware`

---

### Smoke Test Results

All endpoints verified against running API:

| Endpoint | Status |
|---|---|
| `POST /api/auth/register` | ✅ 201 Created |
| `POST /api/auth/verify-email` | ✅ 200 OK |
| `POST /api/auth/login` | ✅ 200 OK |
| `GET /api/auth/me` | ✅ 200 OK |
| `POST /api/auth/refresh` | ✅ 200 OK |
| `POST /api/auth/logout` | ✅ 200 OK |
| `GET /api/auth/admin-only` | ✅ 200 OK (admin role verified) |
| Admin seed (`admin@harfi.com / Admin@1234`) | ✅ Auto-seeded on startup |

---

## [Security] Move Sensitive Config to .NET User Secrets

### Why
Sensitive credentials were stored in plain text inside `appsettings.json`,
which is tracked by Git. This is a security risk.

### What Changed

**`Harfi.API/appsettings.json`**
- `ConnectionStrings:DefaultConnection` → replaced with `"SET_VIA_USER_SECRETS"`
- `JwtSettings:SecretKey` → replaced with `"SET_VIA_USER_SECRETS"`
- `EmailSettings:SenderEmail` → replaced with `"SET_VIA_USER_SECRETS"`
- `EmailSettings:AppPassword` → replaced with `"SET_VIA_USER_SECRETS"`

**`.gitignore`**
- Added `**/secrets.json` to prevent User Secrets from being committed

### How to Set Up Locally (for teammates)
Run these commands once after cloning the project:

```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your_local_connection_string>" --project Harfi.API
dotnet user-secrets set "JwtSettings:SecretKey" "<secret_key>" --project Harfi.API
dotnet user-secrets set "EmailSettings:SenderEmail" "<your_email>" --project Harfi.API
dotnet user-secrets set "EmailSettings:AppPassword" "<your_app_password>" --project Harfi.API
```

### Important Notes
- User Secrets work in **Development only**
- In Production: use Environment Variables or Azure Key Vault
- No code changes needed — `builder.Configuration` reads secrets automatically
