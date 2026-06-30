# Dispute Feature — Implementation Summary

## What Was Built

A complete, end-to-end dispute system replacing the previous `IsDisputed` boolean flag on the `Job` table with a proper `Dispute` entity table. Customers and craftsmen can now open disputes, the other party can respond, and admins can investigate and resolve — with real-time notifications at every step.

---

## 1. Data Model

### New Entity: `Dispute` (`Harfi.Models.Entities.Dispute`)
**File:** `Harfi.Models\Entities\Dispute.cs`

| Field | Type | Notes |
|-------|------|-------|
| `Id` | `int` (PK, auto-increment) | |
| `JobId` | `int` (FK → Jobs) | Which job is disputed |
| `RaisedByUserId` | `int` (FK → Users) | Who opened it |
| `RaisedByRole` | `string(20)` | `"customer"` or `"craftsman"` or `"admin"` |
| `Reason` | `string(500)` | Short reason for the dispute |
| `Description` | `string(2000)?` | Longer explanation |
| `Attachments` | `string(2000)?` | JSON array of image URLs |
| `Status` | `string(20)` | قيد المراجعة, قيد التحقيق, تم الحل, مرفوض |
| `Resolution` | `string(2000)?` | Admin's resolution notes |
| `FavoredParty` | `string(20)?` | `"customer"`, `"craftsman"`, or `null` |
| `ResolvedByAdminId` | `int?` (FK → Users) | Admin who resolved |
| `ResponseMessage` | `string(2000)?` | Other party's counter-evidence |
| `ResponseAttachments` | `string(2000)?` | Attachments from the response |
| `CreatedAt` | `DateTime` | Default GETUTCDATE() |
| `ResolvedAt` | `DateTime?` | |

### New Constants: `DisputeStatusConstants` (`Harfi.Models.Constants.DisputeStatusConstants`)
**File:** `Harfi.Models\Constants\DisputeStatusConstants.cs`
```
Pending     = "قيد المراجعة"
UnderReview = "قيد التحقيق"
Resolved    = "تم الحل"
Rejected    = "مرفوض"

Active = [Pending, UnderReview]   ← used for "is there an open dispute?" checks
```

### Relationship: Job → Disputes (1-to-many)
**File:** `Harfi.Models\Entities\Job.cs:68` — added `public ICollection<Dispute> Disputes`

### EF Core Migration
**File:** `Harfi.Repositories\Migrations\20260630135912_AddDisputeTable.cs`

Creates the `Disputes` table with FKs to `Jobs` and `Users`, indices on `JobId`, `RaisedByUserId`, `ResolvedByAdminId`, and `Status`.

### DbContext Configuration
**File:** `Harfi.Repositories\Data\AppDbContext.cs:316-340`
- `DbSet<Dispute> Disputes`
- Fluent API: three FK relationships (Job, RaisedByUser, ResolvedByAdmin), all Restrict delete
- Default status: `DisputeStatusConstants.Pending`
- Default CreatedAt: `GETUTCDATE()`
- Indices on JobId, RaisedByUserId, Status

---

## 2. DTOs

All in `Harfi.DTOs.Dispute` namespace:

| DTO | File | Key Fields |
|-----|------|------------|
| `CreateDisputeRequest` | `Harfi.DTOs\Dispute\CreateDisputeRequest.cs` | `Reason` (required, 10-500 chars), `Description?`, `Attachments?` |
| `DisputeResponseRequest` | `Harfi.DTOs\Dispute\DisputeResponseRequest.cs` | `Message` (required, 10-2000), `Attachments?` |
| `DisputeSummaryDto` | `Harfi.DTOs\Dispute\DisputeSummaryDto.cs` | `Id`, `JobId`, `Status`, `RaisedByRole`, `Reason`, `CreatedAt`, `ResolvedAt`, `FavoredParty` |
| `DisputeDetailDto` | `Harfi.DTOs\Dispute\DisputeDetailDto.cs` | Full dispute + job info + both parties + conversation link |

### Updated: `JobResponseDto`
**File:** `Harfi.DTOs\Job\JobResponseDto.cs:23-24`
Added `HasOpenDispute: bool` and `DisputeStatus: string?` so customer/craftsman-facing API responses now expose dispute state.

---

## 3. Endpoints

### Customer / Craftsman — via `JobsController`

| Method | Route | Auth | Description | File:Line |
|--------|-------|------|-------------|-----------|
| `POST` | `/api/jobs/{id}/dispute` | Customer or Craftsman | Open a dispute on a job | `JobsController.cs:101` |
| `GET` | `/api/jobs/{id}/dispute` | Customer or Craftsman | View active dispute for a job | `JobsController.cs:133` |

### Customer / Craftsman — via `DisputesController`

| Method | Route | Auth | Description | File:Line |
|--------|-------|------|-------------|-----------|
| `GET` | `/api/disputes/my` | Any authenticated | List my disputes | `DisputesController.cs:23` |
| `POST` | `/api/disputes/{id}/response` | Customer or Craftsman | Respond to a dispute (submit evidence) | `DisputesController.cs:30` |

### Admin — via `AdminController`

| Method | Route | Auth | Description | File:Line |
|--------|-------|------|-------------|-----------|
| `PUT` | `/api/v1/admin/jobs/{id}/flag-dispute` | Admin | Create an admin-flagged dispute | `AdminController.cs:224` |
| `PUT` | `/api/v1/admin/jobs/{id}/resolve-dispute` | Admin | Resolve a dispute (notifies both parties) | `AdminController.cs:232` |
| `GET` | `/api/v1/admin/disputes/{id}` | Admin | Full dispute details | `AdminController.cs:260` |

---

## 4. Service Layer

### `IDisputeService` / `DisputeService` (`Harfi.Services`)
**File:** `Harfi.Services\Interfaces\IDisputeService.cs`
**File:** `Harfi.Services\Implementations\DisputeService.cs`

| Method | Purpose |
|--------|---------|
| `OpenDisputeAsync` | Validates job ownership, job status (only InProgress/Done), no existing active dispute. Creates record + notifies the other party. |
| `GetDisputeForJobAsync` | Returns active dispute for a job (only if user is a party) |
| `GetMyDisputesAsync` | Lists all disputes raised by a user |
| `RespondToDisputeAsync` | Non-initiating party submits evidence. Changes status to UnderReview. Prevents: same-user response, non-party response, response on resolved disputes. |

### `DisputeService` Notification Pattern
On dispute opened: calls `INotificationService.CreateJobNotificationAsync` with type `"dispute_opened"` and pushes via `IRealtimeNotificationPusher` — follows the same pattern as `JobService.AcceptJobAsync` and `CompleteJobAsync`.

### `IAdminService` / `AdminService` — Updated Methods

**File:** `Harfi.Services\Implementations\AdminService.cs`

| Method | Change |
|--------|--------|
| `FlagDisputeAsync` (`:607`) | Now creates a `Dispute` record (Status = UnderReview) instead of setting `IsDisputed` on Job. Checks no active dispute exists. |
| `ResolveDisputeAsync` (`:616`) | Finds active Dispute via `IDisputeRepository`, sets Status=Resolved, stores Resolution/FavoredParty, sets ResolvedByAdminId/ResolvedAt. Calls `NotifyDisputeResolvedAsync` which notifies **both** customer and craftsman via SignalR. |
| `GetJobsAsync` (`:503`) | Derives `IsDisputed` from the Dispute table (checks active dispute per job) |
| `GetJobByIdAsync` (`:546`) | Loads active dispute info into `IsDisputed`, `DisputeRaisedAt`, `DisputeResolvedAt`, `DisputeResolution` from the Dispute table |
| `GetJobMessagesForAdminAsync` (`:672`) | Replaced `!job.IsDisputed` check with `_disputeRepo.GetActiveDisputeForJobAsync(jobId) == null` |
| `GetOverviewAsync` (`:905`) | Replaced `CountAsync(j => j.IsDisputed)` with `_disputeRepo.CountActiveAsync()` |
| `GetJobAnalyticsAsync` (`:997`) | Replaced `jobs.Count(j => j.IsDisputed)` with `await _disputeRepo.CountActiveAsync()` |
| `GetDisputeDetailAsync` (`:764`) | **New** — returns full `DisputeDetailDto` including both parties and conversation reference |

---

## 5. Notifications & Real-Time

### Flow

| Event | Who gets notified | Type | How |
|-------|-------------------|------|-----|
| Customer opens dispute | The craftsman | `"dispute_opened"` | `NotificationService` + `SignalRNotificationPusher` |
| Craftsman opens dispute | The customer | `"dispute_opened"` | Same |
| Admin resolves dispute | Both customer AND craftsman | `"dispute_resolved"` | `NotifyDisputeResolvedAsync` method in AdminService |

### Implementation
- Uses the same `INotificationService.CreateJobNotificationAsync` + `IRealtimeNotificationPusher.PushAsync` pattern already used for `job_accepted`, `job_completed`, `new_order` in `JobService.cs` and `ReviewService.cs`.
- AdminService's `NotifyDisputeResolvedAsync` sends to both parties with Arabic text: `"تم حل النزاع لصالح {العميل/الحرفي/الطرفين}."`

---

## 6. Authorization

| Action | Role check | Data-level check |
|--------|-----------|------------------|
| Open dispute | `[Authorize]` + role in controller (customer/craftsman) | Job must belong to user |
| Respond to dispute | Same | Must be non-initiating party and assigned to the job |
| View dispute | Same | Must be a party to the job |
| Flag dispute | `[Authorize(Roles = "admin")]` on AdminController | — |
| Resolve dispute | Same | Only if active dispute exists |
| View dispute messages | Same | Only if active dispute exists for the job |

---

## 7. Validation

All new DTOs use `System.ComponentModel.DataAnnotations` (same as existing codebase):
- `[Required]`, `[MinLength(10)]`, `[MaxLength]` on Reason/Message fields
- `[MaxLength]` on optional Description/Attachments fields
- Arabic error messages matching the existing style (e.g. `"سبب النزاع مطلوب"`)
- Controllers check `ModelState.IsValid` before proceeding

Business validation in `DisputeService.OpenDisputeAsync`:
- Job must exist (404)
- Only InProgress or Done jobs can be disputed
- No active dispute already exists for this job
- Customer can only dispute their own job
- Craftsman can only dispute jobs assigned to them

---

## 8. Dependency Injection

**File:** `Harfi.API\Extensions\ServiceExtensions.cs`
- `services.AddScoped<IDisputeRepository, DisputeRepository>()` — line 79
- `services.AddScoped<IDisputeService, DisputeService>()` — line 80

---

## 9. Key Files Reference

| Layer | File | Lines |
|-------|------|-------|
| Entity | `Harfi.Models\Entities\Dispute.cs` | Full file |
| Constants | `Harfi.Models\Constants\DisputeStatusConstants.cs` | Full file |
| DbContext config | `Harfi.Repositories\Data\AppDbContext.cs` | 316-340 |
| Migration | `Harfi.Repositories\Migrations\20260630135912_AddDisputeTable.cs` | Full file |
| Job entity update | `Harfi.Models\Entities\Job.cs` | 68 (Disputes nav property) |
| DTOs | `Harfi.DTOs\Dispute\*.cs` | 4 files |
| JobResponseDto update | `Harfi.DTOs\Job\JobResponseDto.cs` | 23-24 |
| Repository interface | `Harfi.Repositories\Interfaces\IDisputeRepository.cs` | Full file |
| Repository impl | `Harfi.Repositories\Implementations\DisputeRepository.cs` | Full file |
| JobRepository update | `Harfi.Repositories\Implementations\JobRepository.cs` | 24,32,39 (Disputes include) |
| Service interface | `Harfi.Services\Interfaces\IDisputeService.cs` | Full file |
| Service impl | `Harfi.Services\Implementations\DisputeService.cs` | Full file |
| AdminService updates | `Harfi.Services\Implementations\AdminService.cs` | 13-28, 503-588, 607-645, 672-683, 764-800, 905-920, 997-1018 |
| IAdminService update | `Harfi.Services\Interfaces\IAdminService.cs` | 32 |
| JobsController | `Harfi.API\Controllers\JobsController.cs` | 14-17, 101-148 |
| DisputesController | `Harfi.API\Controllers\DisputesController.cs` | Full file |
| AdminController update | `Harfi.API\Controllers\AdminController.cs` | 260-270 |
| DI registrations | `Harfi.API\Extensions\ServiceExtensions.cs` | 79-80 |

---

## 10. Backward Compatibility

- The old `IsDisputed`, `DisputeRaisedAt`, `DisputeResolvedAt`, `DisputeResolution` fields on the `Job` entity are still present (no migration removes them), but all admin service code now derives dispute info from the new `Dispute` table instead.
- The `JobAdminDto.IsDisputed` and `JobDetailDto.IsDisputed` fields are still populated (now from the Dispute table), so the frontend admin Jobs panel continues to work unchanged.
- Existing `PUT /api/v1/admin/jobs/{id}/flag-dispute` and `.../resolve-dispute` endpoints keep the same routes and request/response shapes.
- The `JobResponseDto` now includes `HasOpenDispute` and `DisputeStatus` as additive fields — no existing fields were removed.
