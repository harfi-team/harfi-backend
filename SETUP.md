# 🔨 Harfi Backend — Setup Guide
> **Read every step carefully. Do not skip anything.**  
> Estimated time: 5–10 minutes (Prerequisites, cloning, and Visual Studio setup are already done on all team devices).

---

## 📋 Table of Contents
1. [Configure appsettings.json](#1--configure-appsettingsjson)
2. [Get Your Gmail App Password](#2--get-your-gmail-app-password)
3. [Restore NuGet Packages](#3--restore-nuget-packages)
4. [Apply Database Migrations](#4--apply-database-migrations)
5. [Run the Project](#5--run-the-project)
6. [Verify Everything Works](#6--verify-everything-works)
7. [Your Git Workflow](#7--your-git-workflow)
8. [Common Errors & Fixes](#8--common-errors--fixes)

---

## 1 — Configure appsettings.json

Open `Harfi.API/appsettings.json` and fill in **all** of these values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_PC_NAME\\SQLEXPRESS;Database=HarfiDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },

  "JwtSettings": {
    "SecretKey": "Harfi@SuperSecret_2024_JWT_Key!XYZ",
    "Issuer": "HarfiAPI",
    "Audience": "HarfiClient",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 30
  },

  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "SenderEmail": "YOUR_GMAIL@gmail.com",
    "SenderName": "Harfi Platform",
    "AppPassword": "xxxx xxxx xxxx xxxx"
  },

  "AllowedOrigins": [
    "http://localhost:4200",
    "https://harfi.app"
  ],

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },

  "AllowedHosts": "*"
}
```

### How to find YOUR server name:
1. Open **SSMS**
2. Click **Connect → Database Engine**
3. The value in the **Server name** field is your server name
4. Copy it exactly and replace `YOUR_PC_NAME\\SQLEXPRESS`

**Examples:**
```
DESKTOP-ABC123\SQLEXPRESS    →   "Server=DESKTOP-ABC123\\SQLEXPRESS;..."
LAPTOP-XYZ\SQLEXPRESS        →   "Server=LAPTOP-XYZ\\SQLEXPRESS;..."
```
> ⚠️ Notice the **double backslash** `\\` in the JSON — this is required.

### SecretKey rules:
- Must be **at least 32 characters**
- Use the same value as the rest of the team
---

## 2 — Get Your Gmail App Password

> The app password is what lets the project send emails through your Gmail.  
> You need a **separate Gmail account** for this — do not use your personal one.  
> Ask Esraa for the team Gmail account credentials, or create one for testing.

**Steps to generate the App Password:**

1. Go to https://myaccount.google.com/
2. Click **Security** in the left menu
3. Make sure **2-Step Verification** is **ON**
   - If it's off: click it → follow the steps to enable it
4. Search for **App passwords** in the search bar at the top
5. Click **App passwords**
6. In the **App name** field type: `Harfi`
7. Click **Create**
8. Google shows a **16-character password** like: `abcd efgh ijkl mnop`
9. Copy it and paste it in `appsettings.json` under `AppPassword`

> ⚠️ This password is shown only once. Save it somewhere safe.

---

## 3 — Restore NuGet Packages

Open PowerShell in the project root folder and run:
```bash
dotnet restore
```

You should see each project restored successfully:
```
Restore complete
  Harfi.Models      succeeded
  Harfi.DTOs        succeeded
  Harfi.Repositories succeeded
  Harfi.Services    succeeded
  Harfi.API         succeeded
```

If you see any errors here — check your internet connection and run again.

---

## 4 — Apply Database Migrations

This step creates the `HarfiDB` database and all 14 tables automatically.

Run:
```bash
dotnet ef database update --project Harfi.Repositories
```

You should see:
```
Build started...
Build succeeded.
Applying migration '..._InitialCreate'.
Applying migration '..._AddEmailVerification'.
Done.
```

### Verify in SSMS:
1. Open SSMS → connect to your server
2. Expand **Databases** → you should see `HarfiDB`
3. Expand `HarfiDB` → **Tables** → confirm these exist:
```
dbo.Users
dbo.Craftsmen
dbo.Jobs
dbo.Conversations
dbo.Messages
dbo.Reviews
dbo.Notifications
dbo.RefreshTokens
dbo.AIChatMessages
dbo.RAGDocuments
dbo.MediaFiles
dbo.JobFeedbacks
dbo.UserConnections
dbo.EmailVerifications
dbo.__EFMigrationsHistory
```

> If the migration command fails with **AppControl policy error**:  
> This is a Windows security restriction on some machines.  
> Fix: Update `Harfi.Repositories/Data/AppDbContextFactory.cs` with your server name  
> (the file — already exists in the repo, just update the server name inside it)

---

## 5 — Run the Project

```bash
dotnet run --project Harfi.API
```

You should see in the console:
```
✅ Admin seeded: admin@harfi.com / Admin@1234
Now listening on: http://localhost:5108
Application started. Press Ctrl+C to shut down.
```

> The admin account is created **automatically** on first run — you don't need to do anything.

Open your browser and go to:
```
http://localhost:5108
```
**Swagger UI** should open showing all API endpoints.

---

## 6 — Verify Everything Works

Test these in Swagger in order:

---

### Test 1 — Login as Admin
```json
POST /api/auth/login
{
  "email": "admin@harfi.com",
  "password": "Admin@1234"
}
```
**Expected:** `200 OK` with `accessToken` and `refreshToken`

---

### Test 2 — Register a New User
```json
POST /api/auth/register
{
  "name": "Your Name",
  "email": "your_real_email@gmail.com",
  "password": "Customer@1234",
  "confirmPassword": "Customer@1234",
  "role": "customer",
  "phone": "01012345678"
}
```
**Expected:** `201` + a verification email arrives in your inbox within 1 minute

---

### Test 3 — Try Login Before Verifying
```json
POST /api/auth/login
{
  "email": "your_real_email@gmail.com",
  "password": "Customer@1234"
}
```
**Expected:** `401` with message `"البريد الإلكتروني غير مفعّل"`

---

### Test 4 — Verify Email
Check your inbox → copy the 6-digit code → run:
```json
POST /api/auth/verify-email
{
  "email": "your_real_email@gmail.com",
  "code": "123456"
}
```
**Expected:** `200` with `"تم تفعيل البريد الإلكتروني بنجاح"`

---

### Test 5 — Login After Verifying
```json
POST /api/auth/login
{
  "email": "your_real_email@gmail.com",
  "password": "Customer@1234"
}
```
**Expected:** `200 OK` with tokens ✅

---

### Test 6 — Authorize in Swagger
1. Copy the `accessToken` from the login response
2. Click the **Authorize 🔒** button at the top right
3. Paste the token (just the token, no "Bearer" prefix needed)
4. Click **Authorize** → **Close**

---

### Test 7 — Test Role Protection
```
GET /api/auth/me              → 200 (returns your info)
GET /api/auth/admin-only      → 200 if admin, 403 if not
GET /api/auth/craftsman-only  → 200 if craftsman, 403 if not
```

---

## 7 — Your Git Workflow

### Create your own branch (do this once):
```bash
git checkout dev
git pull origin dev
git checkout -b yourname-phase
```

**Branch naming:**
| Developer | Branch name |
|-----------|-------------|
| Hadeeer | `hadeeer-profiles` |
| Habiba | `habiba-booking` |
| Mazen | `mazen-reviews` |
| Ebrahim | `ebrahim-realtime` |
| Ahmed | `ahmed-ai-agent` |

### Daily workflow:
```bash
# Start of day — get latest changes
git pull origin dev

# Work on your feature...

# Save your work
git add .
git commit -m "feat: describe what you did"
git push origin your-branch-name
```

### When Esraa pushes a new migration:
```bash
git pull origin dev
dotnet ef database update --project Harfi.Repositories
```
This updates your local database automatically.

---

## 8 — Common Errors & Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| `Login failed for user` | Wrong server name | Check Step 1 — update connection string |
| `Cannot open database HarfiDB` | Migration not applied | Run Step 4 again |
| `JWT SecretKey is missing` | Missing config | Add SecretKey to appsettings.json |
| `EmailSettings:AppPassword is missing` | Missing config | Follow Step 2 |
| `Build failed` | Missing packages | Run `dotnet restore` |
| `dotnet ef not found` | EF CLI not installed | Run `dotnet tool install --global dotnet-ef` |
| `AppControl policy blocked` | Windows security on D: drive | Update server name in `AppDbContextFactory.cs` |
| `Port 5108 already in use` | Another instance running | Close other terminals or change port in `launchSettings.json` |
| Email not arriving | Wrong app password | Re-generate app password from Google (Step 2) |
| `401 on admin login` | Admin not seeded | Run project once — seeder runs automatically |

---

## 📞 Contact

If you're stuck on any step, contact **Esraa** before spending more than 10 minutes debugging.

---

*Last updated: Phase 1 complete — N-tier structure + JWT Auth + Email Verification*
