# Auth Endpoints — Full Reference

> **Project:** Harfi (حرفي) — Connecting Egyptian customers with verified craftsmen
> **Stack:** ASP.NET Core 8 + Identity + JWT + SQL Server
> **Last updated:** after /me removal + full Arabic responses

---

## Summary Table

| # | Method | Endpoint | Auth | Role(s) | Purpose |
|---|--------|----------|------|---------|---------|
| 1 | POST | `/api/auth/register` | ❌ Anonymous | Guest | Register new customer/craftsman |
| 2 | POST | `/api/auth/login` | ❌ Anonymous | Guest | Login, get JWT + refresh token |
| 3 | POST | `/api/auth/refresh` | ❌ Anonymous | Guest | Rotate refresh token → new tokens |
| 4 | POST | `/api/auth/logout` | ✅ Bearer | Any | Revoke refresh token |
| 5 | GET | `/api/auth/admin-only` | ✅ Bearer | `admin` | Admin-only test endpoint |
| 6 | GET | `/api/auth/craftsman-only` | ✅ Bearer | `craftsman` | Craftsman-only test endpoint |
| 7 | POST | `/api/auth/verify-email` | ❌ Anonymous | Guest | Verify email with 6-digit code |
| 8 | POST | `/api/auth/resend-code` | ❌ Anonymous | Guest | Resend email verification code |
| 9 | POST | `/api/auth/send-phone-code` | ✅ Bearer | Any | Send phone verification code |
| 10 | POST | `/api/auth/verify-phone` | ✅ Bearer | Any | Verify phone with 6-digit code |
| 11 | POST | `/api/auth/resend-phone-code` | ✅ Bearer | Any | Resend phone verification code |

---

## Common Response Shapes

### Success Response (most endpoints)
```json
{
  "success": true,
  "message": "النص العربي هنا"
}
```

### Error Response (global middleware)
```json
{
  "status": 400,
  "message": "رسالة الخطأ بالعربية",
  "timestamp": "2026-06-03T10:00:00Z"
}
```

HTTP status codes from the global exception middleware:

| Exception | Status | When |
|-----------|--------|------|
| `InvalidOperationException` | **400** | Validation / business rule violation |
| `UnauthorizedAccessException` | **401** | Bad credentials / inactive / unverified / bad token |
| `KeyNotFoundException` | **404** | User not found |
| `ArgumentException` | **400** | Invalid arguments |
| `NotImplementedException` | **501** | Feature not available |
| Any other `Exception` | **500** | Internal server error |

---

# Endpoint 1: POST /api/auth/register

## 1. PURPOSE
**AR:** تسجيل مستخدم جديد (عميل أو حرفي) في المنصة.
**EN:** Register a new user (customer or craftsman) on the platform.

## 2. WHO CALLS IT
**Guest** — any unauthenticated user (the registration page).

## 3. WHEN IT IS CALLED
On the **registration/sign-up step**. After filling out the registration form (name, email, password, role, optional phone), the user clicks "إنشاء حساب" (Create Account).

## 4. REQUEST
### Endpoint
```
POST /api/auth/register
Content-Type: application/json
```
### Body — `RegisterDto`
```typescript
interface RegisterDto {
  /** REQUIRED. Max 100 chars */
  name: string;

  /** REQUIRED. Valid email format. Max 200 chars. */
  email: string;

  /** REQUIRED. Min 8 chars. No complexity requirements. */
  password: string;

  /** REQUIRED. Must match `password`. */
  confirmPassword: string;

  /** REQUIRED. Must be exactly "customer" or "craftsman". */
  role: "customer" | "craftsman";

  /** OPTIONAL. Valid phone format. Max 20 chars. */
  phone?: string;
}
```

### Validation Summary
| Field | Required | Type | Constraints |
|-------|----------|------|-------------|
| `name` | ✅ | `string` | 1–100 chars |
| `email` | ✅ | `string` | Valid email, max 200 |
| `password` | ✅ | `string` | Min 8 chars |
| `confirmPassword` | ✅ | `string` | Must equal `password` |
| `role` | ✅ | `string` | Regex: `^(customer\|craftsman)$` |
| `phone` | ❌ | `string` | Valid phone, max 20 |

## 5. RESPONSE (201 Created)
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4e5f6...base64...",
  "expiresAt": "2026-06-03T11:00:00Z",
  "user": {
    "id": 2,
    "name": "أحمد علي",
    "email": "ahmed@example.com",
    "role": "customer",
    "phone": "01012345678",
    "profileImageUrl": null
  }
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **400** | Email already registered → `"البريد الإلكتروني مسجل مسبقاً. جرب تسجيل الدخول."` |
| **400** | ASP.NET Identity validation failures (e.g. password too weak, duplicate email, etc.) |
| **400** | Model validation failures (e.g. invalid email format, passwords don't match, invalid role) |
| **500** | Unexpected server error |

## 7. BACKEND FLOW
1. **Controller:** `AuthController.Register(RegisterDto dto)` receives the DTO, validates `ModelState.IsValid`.
2. **Service:** `AuthService.RegisterAsync(dto)`:
   a. Checks if `email` already exists via `UserManager.FindByEmailAsync(email)` — throws `InvalidOperationException` if found.
   b. Creates a new `User` entity with `UserName = email`, `Name`, `Email`, `Role`, `Phone`, `IsActive = true`, `IsVerified = false`, `CreatedAt = UtcNow`.
   c. Calls `UserManager.CreateAsync(user, password)` — ASP.NET Identity hashes the password and saves.
   d. Generates an ASP.NET Identity email confirmation token via `UserManager.GenerateEmailConfirmationTokenAsync(user)`.
   e. Generates a random 6-digit code (`Random.Next(100000, 999999)`).
   f. Saves a new `EmailVerification` record with `UserId`, `Code`, `IdentityToken`, `ExpiresAt = UtcNow + 10 min`, `IsUsed = false`.
   g. Sends the verification code via `IEmailService.SendVerificationCodeAsync(email, name, code)` — **non-blocking** (catch + log on failure).
   h. Calls `BuildAuthResponseAsync(user)` which generates a JWT access token (60 min expiry) + a refresh token (64 random bytes, 30 day expiry) stored in the `RefreshTokens` table.
   i. Returns `AuthResponseDto` with `AccessToken`, `RefreshToken`, `ExpiresAt`, and `User` info.
3. **Repository:** `IGenericRepository<RefreshToken>.AddAsync()` and `.SaveChangesAsync()` persist the refresh token.
4. **Email:** `EmailService` uses MailKit to send an HTML email via Gmail SMTP.
5. **Response:** Controller returns `201 Created` with the DTO.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **No** (`[AllowAnonymous]`) |
| Issues access token? | **Yes** — JWT with 60 min expiry |
| Issues refresh token? | **Yes** — 64-byte base64, stored in DB, 30 day expiry |
| Invalidates existing tokens? | **No** — first registration, no existing tokens |

Tokens are issued immediately after registration so the user can start using the app without logging in again.

## 9. SIDE EFFECTS
| Effect | Detail |
|--------|--------|
| DB: User created | New row in `Users` (via Identity `AspNetUsers`) |
| DB: EmailVerification created | New row with 6-digit code + Identity token, 10 min expiry |
| DB: RefreshToken created | New row with 64-byte token, 30 day expiry |
| Email sent | HTML email with verification code to user's email (non-blocking; failure logged, doesn't crash registration) |

## 10. FRONTEND USAGE
### Angular Service Method
```typescript
// auth.service.ts
register(dto: RegisterDto): Observable<AuthResponseDto> {
  return this.http.post<AuthResponseDto>(`${this.baseUrl}/auth/register`, dto);
}
```
### Request DTO Shape (TypeScript)
```typescript
export interface RegisterDto {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  role: 'customer' | 'craftsman';
  phone?: string;
}
```
### Navigation
| Outcome | Action |
|---------|--------|
| **Success** | Save tokens to `localStorage`/`sessionStorage`. Navigate to **verify-email page** (since user is not yet verified). |
| **400** (email exists) | Show error: "البريد الإلكتروني مسجل مسبقاً. جرب تسجيل الدخول." with a link to login page. |
| **400** (validation) | Show field-level validation errors. |

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| Service interface | `Harfi.Services/Interfaces/IAuthService.cs` |
| Service implementation | `Harfi.Services/Implementations/AuthService.cs` |
| DTOs | `Harfi.DTOs/Auth/RegisterDto.cs` |
| DTOs | `Harfi.DTOs/Auth/AuthResponseDto.cs` |
| Model | `Harfi.Models/Entities/User.cs` |
| Model | `Harfi.Models/Entities/RefreshToken.cs` |
| Model | `Harfi.Models/Entities/EmailVerification.cs` |
| Repository interface | `Harfi.Repositories/Interfaces/IGenericRepository.cs` |
| Repository implementation | `Harfi.Repositories/Implementations/GenericRepository.cs` |
| Email interface | `Harfi.Services/Interfaces/IEmailService.cs` |
| Email implementation | `Harfi.Services/Implementations/EmailService.cs` |
| Middleware | `Harfi.API/Middleware/GlobalExceptionMiddleware.cs` |
| DI registration | `Harfi.API/Extensions/ServiceExtensions.cs` |
| App config | `Harfi.API/appsettings.json` |
| DB context | `Harfi.Repositories/Data/AppDbContext.cs` |
| Migration | `Harfi.Repositories/Migrations/20260525143725_MigrateToIdentity.cs` |

---

# Endpoint 2: POST /api/auth/login

## 1. PURPOSE
**AR:** تسجيل الدخول باستخدام البريد الإلكتروني وكلمة المرور. إرجاع JWT و Refresh Token.
**EN:** Login with email and password. Returns JWT access token and refresh token.

## 2. WHO CALLS IT
**Guest** — any unauthenticated user on the login page.

## 3. WHEN IT IS CALLED
When the user clicks "تسجيل الدخول" (Login) after entering email + password.

## 4. REQUEST
```
POST /api/auth/login
Content-Type: application/json
```
### Body — `LoginDto`
```typescript
interface LoginDto {
  /** REQUIRED. Valid email format. */
  email: string;

  /** REQUIRED. User's password. */
  password: string;
}
```

## 5. RESPONSE (200 OK)
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4e5f6...base64...",
  "expiresAt": "2026-06-03T11:00:00Z",
  "user": {
    "id": 2,
    "name": "أحمد علي",
    "email": "ahmed@example.com",
    "role": "customer",
    "phone": "01012345678",
    "profileImageUrl": null
  }
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **401** | Email not found or wrong password → `"البريد الإلكتروني أو كلمة المرور غير صحيحة."` |
| **401** | Account is deactivated (`IsActive = false`) → `"الحساب غير مفعّل. تواصل مع الدعم."` |
| **401** | Email not verified (`IsVerified = false`) → `"البريد الإلكتروني غير مفعّل. تحقق من بريدك الإلكتروني."` |
| **400** | Model validation failures |

## 7. BACKEND FLOW
1. **Controller:** `AuthController.Login(LoginDto dto)` validates `ModelState`.
2. **Service:** `AuthService.LoginAsync(dto)`:
   a. Looks up user by email via `UserManager.FindByEmailAsync(email.ToLower().Trim())`.
   b. Checks password via `UserManager.CheckPasswordAsync(user, password)` — if null or wrong password, throws `UnauthorizedAccessException`.
   c. Checks `user.IsActive` — if `false`, throws `UnauthorizedAccessException`.
   d. Checks `user.IsVerified` — if `false`, throws `UnauthorizedAccessException`.
   e. Calls `BuildAuthResponseAsync(user)` to generate JWT + refresh token.
3. **Response:** Controller returns `200 OK` with `AuthResponseDto`.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **No** (`[AllowAnonymous]`) |
| Issues access token? | **Yes** — new JWT with 60 min expiry |
| Issues refresh token? | **Yes** — new 64-byte refresh token stored in DB |
| Invalidates existing tokens? | **No** — old refresh tokens remain valid (but the frontend should discard them) |

## 9. SIDE EFFECTS
| Effect | Detail |
|--------|--------|
| DB: RefreshToken created | A new refresh token row is inserted |
| No emails sent | — |

## 10. FRONTEND USAGE
### Angular Service Method
```typescript
login(dto: LoginDto): Observable<AuthResponseDto> {
  return this.http.post<AuthResponseDto>(`${this.baseUrl}/auth/login`, dto);
}
```
### Navigation
| Outcome | Action |
|---------|--------|
| **Success** | Store `accessToken` and `refreshToken` in `localStorage`. Navigate to **home/dashboard**. Role-based routing: customer → customer dashboard, craftsman → craftsman dashboard, admin → admin panel. |
| **401** (wrong credentials) | Show error: "البريد الإلكتروني أو كلمة المرور غير صحيحة." |
| **401** (inactive) | Show error: "الحساب غير مفعّل. تواصل مع الدعم." |
| **401** (unverified) | Show error: "البريد الإلكتروني غير مفعّل. تحقق من بريدك الإلكتروني." + link to resend code. Navigate to verify-email page. |

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| Service interface | `Harfi.Services/Interfaces/IAuthService.cs` |
| Service implementation | `Harfi.Services/Implementations/AuthService.cs` |
| DTOs | `Harfi.DTOs/Auth/LoginDto.cs` |
| DTOs | `Harfi.DTOs/Auth/AuthResponseDto.cs` |
| Model | `Harfi.Models/Entities/User.cs` |
| Model | `Harfi.Models/Entities/RefreshToken.cs` |

---

# Endpoint 3: POST /api/auth/refresh

## 1. PURPOSE
**AR:** تجديد الـ Access Token المنتهي باستخدام Refresh Token صالح. يتم إلغاء الـ Refresh Token القديم وإصدار واحد جديد (تدوير).
**EN:** Renew an expired access token using a valid refresh token. The old refresh token is revoked and a new one is issued (rotation).

## 2. WHO CALLS IT
**Guest** — The Angular HTTP interceptor automatically when it detects a **401 response** from any API call.

## 3. WHEN IT IS CALLED
- When the `accessToken` expires (after 60 minutes)
- On **page reload** if the stored `refreshToken` is still valid — the frontend can silently get new tokens without the user seeing a login screen
- When the frontend's HTTP interceptor receives a `401` response, it calls `/refresh`, retries the original request, and if `/refresh` also fails, redirects to login

## 4. REQUEST
```
POST /api/auth/refresh
Content-Type: application/json
```
### Body — `RefreshTokenRequestDto`
```typescript
interface RefreshTokenRequestDto {
  /** REQUIRED. The refresh token string previously obtained from login/register */
  refreshToken: string;
}
```

## 5. RESPONSE (200 OK)
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...BRAND_NEW...",
  "refreshToken": "base64...BRAND_NEW...",
  "expiresAt": "2026-06-03T11:00:00Z",
  "user": {
    "id": 2,
    "name": "أحمد علي",
    "email": "ahmed@example.com",
    "role": "customer",
    "phone": "01012345678",
    "profileImageUrl": null
  }
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **401** | Refresh token not found in DB → `"رمز التحديث غير صالح أو منتهي الصلاحية."` |
| **401** | Refresh token is revoked (`IsRevoked = true`) → same message |
| **401** | Refresh token is expired (`ExpiresAt < UtcNow`) → `IsActive` returns `false` → same message |
| **401** | User not found (deleted between sessions) → `"المستخدم غير موجود."` |
| **401** | User is inactive (`IsActive = false`) → `"الحساب غير مفعّل. تواصل مع الدعم."` |
| **400** | Model validation (empty `refreshToken`) |

## 7. BACKEND FLOW — Refresh Token Rotation in Detail

### Why Rotation?
Refresh token rotation means **every time a refresh token is used, it is revoked and a new one is issued**. This limits the damage if a refresh token is stolen: the attacker can use it once, but the legitimate user's next refresh will fail (because the old token was already revoked), alerting the system to token theft.

### Step-by-step
1. **Controller:** `AuthController.Refresh(RefreshTokenRequestDto dto)` receives the `refreshToken` string.
2. **Service:** `AuthService.RefreshTokenAsync(refreshToken)`:
   a. **Find token:** Queries `_refreshTokenRepo.FirstOrDefaultAsync(rt => rt.Token == refreshToken)`.
   b. **Validate:** If `stored is null` or `!stored.IsActive` (i.e. revoked or expired), throws `UnauthorizedAccessException`.
   c. **Revoke old token:** Sets `stored.IsRevoked = true`, calls `_refreshTokenRepo.Update(stored)`.
   d. **Load user:** Finds the user by `stored.UserId` via `UserManager.FindByIdAsync(stored.UserId)`. If null, throws. If `!user.IsActive`, throws.
   e. **Issue new tokens:** Calls `BuildAuthResponseAsync(user)` which:
      - Generates a **brand-new JWT** with fresh claims
      - Calls `CreateAndSaveRefreshTokenAsync(user.Id)` to insert a **new refresh token** row
   f. **Returns** `AuthResponseDto` with new tokens.
3. **Repository:** The old token is updated (`IsRevoked = true`) and the new token is inserted in the same `SaveChangesAsync()` call.

### What Gets Stored Where

| What | Where | Detail |
|------|-------|--------|
| Old refresh token | `RefreshTokens` table | `IsRevoked` set to `true` |
| New refresh token | `RefreshTokens` table | New row: 64 random bytes (base64), `UserId`, `ExpiresAt = UtcNow + 30 days` |
| New access token | Not stored (stateless JWT) | Generated in-memory, sent in response body only |

### Database Schema for `RefreshTokens`
```sql
-- Conceptual schema
CREATE TABLE RefreshTokens (
    Id          INT PRIMARY KEY IDENTITY,
    UserId      INT NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token       NVARCHAR(500) NOT NULL UNIQUE,  -- indexed
    ExpiresAt   DATETIME2 NOT NULL,
    IsRevoked   BIT NOT NULL DEFAULT 0,
    CreatedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

### Concurrency / Token Theft Detection
If an attacker and the legitimate user both use the same refresh token, the first one succeeds and the second gets a **401** because the token is now revoked. This acts as a theft detection mechanism. The app can log this event and invalidate all of the user's sessions as a countermeasure.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **No** (`[AllowAnonymous]`) — it receives the refresh token from the body |
| Issues access token? | **Yes** — brand-new JWT |
| Issues refresh token? | **Yes** — brand-new refresh token (rotation) |
| Invalidates existing tokens? | **Yes** — the old refresh token is revoked (`IsRevoked = true`) |

## 9. SIDE EFFECTS
| Effect | Detail |
|--------|--------|
| DB: RefreshToken updated | Old token marked `IsRevoked = true` |
| DB: RefreshToken created | New token row inserted |
| No emails sent | — |

## 10. FRONTEND USAGE
### Angular Service Method
```typescript
refreshToken(refreshToken: string): Observable<AuthResponseDto> {
  return this.http.post<AuthResponseDto>(`${this.baseUrl}/auth/refresh`, { refreshToken });
}
```

### HTTP Interceptor Pattern
```typescript
// auth.interceptor.ts
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private isRefreshing = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getAccessToken();
    if (token) {
      req = this.addToken(req, token);
    }
    return next.handle(req).pipe(
      catchError(error => {
        if (error instanceof HttpErrorResponse && error.status === 401) {
          return this.handle401Error(req, next);
        }
        return throwError(() => error);
      })
    );
  }

  private handle401Error(req: HttpRequest<any>, next: HttpHandler) {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject.next(null);

      return this.authService.refreshToken(this.authService.getRefreshToken()).pipe(
        switchMap((response: AuthResponseDto) => {
          this.isRefreshing = false;
          this.authService.storeTokens(response);
          this.refreshTokenSubject.next(response.accessToken);
          return next.handle(this.addToken(req, response.accessToken));
        }),
        catchError(err => {
          this.isRefreshing = false;
          this.authService.clearTokens();
          // Redirect to login page
          this.router.navigate(['/login']);
          return throwError(() => err);
        })
      );
    } else {
      return this.refreshTokenSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap(token => next.handle(this.addToken(req, token!)))
      );
    }
  }
}
```

### Store/Restore Session on Page Reload
```typescript
// auth.service.ts
export class AuthService {
  private baseUrl = '/api/auth';

  /** Call on app initialization (APP_INITIALIZER) */
  restoreSession(): Observable<boolean> {
    const accessToken = localStorage.getItem('accessToken');
    const refreshToken = localStorage.getItem('refreshToken');

    if (accessToken && !this.isTokenExpired(accessToken)) {
      // Token still valid — decode and set user
      this.currentUser = this.decodeToken(accessToken);
      return of(true);
    }

    if (refreshToken) {
      // Access token expired or missing — try refresh
      return this.refreshToken(refreshToken).pipe(
        map(response => {
          this.storeTokens(response);
          this.currentUser = response.user;
          return true;
        }),
        catchError(() => {
          this.clearTokens();
          return of(false);
        })
      );
    }

    return of(false);
  }
}
```

### Navigation
| Outcome | Action |
|---------|--------|
| **Success** | Update stored tokens. Retry the original failed request with new access token. |
| **401** | Clear all stored tokens. Navigate to **login page**. |

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| Service interface | `Harfi.Services/Interfaces/IAuthService.cs` |
| Service implementation | `Harfi.Services/Implementations/AuthService.cs` |
| DTOs | `Harfi.DTOs/Auth/RefreshTokenRequestDto.cs` |
| DTOs | `Harfi.DTOs/Auth/AuthResponseDto.cs` |
| Model | `Harfi.Models/Entities/RefreshToken.cs` |
| Model | `Harfi.Models/Entities/User.cs` |
| DB context | `Harfi.Repositories/Data/AppDbContext.cs` |
| Migration (initial) | `Harfi.Repositories/Migrations/20260524111808_InitialCreate.cs` |
| Repository interface | `Harfi.Repositories/Interfaces/IGenericRepository.cs` |
| Repository implementation | `Harfi.Repositories/Implementations/GenericRepository.cs` |

---

# Endpoint 4: POST /api/auth/logout

## 1. PURPOSE
**AR:** تسجيل الخروج وإلغاء الـ Refresh Token الحالي.
**EN:** Logout and revoke the current refresh token.

## 2. WHO CALLS IT
**Any authenticated user** (customer, craftsman, admin) — requires a valid Bearer token.

## 3. WHEN IT IS CALLED
When the user clicks "تسجيل الخروج" (Logout) in the app.

## 4. REQUEST
```
POST /api/auth/logout
Content-Type: application/json
Authorization: Bearer <accessToken>
```
### Body — `RefreshTokenRequestDto`
```typescript
interface RefreshTokenRequestDto {
  /** REQUIRED. The refresh token to revoke. */
  refreshToken: string;
}
```

## 5. RESPONSE (200 OK)
```json
{
  "message": "تم تسجيل الخروج بنجاح"
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **401** | Invalid/missing Bearer token |
| **400** | Model validation (empty `refreshToken`) |

Note: This endpoint **never throws on the service side** — if the token is not found or already revoked, it silently succeeds (idempotent).

## 7. BACKEND FLOW
1. **Controller:** `AuthController.Logout(RefreshTokenRequestDto dto)` — requires `[Authorize]`.
2. **Service:** `AuthService.LogoutAsync(refreshToken)`:
   a. Queries for the refresh token by `Token == refreshToken`.
   b. If found and not already revoked: sets `IsRevoked = true`, calls `Update()` and `SaveChangesAsync()`.
   c. If not found or already revoked: **silent** (no throw).
3. **Response:** `200 OK` with Arabic success message.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **Yes** (`[Authorize]`) |
| Issues access token? | **No** |
| Issues refresh token? | **No** |
| Invalidates existing tokens? | **Yes** — the provided refresh token is revoked |

Note: The access token is **not blacklisted** (JWT is stateless). The frontend must discard it. It will expire naturally in 60 minutes.

## 9. SIDE EFFECTS
| Effect | Detail |
|--------|--------|
| DB: RefreshToken updated | `IsRevoked` set to `true` for the given token |
| No emails sent | — |

## 10. FRONTEND USAGE
### Angular Service Method
```typescript
logout(refreshToken: string): Observable<{ message: string }> {
  return this.http.post<{ message: string }>(`${this.baseUrl}/auth/logout`, { refreshToken });
}
```
### Navigation
| Outcome | Action |
|---------|--------|
| **Success** | Clear `accessToken` and `refreshToken` from storage. Navigate to **login page**. |
| **401** | Token already expired — still clear storage and navigate to login. |

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| Service interface | `Harfi.Services/Interfaces/IAuthService.cs` |
| Service implementation | `Harfi.Services/Implementations/AuthService.cs` |
| DTOs | `Harfi.DTOs/Auth/RefreshTokenRequestDto.cs` |
| Model | `Harfi.Models/Entities/RefreshToken.cs` |

---

# Endpoint 5: GET /api/auth/admin-only

## 1. PURPOSE
**AR:** مثال على endpoint محمي لا يمكن الوصول إليه إلا من قبل الأدمن. للاختبار والتوثيق فقط.
**EN:** Example protected endpoint accessible only by admin users. For testing and documentation only.

## 2. WHO CALLS IT
**Admin only** (`[Authorize(Roles = "admin")]`)

## 3. WHEN IT IS CALLED
Not part of the real user journey. Used for testing RBAC (role-based access control).

## 4. REQUEST
```
GET /api/auth/admin-only
Authorization: Bearer <accessToken>
```
**No request body.**

## 5. RESPONSE (200 OK)
```json
{
  "message": "أهلاً بالأدمن 👋"
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **401** | Missing/invalid/expired Bearer token |
| **403** | Authenticated but user's role is not `admin` |

## 7. BACKEND FLOW
1. **Controller:** `AuthController.AdminOnly()` — requires `[Authorize(Roles = "admin")]`.
2. ASP.NET Core's `AuthorizationMiddleware` checks the `ClaimTypes.Role` claim in the JWT.
3. If role claim is `admin`, the handler returns the success message.
4. If role claim is not present or not `admin`, a **403 Forbidden** is returned.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **Yes** |
| Issues tokens? | **No** |
| Invalidates tokens? | **No** |

## 9. SIDE EFFECTS
**None.**

## 10. FRONTEND USAGE
```typescript
adminOnly(): Observable<{ message: string }> {
  return this.http.get<{ message: string }>(`${this.baseUrl}/auth/admin-only`);
}
```

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| JWT config | `Harfi.API/Extensions/ServiceExtensions.cs` |

---

# Endpoint 6: GET /api/auth/craftsman-only

## 1. PURPOSE
**AR:** مثال على endpoint محمي لا يمكن الوصول إليه إلا من قبل الحرفي. للاختبار والتوثيق فقط.
**EN:** Example protected endpoint accessible only by craftsman users. For testing and documentation only.

## 2. WHO CALLS IT
**Craftsman only** (`[Authorize(Roles = "craftsman")]`)

## 3. WHEN IT IS CALLED
Not part of the real user journey. Used for testing RBAC.

## 4. REQUEST
```
GET /api/auth/craftsman-only
Authorization: Bearer <accessToken>
```
**No request body.**

## 5. RESPONSE (200 OK)
```json
{
  "message": "أهلاً بالحرفي 🔧"
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **401** | Missing/invalid/expired Bearer token |
| **403** | Authenticated but user's role is not `craftsman` |

## 7. BACKEND FLOW
Identical to endpoint 5 but checks for `craftsman` role.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **Yes** |
| Issues tokens? | **No** |
| Invalidates tokens? | **No** |

## 9. SIDE EFFECTS
**None.**

## 10. FRONTEND USAGE
```typescript
craftsmanOnly(): Observable<{ message: string }> {
  return this.http.get<{ message: string }>(`${this.baseUrl}/auth/craftsman-only`);
}
```

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| JWT config | `Harfi.API/Extensions/ServiceExtensions.cs` |

---

# Endpoint 7: POST /api/auth/verify-email

## 1. PURPOSE
**AR:** تفعيل البريد الإلكتروني عن طريق إدخال الكود المكون من 6 أرقام الذي تم إرساله إلى البريد الإلكتروني.
**EN:** Verify the user's email by entering the 6-digit code sent to their email.

## 2. WHO CALLS IT
**Guest** — any unauthenticated user who has registered but not yet verified their email.

## 3. WHEN IT IS CALLED
- On the **verify-email page** after registration
- If the user was redirected from login because `IsVerified = false`

## 4. REQUEST
```
POST /api/auth/verify-email
Content-Type: application/json
```
### Body — `VerifyEmailDto`
```typescript
interface VerifyEmailDto {
  /** REQUIRED. The email address that was registered. */
  email: string;

  /** REQUIRED. Exactly 6 digits. */
  code: string;
}
```

## 5. RESPONSE (200 OK)
```json
{
  "success": true,
  "message": "تم تفعيل البريد الإلكتروني بنجاح."
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **404** | User not found by email → `"المستخدم غير موجود."` |
| **400** | Email already verified → `"البريد الإلكتروني مفعّل مسبقاً."` |
| **400** | Code incorrect or expired → `"الكود غير صحيح أو منتهي الصلاحية."` |
| **400** | ASP.NET Identity `ConfirmEmailAsync` fails → `"فشل تأكيد البريد الإلكتروني: {errors}"` |

## 7. BACKEND FLOW
1. **Controller:** `AuthController.VerifyEmail(VerifyEmailDto dto)` — `[AllowAnonymous]`.
2. **Service:** `AuthService.VerifyEmailAsync(dto)`:
   a. Finds user by email via `UserManager.FindByEmailAsync(email)` — throws `KeyNotFoundException` if null.
   b. Checks `user.IsVerified` — throws `InvalidOperationException` if already verified.
   c. Finds `EmailVerification` record: `UserId == user.Id && Code == dto.Code && !IsUsed && ExpiresAt > UtcNow`.
   d. If not found, throws `InvalidOperationException` ("الكود غير صحيح أو منتهي الصلاحية").
   e. Sets `verification.IsUsed = true`, updates in repo.
   f. Calls `UserManager.ConfirmEmailAsync(user, verification.IdentityToken)` — this validates the Identity token and flips `EmailConfirmed` to `true`.
   g. Sets `user.IsVerified = true` and calls `UserManager.UpdateAsync(user)`. (Both operational and `EmailConfirmed` flags are set.)
   h. Returns success message.
3. **Response:** `200 OK` with `{ success: true, message }`.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **No** (`[AllowAnonymous]`) |
| Issues tokens? | **No** |
| Invalidates tokens? | **No** |

## 9. SIDE EFFECTS
| Effect | Detail |
|--------|--------|
| DB: EmailVerification updated | `IsUsed` set to `true` |
| DB: User updated | `IsVerified` set to `true`, `EmailConfirmed` set to `true` (via Identity) |
| No emails sent | — |

## 10. FRONTEND USAGE
### Angular Service Method
```typescript
verifyEmail(dto: VerifyEmailDto): Observable<{ success: boolean; message: string }> {
  return this.http.post<{ success: boolean; message: string }>(
    `${this.baseUrl}/auth/verify-email`, dto
  );
}
```
### Navigation
| Outcome | Action |
|---------|--------|
| **Success** | Show success toast. Navigate to **login page** (user can now log in). |
| **400** (wrong code) | Show error: "الكود غير صحيح أو منتهي الصلاحية." Allow user to resend code. |
| **400** (already verified) | Navigate to login. |

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| Service interface | `Harfi.Services/Interfaces/IAuthService.cs` |
| Service implementation | `Harfi.Services/Implementations/AuthService.cs` |
| DTOs | `Harfi.DTOs/Auth/VerifyEmailDto.cs` |
| Model | `Harfi.Models/Entities/EmailVerification.cs` |
| Model | `Harfi.Models/Entities/User.cs` |
| Migration | `Harfi.Repositories/Migrations/20260524134133_AddEmailVerification.cs` |
| Migration | `Harfi.Repositories/Migrations/20260601163311_AddIdentityTokenToEmailVerification.cs` |

---

# Endpoint 8: POST /api/auth/resend-code

## 1. PURPOSE
**AR:** إعادة إرسال كود التفعيل إلى البريد الإلكتروني. يتم إلغاء جميع الأكواد القديمة غير المستخدمة وإنشاء كود جديد.
**EN:** Resend the email verification code. Invalidates all old unused codes and generates a new one.

## 2. WHO CALLS IT
**Guest** — user on the verify-email page who needs a new code.

## 3. WHEN IT IS CALLED
- When the user clicks "إعادة إرسال الكود" (Resend Code)
- Automatically after the code expires (10 minutes)

## 4. REQUEST
```
POST /api/auth/resend-code
Content-Type: application/json
```
### Body — `ResendCodeDto`
```typescript
interface ResendCodeDto {
  /** REQUIRED. The email address to resend the code to. */
  email: string;
}
```

## 5. RESPONSE (200 OK)
```json
{
  "success": true,
  "message": "تم إعادة إرسال الكود بنجاح."
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **404** | User not found → `"المستخدم غير موجود."` |
| **400** | Email already verified → `"البريد الإلكتروني مفعّل مسبقاً."` |
| **500** | Email service failure (caught and logged, but still throws up) |

## 7. BACKEND FLOW
1. **Controller:** `AuthController.ResendCode(ResendCodeDto dto)` — `[AllowAnonymous]`.
2. **Service:** `AuthService.ResendVerificationCodeAsync(dto)`:
   a. Finds user by email — throws `KeyNotFoundException` if null.
   b. Checks `user.IsVerified` — throws if already verified.
   c. Invalidates all old unused codes for this user: sets `IsUsed = true` on each.
   d. Generates a new Identity email confirmation token via `UserManager.GenerateEmailConfirmationTokenAsync(user)`.
   e. Generates a new random 6-digit code.
   f. Creates a new `EmailVerification` record with the new `Code`, `IdentityToken`, `ExpiresAt = UtcNow + 10 min`.
   g. Saves and sends the email.
   h. Returns success message.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **No** (`[AllowAnonymous]`) |
| Issues tokens? | **No** |

## 9. SIDE EFFECTS
| Effect | Detail |
|--------|--------|
| DB: Old EmailVerifications updated | All unused codes for user marked `IsUsed = true` |
| DB: New EmailVerification created | New code with 10 min expiry |
| Email sent | New verification code to user's email |

## 10. FRONTEND USAGE
```typescript
resendCode(dto: ResendCodeDto): Observable<{ success: boolean; message: string }> {
  return this.http.post<{ success: boolean; message: string }>(
    `${this.baseUrl}/auth/resend-code`, dto
  );
}
```

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| Service interface | `Harfi.Services/Interfaces/IAuthService.cs` |
| Service implementation | `Harfi.Services/Implementations/AuthService.cs` |
| DTOs | `Harfi.DTOs/Auth/ResendCodeDto.cs` |
| Model | `Harfi.Models/Entities/EmailVerification.cs` |
| Email interface | `Harfi.Services/Interfaces/IEmailService.cs` |
| Email impl | `Harfi.Services/Implementations/EmailService.cs` |

---

# Endpoint 9: POST /api/auth/send-phone-code

## 1. PURPOSE
**AR:** إرسال كود تفعيل مكون من 6 أرقام إلى البريد الإلكتروني لتأكيد رقم الهاتف (SMS gateway غير مطبق بعد).
**EN:** Send a 6-digit phone verification code to the user's email (SMS gateway not yet implemented — currently sends via email as a fallback).

## 2. WHO CALLS IT
**Any authenticated user** — requires Bearer token.

## 3. WHEN IT IS CALLED
On the **verify-phone page** where the user enters their phone number and requests a verification code.

## 4. REQUEST
```
POST /api/auth/send-phone-code
Content-Type: application/json
Authorization: Bearer <accessToken>
```
### Body — `SendPhoneVerificationDto`
```typescript
interface SendPhoneVerificationDto {
  /** REQUIRED. Valid email. */
  email: string;

  /** REQUIRED. Phone number to verify. Max 20 chars. */
  phoneNumber: string;
}
```

## 5. RESPONSE (200 OK)
```json
{
  "success": true,
  "message": "تم إرسال الكود بنجاح."
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **401** | Invalid/missing Bearer token |
| **404** | User not found → `"المستخدم غير موجود."` |
| **400** | Phone already confirmed → `"رقم الهاتف مفعّل مسبقاً."` |

## 7. BACKEND FLOW
1. **Controller:** `AuthController.SendPhoneCode(SendPhoneVerificationDto dto)` — `[Authorize]`.
2. **Service:** `AuthService.SendPhoneVerificationCodeAsync(dto)`:
   a. Finds user by email — throws `KeyNotFoundException`.
   b. Checks `user.PhoneNumberConfirmed` — throws `InvalidOperationException` if already confirmed.
   c. Generates an ASP.NET Identity phone change token via `UserManager.GenerateChangePhoneNumberTokenAsync(user, dto.PhoneNumber)`.
   d. Generates a random 6-digit code.
   e. Creates a new `PhoneVerification` record with `UserId`, `PhoneNumber`, `Code`, `IdentityToken`, `ExpiresAt = UtcNow + 10 min`.
   f. **Temporary:** Sends the code via email (with emoji prefix) instead of SMS. Marked as `TODO: Replace with actual SMS gateway`.
   g. Returns success message.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **Yes** (`[Authorize]`) |
| Issues tokens? | **No** |

## 9. SIDE EFFECTS
| Effect | Detail |
|--------|--------|
| DB: PhoneVerification created | New record with code + Identity token |
| Email sent | Verification code sent to user's email (SMS not yet implemented) |

## 10. FRONTEND USAGE
```typescript
sendPhoneCode(dto: SendPhoneVerificationDto): Observable<{ success: boolean; message: string }> {
  return this.http.post<{ success: boolean; message: string }>(
    `${this.baseUrl}/auth/send-phone-code`, dto
  );
}
```

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| Service interface | `Harfi.Services/Interfaces/IAuthService.cs` |
| Service implementation | `Harfi.Services/Implementations/AuthService.cs` |
| DTOs | `Harfi.DTOs/Auth/SendPhoneVerificationDto.cs` |
| Model | `Harfi.Models/Entities/PhoneVerification.cs` |
| Model | `Harfi.Models/Entities/User.cs` |
| Migration | `Harfi.Repositories/Migrations/20260601170949_AddPhoneVerification.cs` |

---

# Endpoint 10: POST /api/auth/verify-phone

## 1. PURPOSE
**AR:** تأكيد رقم الهاتف عن طريق إدخال الكود المكون من 6 أرقام الذي تم إرساله إلى البريد الإلكتروني.
**EN:** Verify the phone number by entering the 6-digit code sent via email.

## 2. WHO CALLS IT
**Any authenticated user** — requires Bearer token.

## 3. WHEN IT IS CALLED
On the **verify-phone page** after the user has received the code.

## 4. REQUEST
```
POST /api/auth/verify-phone
Content-Type: application/json
Authorization: Bearer <accessToken>
```
### Body — `VerifyPhoneDto`
```typescript
interface VerifyPhoneDto {
  /** REQUIRED. Valid email. */
  email: string;

  /** REQUIRED. Phone number being verified. Max 20 chars. */
  phoneNumber: string;

  /** REQUIRED. Exactly 6 digits. */
  code: string;
}
```

## 5. RESPONSE (200 OK)
```json
{
  "success": true,
  "message": "تم تفعيل رقم الهاتف بنجاح."
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **401** | Invalid/missing Bearer token |
| **404** | User not found → `"المستخدم غير موجود."` |
| **400** | Phone already confirmed → `"رقم الهاتف مفعّل مسبقاً."` |
| **400** | Code incorrect or expired → `"الكود غير صحيح أو منتهي الصلاحية."` |
| **400** | Identity `ChangePhoneNumberAsync` fails → `"فشل تأكيد رقم الهاتف: {errors}"` |

## 7. BACKEND FLOW
1. **Controller:** `AuthController.VerifyPhone(VerifyPhoneDto dto)` — `[Authorize]`.
2. **Service:** `AuthService.VerifyPhoneAsync(dto)`:
   a. Finds user by email — throws `KeyNotFoundException` if null.
   b. Checks `user.PhoneNumberConfirmed` — throws if already confirmed.
   c. Finds `PhoneVerification` record: `UserId == user.Id && PhoneNumber == dto.PhoneNumber && Code == dto.Code && !IsUsed && ExpiresAt > UtcNow`.
   d. If not found, throws `InvalidOperationException`.
   e. Sets `verification.IsUsed = true`, updates in repo.
   f. Calls `UserManager.ChangePhoneNumberAsync(user, verification.PhoneNumber, verification.IdentityToken)` — validates the token and flips `PhoneNumberConfirmed` to `true`.
   g. Syncs `user.Phone = verification.PhoneNumber` and calls `UserManager.UpdateAsync(user)`.
   h. Returns success message.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **Yes** (`[Authorize]`) |
| Issues tokens? | **No** |

## 9. SIDE EFFECTS
| Effect | Detail |
|--------|--------|
| DB: PhoneVerification updated | `IsUsed` set to `true` |
| DB: User updated | `Phone` field synced with verified number, `PhoneNumberConfirmed` set to `true` (via Identity) |
| No emails sent | — |

## 10. FRONTEND USAGE
```typescript
verifyPhone(dto: VerifyPhoneDto): Observable<{ success: boolean; message: string }> {
  return this.http.post<{ success: boolean; message: string }>(
    `${this.baseUrl}/auth/verify-phone`, dto
  );
}
```

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| Service interface | `Harfi.Services/Interfaces/IAuthService.cs` |
| Service implementation | `Harfi.Services/Implementations/AuthService.cs` |
| DTOs | `Harfi.DTOs/Auth/VerifyPhoneDto.cs` |
| Model | `Harfi.Models/Entities/PhoneVerification.cs` |
| Model | `Harfi.Models/Entities/User.cs` |

---

# Endpoint 11: POST /api/auth/resend-phone-code

## 1. PURPOSE
**AR:** إعادة إرسال كود تفعيل رقم الهاتف. يتم إلغاء جميع الأكواد القديمة غير المستخدمة لنفس المستخدم ورقم الهاتف.
**EN:** Resend the phone verification code. Invalidates all old unused codes for this user + phone number.

## 2. WHO CALLS IT
**Any authenticated user** — requires Bearer token.

## 3. WHEN IT IS CALLED
- When the user clicks "إعادة إرسال الكود" on the verify-phone page
- After the phone verification code expires

## 4. REQUEST
```
POST /api/auth/resend-phone-code
Content-Type: application/json
Authorization: Bearer <accessToken>
```
### Body — `ResendPhoneCodeDto`
```typescript
interface ResendPhoneCodeDto {
  /** REQUIRED. Valid email. */
  email: string;

  /** REQUIRED. Phone number. Max 20 chars. */
  phoneNumber: string;
}
```

## 5. RESPONSE (200 OK)
```json
{
  "success": true,
  "message": "تم إعادة إرسال الكود بنجاح."
}
```

## 6. ERROR RESPONSES
| Status | Condition |
|--------|-----------|
| **401** | Invalid/missing Bearer token |
| **404** | User not found → `"المستخدم غير موجود."` |
| **400** | Phone already confirmed → `"رقم الهاتف مفعّل مسبقاً."` |

## 7. BACKEND FLOW
1. **Controller:** `AuthController.ResendPhoneCode(ResendPhoneCodeDto dto)` — `[Authorize]`.
2. **Service:** `AuthService.ResendPhoneVerificationCodeAsync(dto)`:
   a. Finds user by email — throws `KeyNotFoundException`.
   b. Checks `user.PhoneNumberConfirmed` — throws if already confirmed.
   c. Invalidates all old unused codes: queries for `UserId == user.Id && PhoneNumber == dto.PhoneNumber && !IsUsed`, marks them all `IsUsed = true`.
   d. Generates new Identity phone change token and 6-digit code.
   e. Creates new `PhoneVerification` record.
   f. Saves and sends the code via email.
   g. Returns success message.

## 8. TOKENS
| Question | Answer |
|----------|--------|
| Requires Bearer token? | **Yes** (`[Authorize]`) |
| Issues tokens? | **No** |

## 9. SIDE EFFECTS
| Effect | Detail |
|--------|--------|
| DB: Old PhoneVerifications updated | All unused codes for user+phone marked `IsUsed = true` |
| DB: New PhoneVerification created | New code with 10 min expiry |
| Email sent | New verification code via email |

## 10. FRONTEND USAGE
```typescript
resendPhoneCode(dto: ResendPhoneCodeDto): Observable<{ success: boolean; message: string }> {
  return this.http.post<{ success: boolean; message: string }>(
    `${this.baseUrl}/auth/resend-phone-code`, dto
  );
}
```

## 11. RELATED FILES
| Category | File |
|----------|------|
| Controller | `Harfi.API/Controllers/AuthController.cs` |
| Service interface | `Harfi.Services/Interfaces/IAuthService.cs` |
| Service implementation | `Harfi.Services/Implementations/AuthService.cs` |
| DTOs | `Harfi.DTOs/Auth/ResendPhoneCodeDto.cs` |
| Model | `Harfi.Models/Entities/PhoneVerification.cs` |

---

## Complete File Index

### Controllers
| File | Lines |
|------|-------|
| `Harfi.API/Controllers/AuthController.cs` | 150 |

### Services
| File | Lines |
|------|-------|
| `Harfi.Services/Interfaces/IAuthService.cs` | 41 |
| `Harfi.Services/Implementations/AuthService.cs` | 460 |
| `Harfi.Services/Interfaces/IEmailService.cs` | 12 |
| `Harfi.Services/Implementations/EmailService.cs` | 66 |

### Repositories
| File | Lines |
|------|-------|
| `Harfi.Repositories/Interfaces/IGenericRepository.cs` | 29 |
| `Harfi.Repositories/Implementations/GenericRepository.cs` | — |
| `Harfi.Repositories/Data/AppDbContext.cs` | 246 |
| `Harfi.Repositories/Data/DataSeeder.cs` | 37 |

### Models
| File | Lines |
|------|-------|
| `Harfi.Models/Entities/User.cs` | 40 |
| `Harfi.Models/Entities/RefreshToken.cs` | 36 |
| `Harfi.Models/Entities/EmailVerification.cs` | 18 |
| `Harfi.Models/Entities/PhoneVerification.cs` | 19 |

### DTOs
| File | Lines |
|------|-------|
| `Harfi.DTOs/Auth/RegisterDto.cs` | 34 |
| `Harfi.DTOs/Auth/LoginDto.cs` | 13 |
| `Harfi.DTOs/Auth/AuthResponseDto.cs` | 23 |
| `Harfi.DTOs/Auth/RefreshTokenRequestDto.cs` | 13 |
| `Harfi.DTOs/Auth/VerifyEmailDto.cs` | 14 |
| `Harfi.DTOs/Auth/ResendCodeDto.cs` | 10 |
| `Harfi.DTOs/Auth/SendPhoneVerificationDto.cs` | 15 |
| `Harfi.DTOs/Auth/VerifyPhoneDto.cs` | 19 |
| `Harfi.DTOs/Auth/ResendPhoneCodeDto.cs` | 15 |

### Infrastructure
| File | Lines |
|------|-------|
| `Harfi.API/Program.cs` | 78 |
| `Harfi.API/Extensions/ServiceExtensions.cs` | 222 |
| `Harfi.API/Middleware/GlobalExceptionMiddleware.cs` | 81 |
| `Harfi.API/appsettings.json` | 49 |

### Migrations (auth-related)
| File | Description |
|------|-------------|
| `Harfi.Repositories/Migrations/20260524111808_InitialCreate.cs` | Creates Users + RefreshTokens tables |
| `Harfi.Repositories/Migrations/20260524134133_AddEmailVerification.cs` | Adds `IsVerified` to Users, creates EmailVerifications |
| `Harfi.Repositories/Migrations/20260525143725_MigrateToIdentity.cs` | Migrates Users to ASP.NET Core Identity |
| `Harfi.Repositories/Migrations/20260601163311_AddIdentityTokenToEmailVerification.cs` | Adds `IdentityToken` column |
| `Harfi.Repositories/Migrations/20260601170949_AddPhoneVerification.cs` | Creates PhoneVerifications table |
