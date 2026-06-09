# Harfi Admin API — Complete Endpoint Documentation

> **Base URL:** `/api/v1/admin`  
> **Authentication:** JWT Bearer token with role claim `"admin"`  
> **Role attribute:** `[Authorize(Roles = "admin")]` (class-level on `AdminController`)  
> **Additional admin endpoints** exist in `AIController` and `AuthController`.  
> **Language:** All error/success messages are in Arabic.

---

## Table of Contents

1. [Craftsman Verification](#1-craftsman-verification)
2. [User Management](#2-user-management)
3. [Jobs & Disputes](#3-jobs--disputes)
4. [Content Moderation (Reviews & Reports)](#4-content-moderation)
5. [AI Monitoring & Ingestion](#5-ai-monitoring--ingestion)
6. [Analytics & Export](#6-analytics--export)
7. [Platform Configuration](#7-platform-configuration)
8. [Audit Logs](#8-audit-logs)
9. [Utility Endpoints (Auth & Cross-cutting)](#9-utility-endpoints)
10. [Appendix: Common Response Shapes](#10-appendix-common-response-shapes)

---

### Common Request Headers

| Header | Value | Required |
|---|---|---|
| `Authorization` | `Bearer {jwt-token}` | ✓ Always |
| `Content-Type` | `application/json` | ✓ For POST/PUT with body |
| `Accept` | `application/json` | Optional |

### Error Response Shape (non-paginated)

```json
{
  "message": "string — Arabic error description"
}
```

### Paginated Response Shape

```json
{
  "items": [ ... ],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20,
  "totalPages": 0
}
```

### Generic Action Response Shape

```json
{
  "success": true,
  "message": "string",
  "data": { }
}
```

---

## 1. Craftsman Verification

### `GET /api/v1/admin/craftsmen/pending`

**Summary:** List craftsmen awaiting admin approval, with optional filtering by city and service type.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `page` | int | No (default: 1) | Page number |
| `pageSize` | int | No (default: 20) | Items per page |
| `city` | string | No | Filter by city name |
| `serviceType` | string | No | Filter by service type |

**Success (200):**

```json
{
  "items": [
    {
      "id": 1,
      "userId": 5,
      "fullName": "أحمد علي",
      "email": "ahmed@example.com",
      "phone": "01001234567",
      "serviceType": "سباك",
      "city": "القاهرة",
      "neighborhood": "مدينة نصر",
      "experience": 5,
      "nationalIdUrl": "https://storage.example.com/nids/123.jpg",
      "bio": "خبرة في جميع أعمال السباكة",
      "createdAt": "2025-01-15T10:30:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

### `GET /api/v1/admin/craftsmen/approved`

**Summary:** List approved craftsmen with filtering by city, service type, and minimum rating.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `page` | int | No (default: 1) | Page number |
| `pageSize` | int | No (default: 20) | Items per page |
| `city` | string | No | Filter by city name |
| `serviceType` | string | No | Filter by service type |
| `minRating` | decimal | No | Minimum rating filter (e.g., 4.0) |

**Success (200):**

```json
{
  "items": [
    {
      "id": 2,
      "userId": 8,
      "fullName": "محمود حسن",
      "email": "mahmoud@example.com",
      "phone": "01098765432",
      "serviceType": "كهربائي",
      "city": "الإسكندرية",
      "neighborhood": "سيدي جابر",
      "experience": 8,
      "rating": 4.5,
      "isAvailable": true,
      "bio": "متخصص في الكهرباء المنزلية",
      "createdAt": "2025-02-01T08:00:00Z"
    }
  ],
  "totalCount": 15,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

### `GET /api/v1/admin/craftsmen/rejected`

**Summary:** List rejected craftsmen.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `page` | int | No (default: 1) | Page number |
| `pageSize` | int | No (default: 20) | Items per page |

**Success (200):**

```json
{
  "items": [
    {
      "id": 3,
      "userId": 12,
      "fullName": "خالد سعيد",
      "email": "khaled@example.com",
      "phone": "01112345678",
      "serviceType": "نجار",
      "city": "الجيزة",
      "rejectionReason": "المستندات المقدمة غير مكتملة",
      "createdAt": "2025-01-20T12:00:00Z",
      "deletedAt": null
    }
  ],
  "totalCount": 3,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

### `GET /api/v1/admin/craftsmen/{id}`

**Summary:** Get detailed profile of a single craftsman (any status — pending, approved, rejected, or deleted).

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Craftsman ID |

**Success (200):**

```json
{
  "id": 1,
  "userId": 5,
  "fullName": "أحمد علي",
  "email": "ahmed@example.com",
  "phone": "01001234567",
  "profileImageUrl": "https://storage.example.com/profiles/1.jpg",
  "serviceType": "سباك",
  "city": "القاهرة",
  "neighborhood": "مدينة نصر",
  "priceRangeMin": 200.00,
  "priceRangeMax": 800.00,
  "experience": 5,
  "isApproved": true,
  "isAvailable": true,
  "isDeleted": false,
  "rating": 4.2,
  "bio": "خبرة في جميع أعمال السباكة",
  "nationalIdUrl": "https://storage.example.com/nids/123.jpg",
  "rejectionReason": null,
  "deletionReason": null,
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-20T14:00:00Z",
  "completedJobsCount": 12,
  "totalReviews": 8
}
```

**Error (404):** `{ "message": "لم يتم العثور على الحرفي" }`

---

### `PUT /api/v1/admin/craftsmen/{id}/approve`

**Summary:** Approve a pending craftsman application.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Craftsman ID |

**Body:**

```json
{
  "notifyMessage": "مرحباً بك في حرفي! تم قبول طلبك بنجاح 🎉"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `notifyMessage` | string | No | Optional custom notification sent to the craftsman |

**Success (200):**

```json
{
  "success": true,
  "message": "تم قبول الحرفي بنجاح",
  "data": null
}
```

---

### `PUT /api/v1/admin/craftsmen/{id}/reject`

**Summary:** Reject a craftsman application with a required reason.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Craftsman ID |

**Body:**

```json
{
  "reason": "المستندات المقدمة غير مكتملة وغير مطابقة للشروط — برجاء إعادة تقديم الطلب بصورة واضحة للبطاقة الشخصية وإثبات الخبرة."
}
```

| Field | Type | Required | Validation |
|---|---|---|---|
| `reason` | string | ✓ | Min 10, Max 500 chars |

**Success (200):**

```json
{
  "success": true,
  "message": "تم رفض الحرفي بنجاح",
  "data": null
}
```

**Error (400):** Model validation errors when `reason` is missing or too short.

---

### `PUT /api/v1/admin/craftsmen/{id}/suspend`

**Summary:** Temporarily suspend an active/approved craftsman.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Craftsman ID |

**Body:**

```json
{
  "reason": "بلاغات متكررة من العملاء بعدم الالتزام بالمواعيد — تم تعليق الحساب لمدة 14 يوماً للمراجعة."
}
```

| Field | Type | Required | Validation |
|---|---|---|---|
| `reason` | string | ✓ | Min 10, Max 500 chars |

**Success (200):**

```json
{
  "success": true,
  "message": "تم تعليق الحرفي بنجاح",
  "data": null
}
```

---

### `DELETE /api/v1/admin/craftsmen/{id}`

**Summary:** Soft-delete (remove) a craftsman profile. The record remains in the database with `IsDeleted = true`.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Craftsman ID |

**Body:**

```json
{
  "reason": "نشاط مشبوه ومخالفة لشروط الاستخدام — تم حذف الحساب بعد المراجعة."
}
```

| Field | Type | Required | Validation |
|---|---|---|---|
| `reason` | string | ✓ | Min 10, Max 500 chars |

**Success (200):**

```json
{
  "success": true,
  "message": "تم حذف الحرفي بنجاح",
  "data": null
}
```

---

## 2. User Management

### `GET /api/v1/admin/users`

**Summary:** List all platform users with filtering by role, active status, and search query.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `role` | string | No | Filter by role (`customer`, `craftsman`, `admin`) |
| `page` | int | No (default: 1) | Page number |
| `pageSize` | int | No (default: 20) | Items per page |
| `isActive` | bool | No | Filter by active/inactive status |
| `search` | string | No | Search by name or email (partial match) |

**Success (200):**

```json
{
  "items": [
    {
      "id": 1,
      "name": "محمد أحمد",
      "email": "mohamed@example.com",
      "phone": "01001111111",
      "role": "customer",
      "isActive": true,
      "isVerified": true,
      "isDeleted": false,
      "profileImageUrl": null,
      "createdAt": "2025-01-01T00:00:00Z"
    }
  ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

---

### `GET /api/v1/admin/users/{id}`

**Summary:** Get detailed profile of any user by ID, including related counts.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | User ID |

**Success (200):**

```json
{
  "id": 1,
  "name": "محمد أحمد",
  "email": "mohamed@example.com",
  "phone": "01001111111",
  "role": "customer",
  "isActive": true,
  "isVerified": true,
  "isDeleted": false,
  "profileImageUrl": null,
  "deletionReason": null,
  "deletedAt": null,
  "createdAt": "2025-01-01T00:00:00Z",
  "craftsmanProfileId": 0,
  "jobsCount": 5,
  "reviewsCount": 3
}
```

**Error (404):** `{ "message": "لم يتم العثور على المستخدم" }`

---

### `GET /api/v1/admin/users/{id}/activity`

**Summary:** Get recent activity timeline for a specific user (logins, job creations, reviews, etc.).

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | User ID |

**Success (200):**

```json
[
  {
    "action": "تسجيل دخول",
    "details": "تم تسجيل الدخول من جهاز جديد",
    "timestamp": "2025-03-01T09:00:00Z"
  },
  {
    "action": "إنشاء وظيفة",
    "details": "تم إنشاء وظيفة جديدة — سباك في القاهرة",
    "timestamp": "2025-03-02T14:30:00Z"
  }
]
```

**Error (404):** `{ "message": "لم يتم العثور على المستخدم" }`

---

### `PUT /api/v1/admin/users/{id}/deactivate`

**Summary:** Deactivate a user account (prevents login).

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | User ID |

**Body:**

```json
{
  "reason": "نشاط غير عادي ومخالف لسياسة الاستخدام — تم تعطيل الحساب لحين المراجعة."
}
```

| Field | Type | Required | Validation |
|---|---|---|---|
| `reason` | string | ✓ | Min 10, Max 500 chars |

**Success (200):**

```json
{
  "success": true,
  "message": "تم تعطيل المستخدم بنجاح",
  "data": null
}
```

**Error (400):** `{ "message": "لا يمكن تعطيل حساب الأدمن" }` (when trying to deactivate an admin).

---

### `PUT /api/v1/admin/users/{id}/reactivate`

**Summary:** Reactivate a previously deactivated user account.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | User ID |

**Body:** None

**Success (200):**

```json
{
  "success": true,
  "message": "تم إعادة تفعيل المستخدم بنجاح",
  "data": null
}
```

**Error (404):** `{ "message": "لم يتم العثور على المستخدم" }`

---

### `DELETE /api/v1/admin/users/{id}`

**Summary:** Soft-delete a user account.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | User ID |

**Body:**

```json
{
  "reason": "طلب حذف الحساب من المستخدم — تمت المراجعة والموافقة."
}
```

| Field | Type | Required | Validation |
|---|---|---|---|
| `reason` | string | ✓ | Min 10, Max 500 chars |

**Success (200):**

```json
{
  "success": true,
  "message": "تم حذف المستخدم بنجاح",
  "data": null
}
```

**Error (400):** `{ "message": "لا يمكن حذف حساب الأدمن" }`

---

## 3. Jobs & Disputes

### `GET /api/v1/admin/jobs`

**Summary:** List all jobs across the platform with status, craftsman, and customer filtering.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `status` | string | No | Filter by status (e.g., `open`, `in_progress`, `completed`) |
| `craftsmanId` | int | No | Filter by assigned craftsman |
| `customerId` | int | No | Filter by customer who created the job |
| `page` | int | No (default: 1) | Page number |
| `pageSize` | int | No (default: 20) | Items per page |
| `from` | DateTime | No | Start date filter (ISO 8601) |
| `to` | DateTime | No | End date filter (ISO 8601) |

**Success (200):**

```json
{
  "items": [
    {
      "id": 10,
      "customerId": 1,
      "customerName": "محمد أحمد",
      "craftsmanId": 5,
      "craftsmanName": "أحمد علي",
      "status": "completed",
      "serviceType": "سباك",
      "description": "تسريب مياه في الحمام",
      "address": "12 شارع النيل، القاهرة",
      "isDisputed": false,
      "createdAt": "2025-02-10T10:00:00Z",
      "completedAt": "2025-02-12T16:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

### `GET /api/v1/admin/jobs/{id}`

**Summary:** Get detailed information about a specific job, including dispute data.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Job ID |

**Success (200):**

```json
{
  "id": 10,
  "customerId": 1,
  "customerName": "محمد أحمد",
  "craftsmanId": 5,
  "craftsmanName": "أحمد علي",
  "status": "disputed",
  "serviceType": "سباك",
  "description": "تسريب مياه في الحمام",
  "address": "12 شارع النيل، القاهرة",
  "preferredDate": "2025-02-11T09:00:00Z",
  "problemImageUrl": "https://storage.example.com/jobs/10.jpg",
  "problemDescription": "مواسير الحمام بتسرب من تحت الحوض",
  "solutionDescription": "تم تغيير القطعة التالفة",
  "isDisputed": true,
  "disputeRaisedAt": "2025-02-13T08:00:00Z",
  "disputeResolvedAt": null,
  "disputeResolution": null,
  "createdAt": "2025-02-10T10:00:00Z",
  "completedAt": null,
  "updatedAt": "2025-02-13T08:00:00Z"
}
```

**Error (404):** `{ "message": "لم يتم العثور على الوظيفة" }`

---

### `PUT /api/v1/admin/jobs/{id}/status`

**Summary:** Manually update the status of a job (e.g., force-complete, cancel, reopen).

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Job ID |

**Body:**

```json
{
  "status": "completed",
  "justification": "تم تأكيد إتمام العمل من العميل عبر الاتصال الهاتفي."
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `status` | string | ✓ | New status value (e.g., `open`, `in_progress`, `completed`, `cancelled`) |
| `justification` | string | ✓ | Reason for admin override |

**Success (200):**

```json
{
  "success": true,
  "message": "تم تحديث حالة الوظيفة بنجاح",
  "data": null
}
```

---

### `PUT /api/v1/admin/jobs/{id}/flag-dispute`

**Summary:** Manually flag a job as disputed for admin review.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Job ID |

**Body:**

```json
{
  "reason": "بلغ العميل أن العمل لم يكتمل بالشكل المطلوب — تم رفع النزاع للمراجعة."
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `reason` | string | ✓ | Reason for flagging |

**Success (200):**

```json
{
  "success": true,
  "message": "تم رفع النزاع بنجاح",
  "data": null
}
```

---

### `PUT /api/v1/admin/jobs/{id}/resolve-dispute`

**Summary:** Resolve an active dispute by specifying resolution and favored party.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Job ID |

**Body:**

```json
{
  "resolution": "تم الاتفاق على إعادة الخدمة بنصف التكلفة — تم تعويض الطرفين.",
  "favoredParty": "customer"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `resolution` | string | ✓ | Resolution details |
| `favoredParty` | string | ✓ | Who was favored (`customer` or `craftsman`) |

**Success (200):**

```json
{
  "success": true,
  "message": "تم حل النزاع بنجاح",
  "data": null
}
```

---

### `GET /api/v1/admin/jobs/{id}/chat-metadata`

**Summary:** Get metadata about the conversation/chat associated with a job.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Job ID |

**Success (200):**

```json
{
  "conversationId": 5,
  "jobId": 10,
  "customerName": "محمد أحمد",
  "craftsmanName": "أحمد علي",
  "messageCount": 24,
  "createdAt": "2025-02-10T10:00:00Z",
  "lastMessageAt": "2025-02-13T07:45:00Z"
}
```

**Error (404):** `{ "message": "لم يتم العثور على المحادثة" }`

---

### `GET /api/v1/admin/jobs/{id}/chat-messages`

**Summary:** View all chat messages between customer and craftsman for a job. This action is **audit-logged**.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Job ID |

**Success (200):**

```json
[
  {
    "id": 101,
    "conversationId": 5,
    "senderId": 1,
    "senderName": "محمد أحمد",
    "senderAvatar": null,
    "content": "السلام عليكم، ممكن تيجي النهارده؟",
    "messageType": "text",
    "isRead": true,
    "sentAt": "2025-02-10T10:05:00Z"
  },
  {
    "id": 102,
    "conversationId": 5,
    "senderId": 5,
    "senderName": "أحمد علي",
    "senderAvatar": "https://storage.example.com/avatars/5.jpg",
    "content": "وعليكم السلام، إن شاء الله الساعة 4 العصر",
    "messageType": "text",
    "isRead": true,
    "sentAt": "2025-02-10T10:10:00Z"
  }
]
```

**Error (403):** `{ "message": "غير مصرح بالاطلاع على هذه المحادثة" }`  
**Error (404):** `{ "message": "لم يتم العثور على الوظيفة" }`

---

## 4. Content Moderation

### `GET /api/v1/admin/reviews`

**Summary:** List all reviews with optional filtering by craftsman and star rating range.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `craftsmanId` | int | No | Filter by craftsman |
| `minStars` | int | No | Minimum star rating (1–5) |
| `maxStars` | int | No | Maximum star rating (1–5) |
| `page` | int | No (default: 1) | Page number |
| `pageSize` | int | No (default: 20) | Items per page |

**Success (200):**

```json
{
  "items": [
    {
      "id": 50,
      "jobId": 10,
      "customerId": 1,
      "customerName": "محمد أحمد",
      "craftsmanId": 5,
      "craftsmanName": "أحمد علي",
      "stars": 5,
      "comment": "شغل ممتاز والتزام في المواعيد",
      "isDeleted": false,
      "createdAt": "2025-02-15T12:00:00Z"
    }
  ],
  "totalCount": 200,
  "page": 1,
  "pageSize": 20,
  "totalPages": 10
}
```

---

### `GET /api/v1/admin/reviews/{id}`

**Summary:** Get detailed information about a specific review.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Review ID |

**Success (200):**

```json
{
  "id": 50,
  "jobId": 10,
  "jobDescription": "تسريب مياه في الحمام",
  "customerId": 1,
  "customerName": "محمد أحمد",
  "craftsmanId": 5,
  "craftsmanName": "أحمد علي",
  "stars": 5,
  "comment": "شغل ممتاز والتزام في المواعيد",
  "isDeleted": false,
  "deletionReason": null,
  "deletedAt": null,
  "createdAt": "2025-02-15T12:00:00Z"
}
```

**Error (404):** `{ "message": "لم يتم العثور على التقييم" }`

---

### `DELETE /api/v1/admin/reviews/{id}`

**Summary:** Soft-delete a review with a required reason.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Review ID |

**Body:**

```json
{
  "reason": "تقييم غير لائق يحتوي على لغة مسيئة — تم إخفاؤه بعد المراجعة."
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `reason` | string | ✓ | Reason for deletion |

**Success (200):**

```json
{
  "success": true,
  "message": "تم حذف التقييم بنجاح",
  "data": null
}
```

---

### `GET /api/v1/admin/reports`

**Summary:** List all reports filed by users against craftsmen, jobs, or reviews.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `status` | string | No | Filter by status (`open`, `resolved`) |
| `type` | string | No | Filter by target type (`craftsman`, `job`, `review`) |
| `page` | int | No (default: 1) | Page number |
| `pageSize` | int | No (default: 20) | Items per page |

**Success (200):**

```json
{
  "items": [
    {
      "id": 3,
      "reportedByUserId": 1,
      "reportedByUserName": "محمد أحمد",
      "targetType": "craftsman",
      "targetId": 5,
      "reason": "لم يلتزم الحرفي بالمواعيد المتفق عليها",
      "status": "open",
      "resolvedByAdminId": null,
      "resolutionNotes": null,
      "createdAt": "2025-03-01T09:00:00Z",
      "resolvedAt": null
    }
  ],
  "totalCount": 10,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

### `PUT /api/v1/admin/reports/{id}/resolve`

**Summary:** Resolve a user report by taking action and adding notes.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Report ID |

**Body:**

```json
{
  "action": "تم تحذير الحرفي وتنبيهه بالالتزام بالشروط",
  "notes": "تم الاتصال بالطرفين والتوصل إلى تفاهم — تم إغلاق البلاغ."
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `action` | string | ✓ | Action taken |
| `notes` | string | ✓ | Internal resolution notes |

**Success (200):**

```json
{
  "success": true,
  "message": "تم حل البلاغ بنجاح",
  "data": null
}
```

---

### `GET /api/v1/admin/ai-logs`

**Summary:** View AI chat interaction logs for monitoring and debugging.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `page` | int | No (default: 1) | Page number |
| `pageSize` | int | No (default: 20) | Items per page |
| `from` | DateTime | No | Start date filter (ISO 8601) |
| `to` | DateTime | No | End date filter (ISO 8601) |

**Success (200):**

```json
{
  "items": [
    {
      "id": 1000,
      "userId": 1,
      "userName": "محمد أحمد",
      "sessionId": "sess_abc123",
      "role": "user",
      "content": "عاوز سباك في القاهرة",
      "toolUsed": "intent-extraction",
      "tokensUsed": 150,
      "createdAt": "2025-03-01T10:00:00Z"
    }
  ],
  "totalCount": 5000,
  "page": 1,
  "pageSize": 20,
  "totalPages": 250
}
```

---

## 5. AI Monitoring & Ingestion

### `POST /api/AI/ingest/craftsmen`

**Summary:** Index craftsmen data into the vector database for AI-powered search.

**Auth:** `[Authorize(Roles = "admin")]`

**Base Route:** `/api/AI`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `fromId` | int | No (default: 0) | Start indexing from this craftsman ID (supports incremental/resume) |

**Success (200):**

```json
{
  "totalCraftsmen": 150,
  "totalChunksIndexed": 450,
  "message": "تم فهرسة 150 حرفي بنجاح"
}
```

---

### `POST /api/AI/ingest/jobs`

**Summary:** Index completed job solutions into the vector database for the solution recommendation feature.

**Auth:** `[Authorize(Roles = "admin")]`

**Base Route:** `/api/AI`

**Body:** None

**Success (200):**

```json
{
  "indexed": 75
}
```

---

### `GET /api/AI/vectors/count`

**Summary:** Get total number of vectors currently stored in the vector database.

**Auth:** `[Authorize(Roles = "admin")]`

**Base Route:** `/api/AI`

**Success (200):**

```json
{
  "totalVectors": 12345
}
```

---

## 6. Analytics & Export

### `GET /api/v1/admin/analytics/overview`

**Summary:** Dashboard overview with key platform metrics.

**Auth:** `[Authorize(Roles = "admin")]`

**Success (200):**

```json
{
  "totalUsers": 1500,
  "totalCraftsmen": 350,
  "pendingCraftsmen": 12,
  "activeJobs": 45,
  "completedJobs": 1200,
  "disputedJobs": 3,
  "pendingReports": 8,
  "totalReviews": 850,
  "newUsersThisMonth": 65,
  "averageRating": 4.3
}
```

---

### `GET /api/v1/admin/analytics/craftsmen`

**Summary:** Craftsman-specific analytics with breakdowns by service type and city.

**Auth:** `[Authorize(Roles = "admin")]`

**Success (200):**

```json
{
  "totalCraftsmen": 350,
  "pendingApproval": 12,
  "approved": 310,
  "rejected": 18,
  "suspended": 10,
  "averageRating": 4.3,
  "byServiceType": {
    "سباك": 80,
    "كهربائي": 95,
    "نجار": 45,
    "دهان": 30
  },
  "byCity": {
    "القاهرة": 120,
    "الإسكندرية": 60,
    "الجيزة": 50
  }
}
```

---

### `GET /api/v1/admin/analytics/jobs`

**Summary:** Job-specific analytics with status breakdown and average completion time.

**Auth:** `[Authorize(Roles = "admin")]`

**Success (200):**

```json
{
  "totalJobs": 1500,
  "open": 45,
  "inProgress": 30,
  "completed": 1200,
  "rejected": 200,
  "disputed": 25,
  "byServiceType": {
    "سباك": 400,
    "كهربائي": 500
  },
  "averageCompletionDays": 2.5
}
```

---

### `GET /api/v1/admin/analytics/ai`

**Summary:** AI usage statistics including total chats, token usage, and ingestion counts.

**Auth:** `[Authorize(Roles = "admin")]`

**Success (200):**

```json
{
  "totalChats": 8500,
  "totalTokensUsed": 250000,
  "totalCraftsmenIngested": 350,
  "totalSolutionsIngested": 1200,
  "averageTokensPerChat": 29.4
}
```

---

### `GET /api/v1/admin/analytics/reviews`

**Summary:** Review analytics with star distribution and deleted review count.

**Auth:** `[Authorize(Roles = "admin")]`

**Success (200):**

```json
{
  "totalReviews": 850,
  "averageStars": 4.3,
  "starDistribution": {
    "1": 20,
    "2": 30,
    "3": 80,
    "4": 220,
    "5": 500
  },
  "deletedReviews": 15
}
```

---

### `GET /api/v1/admin/analytics/export`

**Summary:** Export platform data as a CSV file for reporting or external analysis.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `type` | string | No (default: `users`) | Data type to export (`users`, `craftsmen`, `jobs`, `reviews`, `reports`) |
| `from` | DateTime | No | Start date filter (ISO 8601) |
| `to` | DateTime | No | End date filter (ISO 8601) |

**Success (200):** `Content-Type: text/csv` — downloads a CSV file named `harfi-{type}-{yyyyMMdd}.csv`

```csv
Id,Name,Email,Role,CreatedAt
1,محمد أحمد,mohamed@example.com,customer,2025-01-01
```

**Error (400):** `{ "message": "نوع البيانات غير صالح — الأنواع المدعومة: users, craftsmen, jobs, reviews, reports" }`

---

## 7. Platform Configuration

### `GET /api/v1/admin/config/service-types`

**Summary:** List all service types available on the platform.

**Auth:** `[Authorize(Roles = "admin")]`

**Success (200):**

```json
[
  {
    "id": 1,
    "nameAr": "سباك",
    "nameEn": "Plumber",
    "icon": "🔧",
    "isActive": true
  },
  {
    "id": 2,
    "nameAr": "كهربائي",
    "nameEn": "Electrician",
    "icon": "⚡",
    "isActive": true
  }
]
```

---

### `POST /api/v1/admin/config/service-types`

**Summary:** Create a new service type.

**Auth:** `[Authorize(Roles = "admin")]`

**Body:**

```json
{
  "nameAr": "حداد",
  "nameEn": "Blacksmith",
  "icon": "🔨",
  "isActive": true
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `nameAr` | string | ✓ | Arabic name |
| `nameEn` | string | ✓ | English name |
| `icon` | string | No | Icon/emoji representation |
| `isActive` | bool | No (default: true) | Whether the type is active |

**Success (201):**

```json
{
  "id": 5,
  "nameAr": "حداد",
  "nameEn": "Blacksmith",
  "icon": "🔨",
  "isActive": true
}
```

**Error (400):** Validation errors when name fields are missing.

---

### `PUT /api/v1/admin/config/service-types/{id}`

**Summary:** Update an existing service type.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Service type ID |

**Body:** Same shape as `POST /config/service-types`

**Success (200):**

```json
{
  "id": 1,
  "nameAr": "سباك",
  "nameEn": "Plumber",
  "icon": "🔧",
  "isActive": true
}
```

**Error (404):** `{ "message": "لم يتم العثور على نوع الخدمة" }`

---

### `DELETE /api/v1/admin/config/service-types/{id}`

**Summary:** Delete a service type.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Service type ID |

**Success (200):**

```json
{
  "success": true,
  "message": "تم حذف نوع الخدمة بنجاح",
  "data": null
}
```

**Error (400):** `{ "success": false, "message": "..." }` when the type is in use  
**Error (404):** `{ "message": "لم يتم العثور على نوع الخدمة" }`

---

### `GET /api/v1/admin/config/cities`

**Summary:** List all cities supported on the platform.

**Auth:** `[Authorize(Roles = "admin")]`

**Success (200):**

```json
[
  {
    "id": 1,
    "nameAr": "القاهرة",
    "nameEn": "Cairo",
    "governorate": "القاهرة",
    "isActive": true
  },
  {
    "id": 2,
    "nameAr": "الإسكندرية",
    "nameEn": "Alexandria",
    "governorate": "الإسكندرية",
    "isActive": true
  }
]
```

---

### `POST /api/v1/admin/config/cities`

**Summary:** Create a new city.

**Auth:** `[Authorize(Roles = "admin")]`

**Body:**

```json
{
  "nameAr": "المنصورة",
  "nameEn": "Mansoura",
  "governorate": "الدقهلية",
  "isActive": true
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `nameAr` | string | ✓ | Arabic name |
| `nameEn` | string | ✓ | English name |
| `governorate` | string | No | Governorate name |
| `isActive` | bool | No (default: true) | Whether the city is active |

**Success (201):**

```json
{
  "id": 10,
  "nameAr": "المنصورة",
  "nameEn": "Mansoura",
  "governorate": "الدقهلية",
  "isActive": true
}
```

---

### `PUT /api/v1/admin/config/cities/{id}`

**Summary:** Update an existing city.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | City ID |

**Body:** Same shape as `POST /config/cities`

**Success (200):** Returns the updated `CityDto`.

**Error (404):** `{ "message": "لم يتم العثور على المدينة" }`

---

### `DELETE /api/v1/admin/config/cities/{id}`

**Summary:** Delete a city.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | City ID |

**Success (200):**

```json
{
  "success": true,
  "message": "تم حذف المدينة بنجاح",
  "data": null
}
```

**Error (404):** `{ "message": "لم يتم العثور على المدينة" }`

---

### `GET /api/v1/admin/config/feature-flags`

**Summary:** List all feature flags and their current status.

**Auth:** `[Authorize(Roles = "admin")]`

**Success (200):**

```json
[
  {
    "key": "ai_chat_enabled",
    "isEnabled": true,
    "updatedAt": "2025-03-01T00:00:00Z"
  },
  {
    "key": "craftsman_direct_booking",
    "isEnabled": false,
    "updatedAt": "2025-03-10T00:00:00Z"
  }
]
```

---

### `PUT /api/v1/admin/config/feature-flags/{key}`

**Summary:** Toggle a feature flag on or off.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `key` | string | ✓ | Feature flag key |

**Body:**

```json
{
  "isEnabled": true
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `isEnabled` | bool | ✓ | New enabled state |

**Success (200):**

```json
{
  "success": true,
  "message": "تم تحديث الميزة بنجاح",
  "data": null
}
```

---

## 8. Audit Logs

### `GET /api/v1/admin/audit-logs`

**Summary:** Retrieve paginated audit trail of all admin actions.

**Auth:** `[Authorize(Roles = "admin")]`

**Query Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `adminId` | int | No | Filter by admin who performed the action |
| `action` | string | No | Filter by action type (e.g., `approve`, `reject`, `delete`) |
| `targetType` | string | No | Filter by target entity type (e.g., `craftsman`, `user`, `review`) |
| `from` | DateTime | No | Start date filter (ISO 8601) |
| `to` | DateTime | No | End date filter (ISO 8601) |
| `page` | int | No (default: 1) | Page number |
| `pageSize` | int | No (default: 20) | Items per page |

**Success (200):**

```json
{
  "items": [
    {
      "id": 1,
      "adminId": 3,
      "adminName": "مدير النظام",
      "action": "approve_craftsman",
      "targetType": "craftsman",
      "targetId": 5,
      "notes": "تم قبول الحرفي أحمد علي",
      "ipAddress": "192.168.1.100",
      "createdAt": "2025-03-01T10:00:00Z"
    }
  ],
  "totalCount": 500,
  "page": 1,
  "pageSize": 20,
  "totalPages": 25
}
```

---

### `GET /api/v1/admin/audit-logs/{id}`

**Summary:** Get a single audit log entry by ID.

**Auth:** `[Authorize(Roles = "admin")]`

**Route Params:**

| Name | Type | Required | Description |
|---|---|---|---|
| `id` | int | ✓ | Audit log entry ID |

**Success (200):**

```json
{
  "id": 1,
  "adminId": 3,
  "adminName": "مدير النظام",
  "action": "approve_craftsman",
  "targetType": "craftsman",
  "targetId": 5,
  "notes": "تم قبول الحرفي أحمد علي",
  "ipAddress": "192.168.1.100",
  "createdAt": "2025-03-01T10:00:00Z"
}
```

**Error (404):** `{ "message": "لم يتم العثور على سجل التدقيق" }`

---

## 9. Utility Endpoints

### `GET /api/auth/admin-only`

**Summary:** Simple health-check endpoint to verify admin authentication is working.

**Auth:** `[Authorize(Roles = "admin")]`

**Base Route:** `/api/auth`

**Success (200):**

```json
{
  "message": "أهلاً بالأدمن 👋"
}
```

---

## 10. Imperative Admin-Allowed Endpoints

These endpoints do **not** have the `[Authorize(Roles = "admin")]` attribute but contain **imperative role checks** that allow admin users to bypass ownership restrictions. They are "admin-capable" rather than admin-exclusive.

| Method | Endpoint | Controller | What it does |
|---|---|---|---|
| `GET` | `/api/Users/profile/{id}` | UsersController | Admin can view any user's profile (bypasses self-only check) |
| `PUT` | `/api/Users/profile/{id}` | UsersController | Admin can update any user's profile |
| `POST` | `/api/Users/profile/{id}/upload-image` | UsersController | Admin can upload profile image for any user |
| `PUT` | `/api/Craftsmen/{id}` | CraftsmenController | Admin can update any craftsman's profile |
| `POST` | `/api/Craftsmen/{id}/upload-image` | CraftsmenController | Admin can upload image for any craftsman |
| `GET` | `/api/jobs/customer/{id}` | JobsController | Admin can view any customer's jobs |
| `GET` | `/api/jobs/craftsman/{id}` | JobsController | Admin can view any craftsman's jobs (bypasses craftsman self-ownership check) |

**Auth for these:** JWT Bearer (`[Authorize]`) + runtime role check `if (requestingRole != "admin")`

---

## 10. Appendix: Common Response Shapes

### Paginated Result (`PagedResult<T>`)

```json
{
  "items": [],
  "totalCount": 0,
  "page": 1,
  "pageSize": 20,
  "totalPages": 0
}
```

### Action Response (`AdminActionResponse`)

```json
{
  "success": true,
  "message": "تمت العملية بنجاح",
  "data": null
}
```

### Error Response

```json
{
  "message": "وصف الخطأ"
}
```

### Model Validation Error (400)

```json
{
  "reason": [
    "سبب الرفض مطلوب",
    "يجب أن يكون سبب الرفض 10 أحرف على الأقل"
  ]
}
```

### Unauthorized (401)

```json
{
  "message": "Unauthorized"
}
```

### Forbidden (403)

Returned as HTTP 403 (empty body or `Forbid()` result).

### Not Found (404)

```json
{
  "message": "لم يتم العثور على المورد"
}
```

---

## Frontend Usage Notes

### Angular Service Pattern

Create a dedicated `AdminService` in Angular (e.g., `admin.service.ts`) that:
- Extends a base API service with the JWT interceptor already configured
- Uses `HttpClient` with `api/v1/admin` prefix

```typescript
@Injectable({ providedIn: 'root' })
export class AdminService {
  private baseUrl = 'api/v1/admin';

  constructor(private http: HttpClient) {}

  // Example method
  getPendingCraftsmen(page = 1, pageSize = 20, city?: string, serviceType?: string) {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
      .appendIfExists('city', city)
      .appendIfExists('serviceType', serviceType);
    return this.http.get<PagedResult<PendingCraftsmanDto>>(
      `${this.baseUrl}/craftsmen/pending`, { params }
    );
  }
}
```

### JWT Interceptor Behavior

- The `Authorization: Bearer {token}` header is added automatically by the Angular HTTP interceptor.
- The JWT **must** contain the role claim with value `"admin"`.
- If the token is missing, expired, or lacks the `admin` role, the server returns **401** or **403**.

### Pagination Handling

- All list endpoints return `PagedResult<T>` with `items`, `totalCount`, `page`, `pageSize`, `totalPages`.
- Angular signal shape:

```typescript
interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Suggested signal store
private craftsmen = signal<PagedResult<PendingCraftsmanDto> | null>(null);
private loading = signal(false);
```

### File Upload

- **Craftsmen image upload** (`POST /api/Craftsmen/{id}/upload-image`): `multipart/form-data` with a single `IFormFile` named `file`.
- **User profile image upload** (`POST /api/Users/profile/{id}/upload-image`): Same pattern.
- Recommended Angular approach:

```typescript
uploadImage(id: number, file: File): Observable<{ url: string }> {
  const formData = new FormData();
  formData.append('file', file);
  return this.http.post<{ url: string }>(
    `${this.baseUrl}/craftsmen/${id}/upload-image`, formData
  );
}
```

### CSV Export

- `GET /api/v1/admin/analytics/export?type=users` returns a Blob with `Content-Type: text/csv`.
- Angular download:

```typescript
downloadExport(type: string, from?: string, to?: string) {
  const params = new HttpParams()
    .set('type', type)
    .appendIfExists('from', from)
    .appendIfExists('to', to);
  return this.http.get(`${this.baseUrl}/analytics/export`, {
    params, responseType: 'blob'
  }).subscribe(blob => {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `harfi-${type}-${new Date().toISOString().slice(0,10)}.csv`;
    a.click();
  });
}
```

### State Management (Signals)

```typescript
// Feature-specific signals
export class AdminStore {
  // Craftsmen
  readonly pendingCraftsmen = signal<PagedResult<PendingCraftsmanDto> | null>(null);
  readonly approvedCraftsmen = signal<PagedResult<ApprovedCraftsmanDto> | null>(null);
  readonly selectedCraftsman = signal<CraftsmanDetailDto | null>(null);

  // Users
  readonly users = signal<PagedResult<UserAdminDto> | null>(null);
  readonly selectedUser = signal<UserAdminDetailDto | null>(null);

  // Jobs
  readonly jobs = signal<PagedResult<JobAdminDto> | null>(null);
  readonly selectedJob = signal<JobDetailDto | null>(null);

  // Analytics
  readonly overview = signal<AdminOverviewDto | null>(null);
  readonly craftsmanAnalytics = signal<CraftsmanAnalyticsDto | null>(null);
  readonly jobAnalytics = signal<JobAnalyticsDto | null>(null);
  readonly aiAnalytics = signal<AiAnalyticsDto | null>(null);
  readonly reviewAnalytics = signal<ReviewAnalyticsDto | null>(null);

  // General
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
}
```

---

*Generated from source code analysis — AdminController.cs, AIController.cs, AuthController.cs, and supporting DTOs.*
*Base URL: `/api/v1/admin` (except where noted otherwise).*
