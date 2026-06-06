# Admin Access to All Conversations

**Date:** 2026-06-07
**Files changed:** 6 files across 4 projects

---

## Problem

Admins had no way to view conversations for moderation or analytics. The participant-scoped `ConversationsController` only returned conversations where the requesting user was a participant.

## Solution

Added admin-only endpoints to `AdminController` with a dedicated service and DTOs — zero changes to existing participant logic.

## Architecture

```
AdminController [Authorize(Roles = "admin")]
  ├── GET /api/admin/conversations?CustomerName=&CraftsmanName=&ServiceType=&DateFrom=&DateTo=&Page=&PageSize=
  └── GET /api/admin/conversations/{id}
        ↓
IAdminConversationService          ← new, dedicated to admin
  └── AdminConversationService
        ↓
IConversationRepository            ← existing, additive methods only
  └── GetAllConversationsQuery()   ← new, returns IQueryable<Conversation> (no user filter)
  └── GetByIdWithMessagesAsync()   ← new, includes Job + Messages.Sender
```

## Files

### Created (3)

| File | Purpose |
|------|---------|
| `Harfi.DTOs/Chat/AdminConversationDto.cs` | `AdminConversationDto` (list), `AdminConversationDetailDto` (detail + messages), `ConversationFilterDto` (pagination/filters) |
| `Harfi.Services/Interfaces/IAdminConversationService.cs` | Interface with `GetAllConversationsAsync(filter)` and `GetConversationWithMessagesAsync(id)` |
| `Harfi.Services/Implementations/AdminConversationService.cs` | Implementation — no participant filter, supports 5 filters + pagination |

### Modified (3)

| File | Change |
|------|--------|
| `Harfi.Repositories/Interfaces/IConversationRepository.cs` | Added `GetByIdWithMessagesAsync(int)` and `GetAllConversationsQuery()` to the interface |
| `Harfi.Repositories/Implementations/ConversationRepository.cs` | Implemented both — includes `Job`, `Customer`, `Craftsman.User`, and `Messages.Sender`; existing participant-scoped methods untouched |
| `Harfi.API/Controllers/AdminController.cs` | Added `IAdminConversationService` dependency + 2 endpoints |
| `Harfi.API/Extensions/ServiceExtensions.cs` | Registered `IAdminConversationService` / `AdminConversationService` as scoped |

## Intended flows

The two endpoints serve two complementary navigation patterns:

| # | Flow | Entry point | Endpoint |
|---|------|-------------|----------|
| 1 | **Browse** — admin scans/filters all conversations, then picks one to read | Admin dashboard or conversation management page | `GET /api/admin/conversations` → `GET /api/admin/conversations/{id}` |
| 2 | **Direct lookup** — admin clicks "view conversation" from a complaint, dispute flag, or job detail page | Another admin context (complaints, disputes, jobs) | `GET /api/admin/conversations/{id}` directly |

The list endpoint is the **index**. The detail endpoint is the **reader**. Removing either breaks the workflow.

---            
## Endpoints

### `GET /api/admin/conversations` — list endpoint

| Aspect | Detail |
|--------|--------|
| **Query params** | `CustomerName`, `CraftsmanName`, `ServiceType`, `DateFrom`, `DateTo`, `Page`, `PageSize` |
| **Where it searches** | The entire `Conversations` table — **no participant filter**. Joins `Customers`, `Craftsmen` → `Users`, `Jobs`, and `Messages`. |
| **What it returns** | `IEnumerable<AdminConversationDto>` — each item has `Id`, `JobId`, `CustomerName`, `CraftsmanName`, `ServiceType`, `MessageCount`, `LastMessageAt`, `CreatedAt`. Messages are NOT included — only the count and last timestamp. Ordered by `LastMessageAt` descending. |
| **Filters** | Each optional param filters with `.Contains()` (partial match for strings, `>=`/`<=` for dates). All filters compose together. |

```json
[{
  "id": 1,
  "jobId": 1,
  "customerName": "سارة أحمد",
  "craftsmanName": "أحمد علي",
  "serviceType": "سباك",
  "messageCount": 4,
  "lastMessageAt": "2026-03-07T10:40:00Z",
  "createdAt": "2026-03-07T10:00:00Z"
}]
```

### `GET /api/admin/conversations/{id}` — detail endpoint

| Aspect | Detail |
|--------|--------|
| **Path param** | `id` (conversation ID) |
| **Where it searches** | Same tables, but filtered by `Id == {id}` |
| **What it returns** | `AdminConversationDetailDto` — inherits everything from the list DTO **plus** a `Messages` array containing every `MessageDto` with `Id`, `ConversationId`, `SenderId`, `SenderName`, `SenderAvatar`, `Content`, `MessageType`, `IsRead`, `SentAt`. Messages are ordered by `SentAt` ascending. |
| **Not found** | Returns `404` if no conversation with that ID exists |

```json
{
  "id": 1,
  "jobId": 1,
  "customerName": "سارة أحمد",
  "craftsmanName": "أحمد علي",
  "serviceType": "سباك",
  "messageCount": 4,
  "lastMessageAt": "...",
  "createdAt": "...",
  "messages": [
    {
      "id": 1,
      "conversationId": 1,
      "senderId": 12,
      "senderName": "سارة أحمد",
      "content": "أهلاً، محتاج حد يصلح تسريب في الحمام",
      "messageType": "text",
      "isRead": true,
      "sentAt": "..."
    }
  ]
}
```
#### `Why Did Not We Use The Existing endpoint Get /api/conversation/{id}` - why?

the existing Conversations endpoints are:

POST /api/Conversations — create
GET /api/Conversations — list (participant-scoped)
GET /api/Conversations/{id} — detail (participant-scoped)
GET /api/Conversations/{id}/messages — messages (participant-scoped)
PUT /api/Conversations/{id}/read — mark read


The two admin endpoints are still needed.
The existing ones all enforce the participant filter — an admin calling GET /api/Conversations only sees conversations they're personally in.
The admin endpoints bypass that filter entirely.
Same routes, completely different data scope.

```

## Key decisions

| Decision | Rationale |
|----------|-----------|
| New endpoints in `AdminController` | Admin concerns stay together; `ConversationsController` remains participant-scoped |
| New `IAdminConversationService` | Isolated from participant logic; no risk of accidentally removing the userId filter |
| `GetAllConversationsQuery()` returns `IQueryable` | Lets the service compose filters (name, service, date) before hitting the database |
| Added `GetByIdWithMessagesAsync()` | Unlike `GetByIdWithDetailsAsync` (which takes only 1 message), this loads all messages with sender info |
| Reuses existing `MessageDto` | Avoids duplicating DTOs; admin detail view reuses the same shape |


