# Harfi API — Complete Documentation
*Last updated: June 10, 2026*

## Quick Reference

| Detail | Value |
|--------|-------|
| **Base URL** | `http://localhost:5108/api/v1` (admin) / `http://localhost:5108/api` (all others) |
| **Authentication** | Bearer JWT token in `Authorization` header |
| **Content-Type** | `application/json` (unless noted otherwise) |
| **Encoding** | UTF-8 (all Arabic text) |
| **SignalR Hubs** | `/hubs/chat`, `/hubs/notifications` |
| **Static Files** | Served from `wwwroot/` at root URL |

### Auth Header Format
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Standard Error Shapes
```json
// 400 Bad Request
{ "message": "رسالة الخطأ بالعربية" }

// 401 Unauthorized
{ "message": "غير مصرح" }

// 403 Forbidden
// No body — empty response

// 404 Not Found
{ "message": "عذراً، هذا الحرفي غير موجود حالياً." }
```

---

## All Endpoints — Master Table

| # | Method | Path | Auth | Group | Description |
|---|--------|------|------|-------|-------------|
| 1 | POST | /api/auth/register | Public | Auth | Register new user |
| 2 | POST | /api/auth/login | Public | Auth | Login and get JWT |
| 3 | POST | /api/auth/refresh | Public | Auth | Refresh expired token |
| 4 | POST | /api/auth/logout | JWT | Auth | Logout and revoke token |
| 5 | GET | /api/auth/admin-only | Admin | Auth | Admin role test |
| 6 | GET | /api/auth/craftsman-only | Craftsman | Auth | Craftsman role test |
| 7 | GET | /api/auth/me | JWT | Auth | Get current user info |
| 8 | POST | /api/auth/verify-email | Public | Auth | Verify email with OTP |
| 9 | POST | /api/auth/resend-code | Public | Auth | Resend email verification |
| 10 | POST | /api/auth/send-phone-code | JWT | Auth | Send phone OTP |
| 11 | POST | /api/auth/verify-phone | JWT | Auth | Verify phone with OTP |
| 12 | POST | /api/auth/resend-phone-code | JWT | Auth | Resend phone OTP |
| 13 | POST | /api/craftsmen/register | Public | Craftsman | Submit craftsman application |
| 14 | GET | /api/craftsmen/{id} | Public | Customer | Get craftsman profile |
| 15 | GET | /api/craftsmen/search | Public | Customer | Search/filter craftsmen |
| 16 | PUT | /api/craftsmen/{id} | JWT | Craftsman | Update craftsman profile |
| 17 | POST | /api/craftsmen/{id}/upload-image | JWT | Craftsman | Upload profile image |
| 18 | POST | /api/jobs | Customer | Customer | Create new job |
| 19 | PUT | /api/jobs/{id}/accept | Craftsman | Craftsman | Accept a job |
| 20 | PUT | /api/jobs/{id}/reject | Craftsman | Craftsman | Reject a job |
| 21 | PUT | /api/jobs/{id}/complete | Craftsman | Craftsman | Mark job as complete |
| 22 | GET | /api/jobs/customer/{id} | JWT | Customer | Get customer's jobs |
| 23 | GET | /api/jobs/craftsman/{id} | JWT | Craftsman | Get craftsman's jobs |
| 24 | POST | /api/reviews | Customer | Customer | Submit review |
| 25 | GET | /api/reviews/craftsman/{craftsmanId} | Public | Customer | Get craftsman reviews |
| 26 | POST | /api/reviews/rag-feedback | JWT | Customer | Submit RAG feedback |
| 27 | POST | /api/conversations | JWT | Shared | Create conversation |
| 28 | GET | /api/conversations | JWT | Shared | List user conversations |
| 29 | GET | /api/conversations/{id} | JWT | Shared | Get conversation details |
| 30 | GET | /api/conversations/{id}/messages | JWT | Shared | Get conversation messages |
| 31 | PUT | /api/conversations/{id}/read | JWT | Shared | Mark conversation read |
| 32 | DELETE | /api/conversations/{id}/messages/{messageId} | JWT | Shared | Delete a message |
| 33 | POST | /api/conversations/upload-image | JWT | Shared | Upload chat image |
| 34 | POST | /api/conversations/upload-voice | JWT | Shared | Upload voice message |
| 35 | GET | /api/notifications | JWT | Shared | List user notifications |
| 36 | GET | /api/notifications/unread-count | JWT | Shared | Get unread count |
| 37 | PUT | /api/notifications/{id}/read | JWT | Shared | Mark notification read |
| 38 | PUT | /api/notifications/read-all | JWT | Shared | Mark all read |
| 39 | DELETE | /api/notifications/{id:int} | JWT | Shared | Delete notification |
| 40 | DELETE | /api/notifications/clear | JWT | Shared | Clear all notifications |
| 41 | GET | /api/users/profile/{id} | JWT | Shared | Get user profile |
| 42 | PUT | /api/users/profile/{id} | JWT | Shared | Update user profile |
| 43 | POST | /api/users/profile/{id}/upload-image | JWT | Shared | Upload profile image |
| 44 | GET | /api/v1/admin/craftsmen/pending | Admin | Admin | List pending craftsmen |
| 45 | GET | /api/v1/admin/craftsmen/approved | Admin | Admin | List approved craftsmen |
| 46 | GET | /api/v1/admin/craftsmen/rejected | Admin | Admin | List rejected craftsmen |
| 47 | GET | /api/v1/admin/craftsmen/{id} | Admin | Admin | Get craftsman details |
| 48 | PUT | /api/v1/admin/craftsmen/{id}/approve | Admin | Admin | Approve craftsman |
| 49 | PUT | /api/v1/admin/craftsmen/{id}/reject | Admin | Admin | Reject craftsman |
| 50 | PUT | /api/v1/admin/craftsmen/{id}/suspend | Admin | Admin | Suspend craftsman |
| 51 | DELETE | /api/v1/admin/craftsmen/{id} | Admin | Admin | Delete craftsman |
| 52 | GET | /api/v1/admin/users | Admin | Admin | List all users |
| 53 | GET | /api/v1/admin/users/{id} | Admin | Admin | Get user details |
| 54 | GET | /api/v1/admin/users/{id}/activity | Admin | Admin | Get user activity |
| 55 | PUT | /api/v1/admin/users/{id}/deactivate | Admin | Admin | Deactivate user |
| 56 | PUT | /api/v1/admin/users/{id}/reactivate | Admin | Admin | Reactivate user |
| 57 | DELETE | /api/v1/admin/users/{id} | Admin | Admin | Delete user |
| 58 | GET | /api/v1/admin/jobs | Admin | Admin | List all jobs |
| 59 | GET | /api/v1/admin/jobs/{id} | Admin | Admin | Get job details |
| 60 | PUT | /api/v1/admin/jobs/{id}/status | Admin | Admin | Update job status |
| 61 | PUT | /api/v1/admin/jobs/{id}/flag-dispute | Admin | Admin | Flag a dispute |
| 62 | PUT | /api/v1/admin/jobs/{id}/resolve-dispute | Admin | Admin | Resolve a dispute |
| 63 | GET | /api/v1/admin/jobs/{id}/chat-metadata | Admin | Admin | Get job chat metadata |
| 64 | GET | /api/v1/admin/jobs/{id}/chat-messages | Admin | Admin | Get job chat messages |
| 65 | GET | /api/v1/admin/reviews | Admin | Admin | List all reviews |
| 66 | GET | /api/v1/admin/reviews/{id} | Admin | Admin | Get review details |
| 67 | DELETE | /api/v1/admin/reviews/{id} | Admin | Admin | Delete review |
| 68 | GET | /api/v1/admin/reports | Admin | Admin | List reports |
| 69 | PUT | /api/v1/admin/reports/{id}/resolve | Admin | Admin | Resolve report |
| 70 | GET | /api/v1/admin/ai-logs | Admin | Admin | List AI chat logs |
| 71 | GET | /api/v1/admin/analytics/overview | Admin | Admin | Platform overview |
| 72 | GET | /api/v1/admin/analytics/craftsmen | Admin | Admin | Craftsman analytics |
| 73 | GET | /api/v1/admin/analytics/jobs | Admin | Admin | Job analytics |
| 74 | GET | /api/v1/admin/analytics/ai | Admin | Admin | AI analytics |
| 75 | GET | /api/v1/admin/analytics/reviews | Admin | Admin | Review analytics |
| 76 | GET | /api/v1/admin/analytics/export | Admin | Admin | Export data as CSV |
| 77 | GET | /api/v1/admin/config/service-types | Admin | Admin | List service types |
| 78 | POST | /api/v1/admin/config/service-types | Admin | Admin | Create service type |
| 79 | PUT | /api/v1/admin/config/service-types/{id} | Admin | Admin | Update service type |
| 80 | DELETE | /api/v1/admin/config/service-types/{id} | Admin | Admin | Delete service type |
| 81 | GET | /api/v1/admin/config/cities | Admin | Admin | List cities |
| 82 | POST | /api/v1/admin/config/cities | Admin | Admin | Create city |
| 83 | PUT | /api/v1/admin/config/cities/{id} | Admin | Admin | Update city |
| 84 | DELETE | /api/v1/admin/config/cities/{id} | Admin | Admin | Delete city |
| 85 | GET | /api/v1/admin/config/feature-flags | Admin | Admin | List feature flags |
| 86 | PUT | /api/v1/admin/config/feature-flags/{key} | Admin | Admin | Update feature flag |
| 87 | GET | /api/v1/admin/audit-logs | Admin | Admin | List audit logs |
| 88 | GET | /api/v1/admin/audit-logs/{id} | Admin | Admin | Get audit log detail |
| 89 | GET | /api/AI/welcome | Public | AI | Welcome message |
| 90 | POST | /api/AI/chat3 | Public | AI | Multi-turn AI chat |
| 91 | POST | /api/AI/ingest/craftsmen | Admin | AI | Ingest craftsmen to Qdrant |
| 92 | POST | /api/AI/ingest/jobs | Admin | AI | Ingest jobs to Qdrant |
| 93 | POST | /api/AI/ingest/job/{jobId} | Public | AI | Ingest single job |
| 94 | GET | /api/AI/vectors/count | Admin | AI | Count vectors in Qdrant |
| 95 | POST | /api/AI/analyze-media | Public | AI | Analyze image/audio |
| 96 | GET | /api/AI/sessions/{userId} | Public | AI | List AI chat sessions |
| 97 | GET | /api/AI/sessions/{userId}/{sessionId} | Public | AI | Get session messages |
| 98 | POST | /api/AI/sessions/message | Public | AI | Save AI chat message |
| 99 | DELETE | /api/AI/sessions/{userId}/{sessionId} | Public | AI | Delete session |
| 100 | POST | /api/AI/craftsman/check-and-submit-solution | Public | AI | Verify & submit solution |

**Total: 100 HTTP endpoints + 2 SignalR hubs**

---

## Group 1 — Admin Endpoints
> All admin endpoints are prefixed with `/api/v1/admin` and require `Authorization: Bearer {token}` with an admin role JWT.

---

### 1. GET /api/v1/admin/craftsmen/pending

**What it does:** Returns a paginated list of craftsmen who have registered but not yet been approved.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Pending Craftsmen page (the main page admin sees after login).

---

#### Request

**Route Parameters:** None

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| page | int | No | 1 | Which page of results |
| pageSize | int | No | 20 | Results per page |
| city | string | No | null | Filter by city name |
| serviceType | string | No | null | Filter by service type |

**Request Body:** None — no request body needed

---

#### Response

**Success Response — 200 OK**
```json
{
  "items": [
    {
      "id": 1,
      "userId": 26,
      "fullName": "أحمد علي",
      "email": "ahmed.ali@gmail.com",
      "phone": "01000000001",
      "serviceType": "سباكة",
      "city": "القاهرة",
      "neighborhood": "مدينة نصر",
      "experience": 3,
      "nationalIdUrl": "/uploads/ids/id_1.jpg",
      "bio": "سباك متخصص في تركيب وصيانة شبكات المياه والصرف الصحي",
      "createdAt": "2026-01-10T00:00:00Z"
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

**Possible Error Responses:**
| Status | When | Body |
|--------|------|------|
| 401 | No token or invalid token | `{ "message": "غير مصرح" }` |

---

#### How It Works
1. Request arrives at `AdminController.GetPendingCraftsmen()`
2. JWT authentication validates the admin token
3. Controller calls `_adminService.GetPendingCraftsmenAsync(page, pageSize, city, serviceType)`
4. Service queries `Craftsmen` table filtered by `IsApproved == false && IsDeleted == false`, includes `User` data
5. Applies city/serviceType filters if provided
6. Returns a `PagedResult<PendingCraftsmanDto>`

---

#### Frontend Integration Notes
- Endpoint is paginated — use the `page`, `totalPages`, and `totalCount` fields to build pagination controls
- Each craftsman has a `nationalIdUrl` — render this as a clickable link to view the uploaded ID image
- The `bio` field can be long (up to 1000 chars) — consider truncating with a "Read More" link

---

### 2. GET /api/v1/admin/craftsmen/approved

**What it does:** Returns a paginated list of approved craftsmen available on the platform.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Approved Craftsmen page.

---

#### Request

**Route Parameters:** None

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| page | int | No | 1 | Which page |
| pageSize | int | No | 20 | Results per page |
| city | string | No | null | Filter by city |
| serviceType | string | No | null | Filter by service type |
| minRating | decimal | No | null | Minimum rating filter (0-5) |

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "items": [
    {
      "id": 2,
      "userId": 27,
      "fullName": "محمد حسن",
      "email": "mohamed.hassan@gmail.com",
      "phone": "01000000002",
      "serviceType": "كهرباء",
      "city": "الإسكندرية",
      "neighborhood": "سيدي بشر",
      "experience": 12,
      "rating": 4.5,
      "isAvailable": true,
      "bio": "مهندس كهربائي خبرة 12 سنة",
      "createdAt": "2026-01-10T00:00:00Z"
    }
  ],
  "totalCount": 15,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

### 3. GET /api/v1/admin/craftsmen/rejected

**What it does:** Returns a paginated list of craftsmen whose registration was rejected.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Rejected Craftsmen page.

---

#### Request

**Query Parameters:** `page` (int, default 1), `pageSize` (int, default 20)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "items": [
    {
      "id": 5,
      "userId": 31,
      "fullName": "حسين رضا",
      "email": "hussien.reda@gmail.com",
      "phone": "01000000005",
      "serviceType": "تكييف وتبريد",
      "city": "طنطا",
      "rejectionReason": "بيانات الهوية الوطنية غير واضحة",
      "createdAt": "2026-01-10T00:00:00Z",
      "deletedAt": "2026-01-13T00:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

### 4. GET /api/v1/admin/craftsmen/{id}

**What it does:** Returns complete details for a single craftsman, including review stats.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Craftsman Detail page (when admin clicks on a craftsman).

---

#### Request

**Route Parameters:** `id` (int, required) — the craftsman's ID

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 2,
  "userId": 27,
  "fullName": "محمد حسن",
  "email": "mohamed.hassan@gmail.com",
  "phone": "01000000002",
  "profileImageUrl": null,
  "serviceType": "كهرباء",
  "city": "الإسكندرية",
  "neighborhood": "سيدي بشر",
  "priceRangeMin": 200.00,
  "priceRangeMax": 600.00,
  "experience": 12,
  "isApproved": true,
  "isAvailable": true,
  "isDeleted": false,
  "rating": 4.5,
  "bio": "مهندس كهربائي خبرة 12 سنة في تمديد الكهرباء والصيانة الشاملة",
  "nationalIdUrl": "/uploads/ids/id_2.jpg",
  "rejectionReason": null,
  "deletionReason": null,
  "createdAt": "2026-01-10T00:00:00Z",
  "updatedAt": "2026-01-10T00:00:00Z",
  "completedJobsCount": 8,
  "totalReviews": 5
}
```

**Error Responses:**
| Status | When | Body |
|--------|------|------|
| 404 | Craftsman not found | `{ "message": "الحرفي غير موجود" }` |

---

### 5. PUT /api/v1/admin/craftsmen/{id}/approve

**What it does:** Approves a pending craftsman's registration, making them visible to customers.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Pending Craftsmen → "Approve" button.

---

#### Request

**Route Parameters:** `id` (int, required) — the craftsman ID to approve

**Request Body:**
```json
{
  "notifyMessage": "optional custom message to send to the craftsman"
}
```
All fields are optional. The body can also be empty `{}`.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم اعتماد الحرفي بنجاح"
}
```

---

### 6. PUT /api/v1/admin/craftsmen/{id}/reject

**What it does:** Rejects a pending craftsman's registration with a required reason.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Pending Craftsmen → "Reject" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "reason": "بيانات الهوية الوطنية غير واضحة — يرجى إعادة رفع صورة واضحة"
}
```
`reason` is required, min 10 chars, max 500 chars.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم رفض طلب الحرفي"
}
```

**Error Responses:**
| 400 | Validation failed (reason too short) | `{ "message": { "reason": ["يجب أن يكون سبب الرفض 10 أحرف على الأقل"] } }` |

---

### 7. PUT /api/v1/admin/craftsmen/{id}/suspend

**What it does:** Temporarily suspends an approved craftsman, making them unavailable for new jobs.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Craftsman Detail → "Suspend" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "reason": "شكاوى متعددة من العملاء بخصوص التأخير المتكرر"
}
```
`reason` is required, min 10 chars, max 500 chars.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم تعليق الحرفي"
}
```

---

### 8. DELETE /api/v1/admin/craftsmen/{id}

**What it does:** Permanently removes a craftsman from the platform. This is a soft delete — the record stays in the database.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Craftsman Detail → "Delete" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "reason": "مخالفة شروط الاستخدام — تسجيل وهمي"
}
```
`reason` is required, min 10 chars, max 500 chars.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم حذف الحرفي"
}
```

---

### 9. GET /api/v1/admin/users

**What it does:** Returns a paginated list of all platform users (customers and craftsmen) with optional filters.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → User Management page.

---

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| role | string | No | null | Filter by role: "customer", "craftsman", or "admin" |
| page | int | No | 1 | Which page |
| pageSize | int | No | 20 | Results per page |
| isActive | bool | No | null | Filter by active/inactive status |
| search | string | No | null | Search by name or email |

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "items": [
    {
      "id": 1,
      "name": "مريم أحمد",
      "email": "mariam.ahmed@gmail.com",
      "phone": "01000000001",
      "role": "customer",
      "isActive": true,
      "isVerified": true,
      "isDeleted": false,
      "profileImageUrl": null,
      "createdAt": "2026-01-10T00:00:00Z"
    }
  ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

---

### 10. GET /api/v1/admin/users/{id}

**What it does:** Returns detailed information about a specific user, including their craftsman profile ID and job/review counts.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → User Detail page.

---

#### Request

**Route Parameters:** `id` (int, required) — the user's ID

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 26,
  "name": "أحمد علي",
  "email": "ahmed.ali@gmail.com",
  "phone": "01000000001",
  "role": "craftsman",
  "isActive": true,
  "isVerified": true,
  "isDeleted": false,
  "profileImageUrl": null,
  "deletionReason": null,
  "deletedAt": null,
  "createdAt": "2026-01-10T00:00:00Z",
  "craftsmanProfileId": 1,
  "jobsCount": 5,
  "reviewsCount": 3
}
```

**Error Responses:**
| 404 | User not found | `{ "message": "المستخدم غير موجود" }` |

---

### 11. GET /api/v1/admin/users/{id}/activity

**What it does:** Returns recent activity history for a specific user (login, jobs created, conversations, etc.)

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → User Detail → Activity tab.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "action": "إنشاء حساب",
    "details": "تم إنشاء الحساب بنجاح",
    "timestamp": "2026-01-10T00:00:00Z"
  },
  {
    "action": "تسجيل دخول",
    "details": "آخر تسجيل دخول",
    "timestamp": "2026-06-01T12:00:00Z"
  }
]
```

**Error Responses:**
| 404 | User not found | `{ "message": "المستخدم غير موجود" }` |

---

### 12. PUT /api/v1/admin/users/{id}/deactivate

**What it does:** Deactivates a user account, preventing them from logging in.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → User Detail → "Deactivate" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "reason": "نشاط مشبوه ومخالفة شروط الاستخدام"
}
```
`reason` is required, min 10 chars, max 500 chars.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم تعطيل المستخدم"
}
```

**Error Responses:**
| 400 | Cannot deactivate admin | `{ "message": "لا يمكن تعطيل حساب أدمن" }` |

---

### 13. PUT /api/v1/admin/users/{id}/reactivate

**What it does:** Reactivates a previously deactivated user account.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → User Detail → "Reactivate" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم إعادة تفعيل المستخدم"
}
```

| 404 | User not found | `{ "message": "المستخدم غير موجود" }` |

---

### 14. DELETE /api/v1/admin/users/{id}

**What it does:** Soft-deletes a user account, hiding their data from the platform.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → User Detail → "Delete User" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "reason": "طلب العميل حذف حسابه"
}
```
`reason` is required, min 10 chars, max 500 chars.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم حذف المستخدم"
}
```

| 400 | Cannot delete admin | `{ "message": "لا يمكن حذف حساب أدمن" }` |

---

### 15. GET /api/v1/admin/jobs

**What it does:** Returns a paginated list of all jobs on the platform with filtering options.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Jobs Management page.

---

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| status | string | No | null | Filter by status |
| craftsmanId | int | No | null | Filter by craftsman |
| customerId | int | No | null | Filter by customer |
| page | int | No | 1 | Which page |
| pageSize | int | No | 20 | Results per page |
| from | DateTime | No | null | Filter by created date from |
| to | DateTime | No | null | Filter by created date to |

**Note:** Status values in Arabic: `مفتوح` (Open), `قيد التنفيذ` (In Progress), `مكتمل` (Completed), `مرفوض` (Rejected), `ملغى` (Cancelled)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "items": [
    {
      "id": 1,
      "customerId": 2,
      "customerName": "سارة أحمد",
      "craftsmanId": 2,
      "craftsmanName": "محمد حسن",
      "status": "مكتمل",
      "serviceType": "كهرباء",
      "description": "المفاتيح في الصالة بتشرر وفيه رائحة احتراق",
      "address": "15 شارع التحرير، الإسكندرية",
      "isDisputed": false,
      "createdAt": "2026-01-10T00:00:00Z",
      "completedAt": "2026-01-15T00:00:00Z"
    }
  ],
  "totalCount": 80,
  "page": 1,
  "pageSize": 20,
  "totalPages": 4
}
```

---

### 16. GET /api/v1/admin/jobs/{id}

**What it does:** Returns full details of a single job, including dispute information.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Jobs → Job Detail page.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 1,
  "customerId": 2,
  "customerName": "سارة أحمد",
  "craftsmanId": 2,
  "craftsmanName": "محمد حسن",
  "status": "مكتمل",
  "serviceType": "كهرباء",
  "description": "المفاتيح في الصالة بتشرر وفيه رائحة احتراق",
  "address": "15 شارع التحرير، الإسكندرية",
  "preferredDate": null,
  "problemImageUrl": null,
  "problemDescription": "شرار من المفاتيح الكهربائية مع رائحة بلاستيك محترق",
  "solutionDescription": "تم استبدال المفاتيح والتوصيلات",
  "isDisputed": true,
  "disputeRaisedAt": "2026-03-15T00:00:00Z",
  "disputeResolvedAt": null,
  "disputeResolution": "نزاع مرفوع — جاري المراجعة من الإدارة",
  "createdAt": "2026-01-10T00:00:00Z",
  "completedAt": "2026-01-15T00:00:00Z",
  "updatedAt": "2026-03-15T00:00:00Z"
}
```

| 404 | Job not found | `{ "message": "الوظيفة غير موجودة" }` |

---

### 17. PUT /api/v1/admin/jobs/{id}/status

**What it does:** Manually updates the status of a job by an admin.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Job Detail → "Update Status" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "status": "مكتمل",
  "justification": "تم التأكد من إنجاز العمل بناءً على تواصلنا مع الطرفين"
}
```
`justification` is required.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم تحديث حالة الوظيفة"
}
```

---

### 18. PUT /api/v1/admin/jobs/{id}/flag-dispute

**What it does:** Flags a job as disputed, initiating the dispute resolution process.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Job Detail → "Flag Dispute" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "reason": "العميل يشتكي من جودة العمل ويرفض الدفع"
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم الإبلاغ عن النزاع"
}
```

---

### 19. PUT /api/v1/admin/jobs/{id}/resolve-dispute

**What it does:** Resolves a disputed job, recording the resolution and favored party.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Job Detail → "Resolve Dispute" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "resolution": "تم حل الموضوع بالتراضي — استرداد 30% للعميل",
  "favoredParty": "customer"
}
```
`favoredParty` values: `"customer"` or `"craftsman"`

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم حل النزاع"
}
```

---

### 20. GET /api/v1/admin/jobs/{id}/chat-metadata

**What it does:** Returns metadata about the conversation associated with a job.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Job Detail → "View Chat" section.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "conversationId": 5,
  "jobId": 1,
  "customerName": "سارة أحمد",
  "craftsmanName": "محمد حسن",
  "messageCount": 12,
  "createdAt": "2026-01-10T00:00:00Z",
  "lastMessageAt": "2026-01-14T15:30:00Z"
}
```

| 404 | No conversation found | `{ "message": "لا توجد محادثة" }` |

---

### 21. GET /api/v1/admin/jobs/{id}/chat-messages

**What it does:** Returns all messages in the conversation for a specific job, for admin review.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Job Detail → Chat Messages panel.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "id": 1,
    "conversationId": 5,
    "senderId": 2,
    "senderName": "سارة أحمد",
    "senderAvatar": null,
    "content": "أهلاً، محتاج حد يصلح التسريب في الحمام بسرعة",
    "messageType": "text",
    "isRead": true,
    "sentAt": "2026-01-10T10:00:00Z"
  }
]
```

| 403 | Admin doesn't have permission to view | `{ "message": "غير مصرح" }` |
| 404 | Job not found | `{ "message": "الوظيفة غير موجودة" }` |

---

### 22. GET /api/v1/admin/reviews

**What it does:** Returns a paginated list of all reviews across the platform.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Reviews Moderation page.

---

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| craftsmanId | int | No | null | Filter by craftsman |
| minStars | int | No | null | Minimum star rating (1-5) |
| maxStars | int | No | null | Maximum star rating (1-5) |
| page | int | No | 1 | Which page |
| pageSize | int | No | 20 | Results per page |

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "items": [
    {
      "id": 1,
      "jobId": 3,
      "customerId": 5,
      "customerName": "نورhan محمد",
      "craftsmanId": 1,
      "craftsmanName": "محمد حسن",
      "stars": 5,
      "comment": "شغل محترف ونظافة بعد الشغل.",
      "isDeleted": false,
      "createdAt": "2026-01-12T00:00:00Z"
    }
  ],
  "totalCount": 40,
  "page": 1,
  "pageSize": 20,
  "totalPages": 2
}
```

---

### 23. GET /api/v1/admin/reviews/{id}

**What it does:** Returns detailed information about a specific review, including deletion info.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Reviews → Review Detail.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 1,
  "jobId": 3,
  "jobDescription": "صيانة دورية شاملة للتوصيلات الكهربائية",
  "customerId": 5,
  "customerName": "نورhan محمد",
  "craftsmanId": 1,
  "craftsmanName": "محمد حسن",
  "stars": 5,
  "comment": "شغل محترف ونظافة بعد الشغل.",
  "isDeleted": false,
  "deletionReason": null,
  "deletedAt": null,
  "createdAt": "2026-01-12T00:00:00Z"
}
```

| 404 | Review not found | `{ "message": "التقييم غير موجود" }` |

---

### 24. DELETE /api/v1/admin/reviews/{id}

**What it does:** Soft-deletes a review for policy violation.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Reviews → "Delete Review" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "reason": "التقييم يحتوي على ألفاظ مسيئة تخالف شروط الاستخدام"
}
```
`reason` is required.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم حذف التقييم"
}
```

| 404 | Review not found | `{ "message": "التقييم غير موجود" }` |

---

### 25. GET /api/v1/admin/reports

**What it does:** Returns a paginated list of user-submitted reports.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Reports Management page.

---

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| status | string | No | null | Filter by "pending" or "resolved" |
| type | string | No | null | Filter by target type |
| page | int | No | 1 | Which page |
| pageSize | int | No | 20 | Results per page |

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "items": [
    {
      "id": 1,
      "reportedByUserId": 5,
      "reportedByUserName": "نورhan محمد",
      "targetType": "Craftsman",
      "targetId": 1,
      "reason": "الحرفي استخدم مواد رديئة وخالف شروط العقد",
      "status": "resolved",
      "resolvedByAdminId": 1,
      "resolutionNotes": "تم التحقق وتوجيه تحذير",
      "createdAt": "2026-03-10T00:00:00Z",
      "resolvedAt": "2026-03-12T00:00:00Z"
    }
  ],
  "totalCount": 10,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

### 26. PUT /api/v1/admin/reports/{id}/resolve

**What it does:** Resolves a user report by taking action and adding notes.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Reports → "Resolve" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "action": "warning",
  "notes": "تم توجيه إنذار للحرفي"
}
```
`action` values: `"warning"`, `"dismiss"`, `"ban"`

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم حل البلاغ"
}
```

---

### 27. GET /api/v1/admin/ai-logs

**What it does:** Returns a paginated list of AI chat message logs.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → AI Logs page.

---

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| page | int | No | 1 | Which page |
| pageSize | int | No | 20 | Results per page |
| from | DateTime | No | null | Filter by date from |
| to | DateTime | No | null | Filter by date to |

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "items": [
    {
      "id": 1,
      "userId": 5,
      "userName": "نورhan محمد",
      "sessionId": "3f139bce-256e-45e4-9583-f3d80a09d895",
      "role": "user",
      "content": "عاوز سباك في القاهرة",
      "toolUsed": null,
      "tokensUsed": 150,
      "createdAt": "2026-06-10T12:00:00Z"
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

---

### 28. GET /api/v1/admin/analytics/overview

**What it does:** Returns a high-level platform overview with key metrics.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Analytics → Overview tab (home page).

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "totalUsers": 50,
  "totalCraftsmen": 20,
  "pendingCraftsmen": 2,
  "activeJobs": 15,
  "completedJobs": 45,
  "disputedJobs": 3,
  "pendingReports": 2,
  "totalReviews": 40,
  "newUsersThisMonth": 12,
  "averageRating": 4.2
}
```

---

### 29. GET /api/v1/admin/analytics/craftsmen

**What it does:** Returns craftsman-specific analytics with breakdowns by service type and city.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Analytics → Craftsmen tab.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "totalCraftsmen": 20,
  "pendingApproval": 2,
  "approved": 16,
  "rejected": 1,
  "suspended": 1,
  "averageRating": 4.0,
  "byServiceType": {
    "سباكة": 3,
    "كهرباء": 4,
    "دهانات": 2,
    "نجارة": 2
  },
  "byCity": {
    "القاهرة": 5,
    "الإسكندرية": 3,
    "الجيزة": 2
  }
}
```

---

### 30. GET /api/v1/admin/analytics/jobs

**What it does:** Returns job analytics with status breakdowns and average completion time.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Analytics → Jobs tab.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "totalJobs": 72,
  "open": 10,
  "inProgress": 5,
  "completed": 45,
  "rejected": 8,
  "disputed": 4,
  "byServiceType": {
    "كهرباء": 12,
    "سباكة": 8,
    "دهانات": 6
  },
  "averageCompletionDays": 4.5
}
```

---

### 31. GET /api/v1/admin/analytics/ai

**What it does:** Returns AI assistant usage analytics.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Analytics → AI tab.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "totalChats": 96,
  "totalTokensUsed": 25000,
  "totalCraftsmenIngested": 20,
  "totalSolutionsIngested": 10,
  "averageTokensPerChat": 260
}
```

---

### 32. GET /api/v1/admin/analytics/reviews

**What it does:** Returns review analytics including star distribution.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Analytics → Reviews tab.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "totalReviews": 40,
  "averageStars": 4.2,
  "starDistribution": {
    "1": 2,
    "2": 5,
    "3": 8,
    "4": 12,
    "5": 13
  },
  "deletedReviews": 1
}
```

---

### 33. GET /api/v1/admin/analytics/export

**What it does:** Exports platform data as a CSV file for download.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Analytics → "Export Data" button.

---

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| type | string | No | "users" | Export type: "users", "craftsmen", "jobs", "reviews" |
| from | DateTime | No | null | Filter from date |
| to | DateTime | No | null | Filter to date |

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
The response is a CSV file download:
```
Content-Type: text/csv
Content-Disposition: attachment; filename=harfi-users-20260610.csv

Id,Name,Email,Role,IsActive,CreatedAt
1,مريم أحمد,mariam.ahmed@gmail.com,customer,true,2026-01-10
```

| 400 | Invalid export type | `{ "message": "نوع التصدير غير صالح" }` |

---

### 34. GET /api/v1/admin/config/service-types

**What it does:** Returns all service types (e.g. سباكة, كهرباء, دهانات) used on the platform.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Service Types section.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "id": 1,
    "nameAr": "سباكة",
    "nameEn": "Plumbing",
    "icon": null,
    "isActive": true
  },
  {
    "id": 2,
    "nameAr": "كهرباء",
    "nameEn": "Electrical",
    "icon": null,
    "isActive": true
  }
]
```

---

### 35. POST /api/v1/admin/config/service-types

**What it does:** Creates a new service type.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Service Types → "Add New" button.

---

#### Request

**Request Body:**
```json
{
  "nameAr": "تبريد وتكييف",
  "nameEn": "Cooling & AC",
  "icon": "fas fa-snowflake",
  "isActive": true
}
```
`nameAr` and `nameEn` are required.

---

#### Response

**Success Response — 201 Created**
```json
{
  "id": 16,
  "nameAr": "تبريد وتكييف",
  "nameEn": "Cooling & AC",
  "icon": "fas fa-snowflake",
  "isActive": true
}
```

---

### 36. PUT /api/v1/admin/config/service-types/{id}

**What it does:** Updates an existing service type.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Service Types → Edit.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "nameAr": "تبريد وتكييف",
  "nameEn": "Cooling & AC",
  "icon": "fas fa-snowflake",
  "isActive": true
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 16,
  "nameAr": "تبريد وتكييف",
  "nameEn": "Cooling & AC",
  "icon": "fas fa-snowflake",
  "isActive": true
}
```

| 404 | Service type not found | `{ "message": "نوع الخدمة غير موجود" }` |

---

### 37. DELETE /api/v1/admin/config/service-types/{id}

**What it does:** Deletes a service type.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Service Types → Delete.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم حذف نوع الخدمة"
}
```

| 404 | Service type not found | `{ "message": "نوع الخدمة غير موجود" }` |

---

### 38. GET /api/v1/admin/config/cities

**What it does:** Returns all cities on the platform.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Cities section.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
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

### 39. POST /api/v1/admin/config/cities

**What it does:** Creates a new city.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Cities → "Add New" button.

---

#### Request

**Request Body:**
```json
{
  "nameAr": "الغردقة",
  "nameEn": "Hurghada",
  "governorate": "البحر الأحمر",
  "isActive": true
}
```
`nameAr` and `nameEn` are required.

---

#### Response

**Success Response — 201 Created**
```json
{
  "id": 19,
  "nameAr": "الغردقة",
  "nameEn": "Hurghada",
  "governorate": "البحر الأحمر",
  "isActive": true
}
```

---

### 40. PUT /api/v1/admin/config/cities/{id}

**What it does:** Updates an existing city.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "nameAr": "الغردقة",
  "nameEn": "Hurghada",
  "governorate": "البحر الأحمر",
  "isActive": true
}
```

---

#### Response

**Success Response — 200 OK** — Returns the updated CityDto

| 404 | City not found | `{ "message": "المدينة غير موجودة" }` |

---

### 41. DELETE /api/v1/admin/config/cities/{id}

**What it does:** Deletes a city.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم حذف المدينة"
}
```

| 404 | City not found | `{ "message": "المدينة غير موجودة" }` |

---

### 42. GET /api/v1/admin/config/feature-flags

**What it does:** Returns all feature flags that control platform features.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Feature Flags.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "key": "SelfFixGuideEnabled",
    "isEnabled": true,
    "updatedAt": "2026-04-10T00:00:00Z"
  },
  {
    "key": "VoiceSearchEnabled",
    "isEnabled": false,
    "updatedAt": "2026-04-10T00:00:00Z"
  }
]
```

---

### 43. PUT /api/v1/admin/config/feature-flags/{key}

**What it does:** Enables or disables a feature flag.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Feature Flags → Toggle switch.

---

#### Request

**Route Parameters:** `key` (string, required) — the flag key e.g. "SelfFixGuideEnabled"

**Request Body:**
```json
{
  "isEnabled": true
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم تحديث الخاصية"
}
```

---

### 44. GET /api/v1/admin/audit-logs

**What it does:** Returns a paginated list of all admin actions for auditing.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Audit Logs page.

---

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| adminId | int | No | null | Filter by admin |
| action | string | No | null | Filter by action type |
| targetType | string | No | null | Filter by target (User, Craftsman, Job, etc.) |
| from | DateTime | No | null | From date |
| to | DateTime | No | null | To date |
| page | int | No | 1 | Which page |
| pageSize | int | No | 20 | Per page |

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "items": [
    {
      "id": 1,
      "adminId": 1,
      "adminName": "أدمن",
      "action": "اعتماد حرفي",
      "targetType": "Craftsman",
      "targetId": 2,
      "notes": "تم اعتماد الحرفي محمد حسن",
      "ipAddress": "192.168.1.1",
      "createdAt": "2026-01-11T00:00:00Z"
    }
  ],
  "totalCount": 31,
  "page": 1,
  "pageSize": 20,
  "totalPages": 2
}
```

---

### 45. GET /api/v1/admin/audit-logs/{id}

**What it does:** Returns a single audit log entry.

**Who uses it:** Admin

**Authorizarion:** Requires Role: Admin

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK** — Returns the same shape as a single item in the list above.

| 404 | Audit log not found | `{ "message": "سجل المراجعة غير موجود" }` |

---

## Group 2 — Customer Endpoints

---

### 1. POST /api/auth/register

**What it does:** Creates a new user account (customer or craftsman).

**Who uses it:** Public (no login required)

**Authorization:** Public

**Where to use it in the frontend:** Registration page → "Create Account" form.

---

#### Request

**Request Body:**
```json
{
  "name": "أحمد محمد",
  "email": "ahmed.mohamed@example.com",
  "password": "password123",
  "confirmPassword": "password123",
  "role": "customer",
  "phone": "01012345678"
}
```

**Field Validation:**
| Field | Rules |
|-------|-------|
| name | Required, max 100 chars |
| email | Required, valid email format, max 200 chars |
| password | Required, min 8 chars |
| confirmPassword | Required, must match password |
| role | Required, must be "customer" or "craftsman" (not "admin") |
| phone | Optional, valid phone format, max 20 chars |

---

#### Response

**Success Response — 201 Created**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "expiresAt": "2026-06-10T14:00:00Z",
  "user": {
    "id": 51,
    "name": "أحمد محمد",
    "email": "ahmed.mohamed@example.com",
    "role": "customer",
    "phone": "01012345678",
    "profileImageUrl": null,
    "craftsmanId": null
  },
  "requiresPhoneVerification": false
}
```

**Error Responses:**
| 400 | Email already exists | `{ "message": "البريد الإلكتروني مسجل بالفعل" }` |
| 400 | Validation failed | `{ "message": { "Email": ["صيغة البريد الإلكتروني غير صحيحة"] } }` |
| 400 | Admin registration blocked | `{ "message": "لا يمكن تسجيل حساب أدمن من خلال API التسجيل." }` |

---

#### How It Works
1. Request arrives at `AuthController.Register()`
2. If role is "admin", returns 400 — admin accounts are created only via seed
3. Controller calls `_authService.RegisterAsync(dto)`
4. Service validates email uniqueness in `Users` table
5. Creates `User` entity with Identity, generates email verification code
6. Generates JWT access token + refresh token
7. Returns 201 with tokens and user info

---

#### Frontend Integration Notes
- Store the `accessToken` in `localStorage` or `sessionStorage`
- Store the `refreshToken` securely — needed for token refresh
- `expiresAt` tells you when the access token expires (default: 60 minutes)
- Send `Authorization: Bearer {accessToken}` in all subsequent requests
- If `role === "craftsman"`, redirect to craftsman registration form to fill profile details
- If `role === "customer"`, the account is ready to use immediately
- A 6-digit verification code is sent to the user's email — verify before proceeding

---

### 2. POST /api/auth/login

**What it does:** Authenticates a user and returns a JWT token.

**Who uses it:** Public

**Authorization:** Public

**Where to use it in the frontend:** Login page.

---

#### Request

**Request Body:**
```json
{
  "email": "ahmed.mohamed@example.com",
  "password": "password123"
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "expiresAt": "2026-06-10T14:00:00Z",
  "user": {
    "id": 51,
    "name": "أحمد محمد",
    "email": "ahmed.mohamed@example.com",
    "role": "customer",
    "phone": "01012345678",
    "profileImageUrl": null,
    "craftsmanId": null
  },
  "requiresPhoneVerification": false
}
```

| 401 | Invalid credentials | `{ "message": "البريد الإلكتروني أو كلمة المرور غير صحيحة" }` |
| 401 | Account deactivated | `{ "message": "تم تعطيل حسابك" }` |
| 400 | Email not verified | `{ "message": "يرجى تفعيل البريد الإلكتروني أولاً" }` |

---

#### How It Works
1. `AuthController.Login()` → `_authService.LoginAsync(dto)`
2. Service checks if user exists, is active, and email is verified
3. Validates password using `SignInManager.CheckPasswordSignInAsync()`
4. Generates JWT with claims: `NameIdentifier` (userId), `Name`, `Email`, `Role`
5. Creates refresh token in `RefreshTokens` table
6. Returns tokens and user info

---

### 3. POST /api/auth/refresh

**What it does:** Gets a new access token using a valid refresh token.

**Who uses it:** Public

**Authorization:** Public

**Where to use it in the frontend:** When the access token expires (check `expiresAt`), use this to get a new one without asking the user to re-login.

---

#### Request

**Request Body:**
```json
{
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "new-refresh-token-here",
  "expiresAt": "2026-06-10T15:00:00Z",
  "user": { ... }
}
```

| 401 | Invalid or expired refresh token | `{ "message": "رمز التحديث غير صالح أو منتهي" }` |

---

### 4. POST /api/auth/logout

**What it does:** Logs out the user by revoking their refresh token and removing SignalR connections.

**Who uses it:** Any authenticated user

**Authorization:** Requires Login

**Where to use it in the frontend:** User menu → "Logout" button.

---

#### Request

**Request Body:**
```json
{
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "message": "تم تسجيل الخروج بنجاح"
}
```

**Side effects:** Also deletes all SignalR `UserConnection` records for this user and broadcasts `UserOffline` to all SignalR clients.

---

### 5. GET /api/auth/me

**What it does:** Returns the current authenticated user's info from the JWT token.

**Who uses it:** Any authenticated user

**Authorization:** Requires Login

**Where to use it in the frontend:** On app initialization to verify the token is still valid and get the user's ID, name, role, and craftsman ID.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": "51",
  "name": "أحمد محمد",
  "email": "ahmed.mohamed@example.com",
  "role": "craftsman",
  "craftsmanId": 1
}
```

**Note:** `craftsmanId` is only present if the user's role is `craftsman`. For customers it will be `null`.

---

### 6. POST /api/auth/verify-email

**What it does:** Verifies the user's email address using the 6-digit code sent after registration.

**Who uses it:** Public

**Authorization:** Public

**Where to use it in the frontend:** Email verification page (shown after registration).

---

#### Request

**Request Body:**
```json
{
  "email": "ahmed.mohamed@example.com",
  "code": "482913"
}
```
`code` must be exactly 6 characters.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم تفعيل البريد الإلكتروني بنجاح"
}
```

| 400 | Invalid or expired code | `{ "message": "الكود غير صالح أو منتهي الصلاحية" }` |

---

### 7. POST /api/auth/resend-code

**What it does:** Resends the email verification code.

**Who uses it:** Public

**Authorization:** Public

**Where to use it in the frontend:** Email verification page → "Resend Code" link.

---

#### Request

**Request Body:**
```json
{
  "email": "ahmed.mohamed@example.com"
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم إرسال الكود الجديد"
}
```

---

### 8. POST /api/auth/send-phone-code

**What it does:** Sends a 6-digit OTP code to the user's phone number via SMS.

**Who uses it:** Any authenticated user

**Authorization:** Requires Login

**Where to use it in the frontend:** Settings → Phone Verification → "Send Code" button.

---

#### Request

**Request Body:**
```json
{
  "phoneNumber": "01012345678"
}
```
Max 20 chars, valid phone format.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم إرسال كود التفعيل"
}
```

---

### 9. POST /api/auth/verify-phone

**What it does:** Verifies the user's phone number using the OTP code.

**Who uses it:** Any authenticated user

**Authorization:** Requires Login

**Where to use it in the frontend:** Settings → Phone Verification → "Verify" button.

---

#### Request

**Request Body:**
```json
{
  "phoneNumber": "01012345678",
  "code": "729104"
}
```
`code` must be exactly 6 characters.

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم تفعيل رقم الهاتف بنجاح"
}
```

---

### 10. POST /api/auth/resend-phone-code

**What it does:** Resends the phone verification OTP.

**Who uses it:** Any authenticated user

**Authorization:** Requires Login

**Where to use it in the frontend:** Settings → Phone Verification → "Resend Code" link.

---

#### Request

**Request Body:**
```json
{
  "phoneNumber": "01012345678"
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "success": true,
  "message": "تم إرسال الكود الجديد"
}
```

---

### 11. GET /api/auth/craftsman-only

**What it does:** Simple test endpoint to verify the JWT has a "craftsman" role. Returns a greeting message.

**Who uses it:** Craftsman users

**Authorization:** Requires Role: Craftsman

**Where to use it in the frontend:** Use in development/testing to verify role-based auth works.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "message": "أهلاً بالحرفي 🔧"
}
```

---

### 12. GET /api/auth/admin-only

**What it does:** Simple test endpoint to verify the JWT has an "admin" role.

---

#### Response

**Success Response — 200 OK**
```json
{
  "message": "أهلاً بالأدمن 👋"
}
```

---

### 13. GET /api/craftsmen/search

**What it does:** Searches for approved craftsmen by service type, city, minimum rating, and minimum experience.

**Who uses it:** Customers (public)

**Authorization:** Public

**Where to use it in the frontend:** Home page search bar, Browse Craftsmen page, Category filter page.

---

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| serviceType | string | No | null | Filter by service type (e.g. "سباكة") |
| city | string | No | null | Filter by city (e.g. "القاهرة") |
| minRating | decimal | No | null | Only show craftsmen with rating >= this value |
| minExperience | int | No | null | Only show craftsmen with experience >= this value |

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "id": 2,
    "userId": 27,
    "fullName": "محمد حسن",
    "email": "mohamed.hassan@gmail.com",
    "phone": "01000000002",
    "profileImageUrl": null,
    "serviceType": "كهرباء",
    "city": "الإسكندرية",
    "neighborhood": "سيدي بشر",
    "priceRangeMin": 200.00,
    "priceRangeMax": 600.00,
    "experience": 12,
    "isApproved": true,
    "isAvailable": true,
    "rating": 4.5,
    "bio": "مهندس كهربائي خبرة 12 سنة",
    "nationalIdUrl": null,
    "createdAt": "2026-01-10T00:00:00Z"
  }
]
```

**Error Responses:**
| 404 | No results found | `{ "message": "لم يتم العثور على أي حرفيين يطابقون محددات البحث الحالية." }` |

---

#### How It Works
1. `CraftsmenController.Search()` → `_craftsmanService.GetFilteredCraftsmenAsync(filter)`
2. Service queries `Craftsmen` table: `IsApproved == true && IsDeleted == false && IsAvailable == true`
3. Applies filters: service type, city, minimum rating, minimum experience
4. Returns list of `CraftsmanDto`

---

#### Frontend Integration Notes
- All text parameters are in Arabic — send them exactly as stored (e.g. "سباكة" not "sebaka")
- The response is NOT paginated — returns all matching results
- Use the `rating` field to display star ratings (0-5 scale)
- Use `priceRangeMin` and `priceRangeMax` to show price range
- The `profileImageUrl` can be null — show a default avatar in that case

---

### 14. GET /api/craftsmen/{id}

**What it does:** Returns the full profile of a single approved craftsman.

**Who uses it:** Customers (public)

**Authorization:** Public

**Where to use it in the frontend:** Craftsman Detail page (when a customer clicks on a craftsman card).

---

#### Request

**Route Parameters:** `id` (int, required) — the craftsman's ID

**Request Body:** None

---

#### Response

**Success Response — 200 OK** — Same shape as a single item in the search results above.

| 404 | Not found | `{ "message": "عذراً، هذا الحرفي غير موجود حالياً." }` |

---

### 15. POST /api/craftsmen/{id}/upload-image

**What it does:** Uploads a profile image for a craftsman. The image is saved to the server and a URL is returned.

**Who uses it:** Craftsman (owner) or Admin

**Authorization:** Requires Login (owner or admin)

**Where to use it in the frontend:** Craftsman Profile Edit page → Image upload.

---

#### Request

**Route Parameters:** `id` (int, required) — the craftsman's ID

**Content-Type:** `multipart/form-data`

**Request Body:** Form data with a single field:
| Field | Type | Required | What it does |
|-------|------|----------|--------------|
| file | IFormFile | Yes | The image file (JPEG, PNG, or WEBP) |

---

#### Response

**Success Response — 200 OK**
```json
{
  "url": "/uploads/craftsman/{unique-filename}.jpg"
}
```

| 400 | No file selected | `{ "message": "الرجاء اختيار صورة للرفع." }` |
| 404 | Craftsman not found | `{ "message": "عذراً، هذا الحرفي غير موجود حالياً." }` |

---

### 16. PUT /api/craftsmen/{id}

**What it does:** Updates a craftsman's profile information.

**Who uses it:** Craftsman (owner) or Admin

**Authorization:** Requires Login (owner or admin)

**Where to use it in the frontend:** Craftsman Profile Edit page → Save changes.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "fullname": "أحمد علي محمد",
  "profileImageUrl": "/uploads/craftsman/new-image.jpg",
  "city": "القاهرة",
  "neighborhood": "مدينة نصر",
  "priceRangeMin": 200,
  "priceRangeMax": 500,
  "experience": 4,
  "bio": "سباك محترف بخبرة 4 سنوات"
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "message": "تم تحديث بيانات الملف الشخصي بنجاح."
}
```

| 404 | Craftsman not found | `{ "message": "لم يتم العثور على حساب الحرفي المطلوب لتحديثه." }` |

---

### 17. POST /api/jobs

**What it does:** Creates a new job request. The customer describes the problem and a craftsman can accept it.

**Who uses it:** Customers

**Authorization:** Requires Role: Customer

**Where to use it in the frontend:** Create Job page / "Post a Job" form.

---

#### Request

**Request Body:**
```json
{
  "craftsmanId": null,
  "serviceType": "سباكة",
  "description": "الحنفية في المطبخ بتقطر مياه باستمرار ومش بتقفل كويس",
  "address": "12 شارع النيل، مدينة نصر، القاهرة",
  "preferredDate": "2026-06-15T10:00:00Z",
  "problemImageUrl": null,
  "problemDescription": "تقطير مستمر من الحنفية بعد الإغلاق"
}
```

**Field Validation:**
| Field | Required | Rules |
|-------|----------|-------|
| craftsmanId | No | If null, any craftsman can accept |
| serviceType | Yes | Max 50 chars |
| description | Yes | Min 10 chars, max 2000 chars |
| address | Yes | Max 500 chars |
| preferredDate | No | DateTime |
| problemImageUrl | No | Max 500 chars |
| problemDescription | No | Max 2000 chars |

---

#### Response

**Success Response — 201 Created**
```json
{
  "id": 81,
  "customerId": 51,
  "craftsmanId": null,
  "status": "مفتوح",
  "serviceType": "سباكة",
  "description": "الحنفية في المطبخ بتقطر مياه باستمرار",
  "address": "12 شارع النيل، مدينة نصر، القاهرة",
  "preferredDate": "2026-06-15T10:00:00Z",
  "problemImageUrl": null,
  "problemDescription": "تقطير مستمر من الحنفية",
  "solutionDescription": null,
  "createdAt": "2026-06-10T12:00:00Z",
  "completedAt": null,
  "updatedAt": "2026-06-10T12:00:00Z"
}
```

---

### 18. GET /api/jobs/customer/{id}

**What it does:** Returns all jobs for a specific customer.

**Who uses it:** Customer (owner) or Admin

**Authorization:** Requires Login (owner or admin)

**Where to use it in the frontend:** Customer Dashboard → My Jobs page.

---

#### Request

**Route Parameters:** `id` (int, required) — the customer's User ID

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "id": 81,
    "customerId": 51,
    "craftsmanId": null,
    "status": "مفتوح",
    "serviceType": "سباكة",
    "description": "الحنفية بتقطر مياه",
    "address": "12 شارع النيل، القاهرة",
    "preferredDate": null,
    "problemImageUrl": null,
    "problemDescription": null,
    "solutionDescription": null,
    "createdAt": "2026-06-10T12:00:00Z",
    "completedAt": null,
    "updatedAt": "2026-06-10T12:00:00Z"
  }
]
```

---

### 19. POST /api/reviews

**What it does:** Submits a star rating and optional comment for a completed job.

**Who uses it:** Customers

**Authorization:** Requires Role: Customer

**Where to use it in the frontend:** Job Complete page / My Jobs → "Rate Craftsman" button.

---

#### Request

**Request Body:**
```json
{
  "jobId": 1,
  "stars": 5,
  "comment": "شغل ممتاز ونظيف — أنصح بالتعامل"
}
```

**Field Validation:**
| Field | Required | Rules |
|-------|----------|-------|
| jobId | Yes | Valid job ID |
| stars | Yes | Between 1 and 5 |
| comment | No | Max 1000 chars |

---

#### Response

**Success Response — 201 Created**
```json
{
  "message": "تم إرسال تقييمك بنجاح",
  "data": {
    "id": 41,
    "jobId": 1,
    "stars": 5,
    "comment": "شغل ممتاز ونظيف — أنصح بالتعامل",
    "customerName": "أحمد محمد",
    "createdAt": "2026-06-10T12:30:00Z"
  }
}
```

| 400 | Already reviewed | `{ "message": "هذه الوظيفة تم تقييمها بالفعل" }` |
| 400 | Job not completed | `{ "message": "يمكن تقييم الوظائف المكتملة فقط" }` |

---

### 20. GET /api/reviews/craftsman/{craftsmanId}

**What it does:** Returns all reviews for a specific craftsman with average rating.

**Who uses it:** Public

**Authorization:** Public

**Where to use it in the frontend:** Craftsman Detail page → Reviews section.

---

#### Request

**Route Parameters:** `craftsmanId` (int, required)

---

#### Response

**Success Response — 200 OK**
```json
{
  "craftsmanId": 2,
  "totalReviews": 5,
  "averageStars": 4.2,
  "reviews": [
    {
      "id": 1,
      "jobId": 3,
      "stars": 5,
      "comment": "شغل محترف ونظافة بعد الشغل",
      "customerName": "سارة أحمد",
      "createdAt": "2026-01-12T00:00:00Z"
    }
  ]
}
```

---

### 21. POST /api/reviews/rag-feedback

**What it does:** Submits feedback on an AI-provided solution (helpful / not helpful).

**Who uses it:** Any authenticated user (customers and craftsmen)

**Authorization:** Requires Login

**Where to use it in the frontend:** AI Chat → After viewing a solution → "Was this helpful?" buttons.

---

#### Request

**Request Body:**
```json
{
  "ragDocumentId": 5,
  "feedbackType": "helpful"
}
```
`feedbackType` values: `"helpful"` or `"not_helpful"`

---

#### Response

**Success Response — 200 OK**
```json
{
  "message": "تم تسجيل رأيك"
}
```

---

### 22. POST /api/conversations

**What it does:** Creates a new conversation (chat) between a customer and a craftsman for a specific job. If a conversation already exists, it returns the existing one.

**Who uses it:** Customers and Craftsmen (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Job Detail page → "Chat with Craftsman" / "Chat with Customer" button.

---

#### Request

**Request Body:**
```json
{
  "jobId": 1,
  "craftsmanId": 2
}
```

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 1,
  "jobId": 1,
  "otherUserId": 27,
  "otherUserName": "محمد حسن",
  "otherUserAvatar": null,
  "lastMessage": null,
  "lastMessageType": null,
  "lastMessageAt": null,
  "unreadCount": 0,
  "isOnline": false
}
```

| 400 | Cannot create conversation with self | `"لا يمكنك إنشاء محادثة مع نفسك."` |
| 400 | Job rejected or cancelled | `"لا يمكن بدء محادثة على وظيفة مرفوض."` |
| 404 | Job not found | `"الوظيفة غير موجودة."` |

---

### 23. GET /api/conversations

**What it does:** Returns all conversations the current user is participating in.

**Who uses it:** Customers and Craftsmen (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Chat Inbox page.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "id": 1,
    "jobId": 1,
    "otherUserId": 27,
    "otherUserName": "محمد حسن",
    "otherUserAvatar": null,
    "lastMessage": "تمام هكون عندك بكره",
    "lastMessageType": "text",
    "lastMessageAt": "2026-01-14T15:30:00Z",
    "unreadCount": 2,
    "isOnline": true
  }
]
```

---

#### Frontend Integration Notes
- `otherUserId` is the ID of the other participant (if you're customer, it's the craftsman's userId; if you're craftsman, it's the customer's userId)
- `unreadCount` tells you how many messages are unread — use this for badge notifications
- `isOnline` reflects real-time connection status (from SignalR `UserConnected`/`UserOffline` events)
- The conversation list is sorted by `lastMessageAt` descending (most recent first)

---

### 24. GET /api/conversations/{id}

**What it does:** Returns a single conversation's details.

**Who uses it:** Customers and Craftsmen (Shared endpoint — participant only)

**Authorization:** Requires Login

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK** — Same shape as a single item in the list above.

| 404 | Not found or not a participant | `"المحادثة غير موجودة أو الوصول مرفوض."` |

---

### 25. GET /api/conversations/{id}/messages

**What it does:** Returns paginated messages for a conversation.

**Who uses it:** Customers and Craftsmen (Shared endpoint — participant only)

**Authorization:** Requires Login

**Where to use it in the frontend:** Chat Detail page → Message list.

---

#### Request

**Route Parameters:** `id` (int, required)

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| page | int | No | 1 | Which page (oldest messages first) |
| pageSize | int | No | 20 | Messages per page (max 100) |

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "id": 1,
    "conversationId": 1,
    "senderId": 2,
    "senderName": "سارة أحمد",
    "senderAvatar": null,
    "content": "أهلاً، محتاج حد يصلح التسريب في الحمام بسرعة",
    "messageType": "text",
    "isRead": true,
    "sentAt": "2026-01-10T10:00:00Z"
  }
]
```

---

#### Frontend Integration Notes
- Messages are ordered by `sentAt` ascending (oldest first)
- Use `page` and `pageSize` for infinite scroll (load older messages as user scrolls up)
- `isRead` indicates if the other participant has seen the message
- `messageType` can be: `"text"`, `"image"`, `"voice"`, or `"location"`
- For `"image"` type, `content` contains the image URL
- For `"voice"` type, `content` contains the audio URL

---

### 26. PUT /api/conversations/{id}/read

**What it does:** Marks all messages in a conversation as read.

**Who uses it:** Customers and Craftsmen (Shared endpoint — participant only)

**Authorization:** Requires Login

**Where to use it in the frontend:** Chat Detail page → When user opens the conversation.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 204 No Content** — Empty body

---

### 27. DELETE /api/conversations/{id}/messages/{messageId}

**What it does:** Deletes a single message from a conversation.

**Who uses it:** Customers and Craftsmen (Shared endpoint — participant only)

**Authorization:** Requires Login

**Where to use it in the frontend:** Chat Detail → Long press / Right click on a message → "Delete".

---

#### Request

**Route Parameters:** `id` (int, required, conversation Id), `messageId` (int, required, message Id)

**Request Body:** None

---

#### Response

**Success Response — 204 No Content** — Empty body

| 400 | Cannot delete | `"لا يمكن حذف هذه الرسالة."` |

---

### 28. POST /api/conversations/upload-image

**What it does:** Uploads an image to be sent as a chat message.

**Who uses it:** Customers and Craftsmen (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Chat Detail → Image picker → "Send".

---

#### Request

**Content-Type:** `multipart/form-data`

| Field | Type | Required | What it does |
|-------|------|----------|--------------|
| file | IFormFile | Yes | Image file (JPG, PNG, or WEBP, max 5MB) |

---

#### Response

**Success Response — 200 OK**
```json
{
  "url": "/uploads/chat/{unique-filename}.jpg"
}
```

| 400 | Invalid file type | `{ "message": "نوع الملف غير مدعوم. الأنواع المسموحة: JPG, PNG, WEBP." }` |
| 400 | File too large | `{ "message": "حجم الصورة يجب أن لا يتجاوز 5 ميجابايت." }` |

---

### 29. POST /api/conversations/upload-voice

**What it does:** Uploads a voice recording to be sent as a chat message.

**Who uses it:** Customers and Craftsmen (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Chat Detail → Voice recorder → "Send".

---

#### Request

**Content-Type:** `multipart/form-data`

| Field | Type | Required | What it does |
|-------|------|----------|--------------|
| voice | IFormFile | Yes | Audio file (MP3, WAV, OGG, or WEBM, max 10MB) |

---

#### Response

**Success Response — 200 OK**
```json
{
  "url": "/uploads/chat-voices/{unique-filename}.mp3"
}
```

| 400 | Invalid file type | Error message string (not wrapped in JSON) |
| 400 | No file | `"يرجى اختيار ملف صوتي للرفع."` |

---

### 30. GET /api/users/profile/{id}

**What it does:** Returns a user's profile information.

**Who uses it:** User (owner) or Admin (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Settings → Profile page / Admin User Detail.

---

#### Request

**Route Parameters:** `id` (int, required) — the user's ID

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 51,
  "name": "أحمد محمد",
  "email": "ahmed.mohamed@example.com",
  "role": "customer",
  "phone": "01012345678",
  "profileImageUrl": null,
  "isActive": true,
  "createdAt": "2026-06-10T12:00:00Z"
}
```

| 404 | User not found | `{ "message": "عذراً، هذا المستخدم غير موجود." }` |

---

### 31. PUT /api/users/profile/{id}

**What it does:** Updates a user's profile information (name and phone).

**Who uses it:** User (owner) or Admin (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Settings → Edit Profile page.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "name": "أحمد محمد علي",
  "phone": "01112345678"
}
```
`name` is required (max 100 chars), `phone` is required (max 20 chars, valid phone format).

---

#### Response

**Success Response — 200 OK**
```json
{
  "message": "تم تحديث بيانات الملف الشخصي بنجاح."
}
```

---

### 32. POST /api/users/profile/{id}/upload-image

**What it does:** Uploads a profile image for a user.

**Who uses it:** User (owner) or Admin (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Settings → Profile → Change profile picture.

---

#### Request

**Route Parameters:** `id` (int, required)

**Content-Type:** `multipart/form-data`

| Field | Type | Required |
|-------|------|----------|
| file | IFormFile | Yes |

---

#### Response

**Success Response — 200 OK**
```json
{
  "url": "/uploads/profiles/{unique-filename}.jpg"
}
```

---

### 33. GET /api/notifications

**What it does:** Returns all notifications for the current user.

**Who uses it:** Customers and Craftsmen (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Notification panel / Notification page.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "id": 1,
    "title": "تم قبول طلب الخدمة",
    "body": "قام الحرفي إبراهيم نصر بقبول طلب الخدمة الخاص بك.",
    "type": "job_accepted",
    "relatedJobId": 3,
    "conversationId": null,
    "isRead": true,
    "createdAt": "2026-01-10T12:00:00Z"
  }
]
```

**Notification Types:** `registration_pending`, `approved`, `rejected`, `job_accepted`, `job_completed`, `new_message`, `dispute_opened`, `dispute_resolved`

---

### 34. GET /api/notifications/unread-count

**What it does:** Returns the count of unread notifications.

**Who uses it:** Customers and Craftsmen (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Navbar → Notification bell badge.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "unreadCount": 3
}
```

---

### 35. PUT /api/notifications/{id}/read

**What it does:** Marks a single notification as read.

**Who uses it:** Customers and Craftsmen (Shared endpoint)

**Authorization:** Requires Login

**Where to use it in the frontend:** Notification panel → Click on notification.

---

#### Request

**Route Parameters:** `id` (int, required)

---

#### Response

**Success Response — 200 OK**
```json
{
  "message": "تم تحديث الإشعار بنجاح"
}
```

| 404 | Notification not found or not yours | `{ "message": "الإشعار غير موجود أو لا ينتمي إليك." }` |

---

### 36. PUT /api/notifications/read-all

**What it does:** Marks all notifications as read.

**Who uses it:** Customers and Craftsmen (Shared endpoint)

---

#### Response

**Success Response — 204 No Content**

---

### 37. DELETE /api/notifications/{id:int}

**What it does:** Deletes a single notification.

---

#### Response

**Success Response — 204 No Content**

| 404 | Not found or not yours | `{ "message": "الإشعار غير موجود أو لا ينتمي إليك." }` |

---

### 38. DELETE /api/notifications/clear

**What it does:** Deletes all notifications.

---

#### Response

**Success Response — 204 No Content**

---

## Group 3 — Craftsman Endpoints

---

### 1. POST /api/craftsmen/register

**What it does:** Submits a craftsman application after the user account has been created. Sends the application for admin review.

**Who uses it:** Craftsman applicants

**Authorization:** Public

**Where to use it in the frontend:** Craftsman Registration page → "Complete Profile" form (shown after user registration with role "craftsman").

---

#### Request

**Request Body:**
```json
{
  "userId": 51,
  "serviceType": "سباكة",
  "city": "القاهرة",
  "neighborhood": "مدينة نصر",
  "priceRangeMin": 150,
  "priceRangeMax": 400,
  "experience": 5,
  "bio": "سباك متخصص في تركيب وصيانة جميع أنواع السباكة",
  "nationalIdUrl": "/uploads/ids/id_card.jpg"
}
```

**Field Validation:**
| Field | Required | Rules |
|-------|----------|-------|
| userId | Yes | Must match the registered user |
| serviceType | Yes | Max 50 chars |
| city | Yes | Max 100 chars |
| neighborhood | No | Max 100 chars |
| priceRangeMin | No | Decimal |
| priceRangeMax | No | Decimal |
| experience | Yes | Integer (years) |
| bio | No | Max 1000 chars |
| nationalIdUrl | Yes | Max 500 chars (URL to uploaded ID image) |

---

#### Response

**Success Response — 201 Created**
```json
{
  "message": "تم تقديم طلبك بنجاح وهو قيد المراجعة حالياً."
}
```

| 400 | Registration failed | `{ "message": "فشل في تقديم طلب التسجيل، يرجى المحاولة مرة أخرى." }` |

---

### 2. PUT /api/craftsmen/{id}

**What it does:** Updates the craftsman's profile. Documented in Group 2 — same endpoint, but used by craftsmen to update their own profile.

**Authorization:** Craftsman (owner) or Admin

---

### 3. POST /api/craftsmen/{id}/upload-image

**What it does:** Uploads a craftsman profile image. Documented in Group 2.

---

### 4. PUT /api/jobs/{id}/accept

**What it does:** Accepts a job, changing its status from "مفتوح" (Open) to "قيد التنفيذ" (In Progress).

**Who uses it:** Craftsmen

**Authorization:** Requires Role: Craftsman

**Where to use it in the frontend:** Craftsman Dashboard → Available Jobs → "Accept" button.

---

#### Request

**Route Parameters:** `id` (int, required) — the job ID

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 1,
  "craftsmanId": 2,
  "status": "قيد التنفيذ",
  "serviceType": "كهرباء",
  "description": "المفاتيح في الصالة بتشرر",
  "address": "15 شارع التحرير",
  "createdAt": "2026-01-10T00:00:00Z",
  "updatedAt": "2026-01-11T00:00:00Z"
}
```

---

### 5. PUT /api/jobs/{id}/reject

**What it does:** Rejects a job, changing its status to "مرفوض" (Rejected).

**Who uses it:** Craftsmen

**Authorization:** Requires Role: Craftsman

**Where to use it in the frontend:** Craftsman Dashboard → Available Jobs → "Reject" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 1,
  "status": "مرفوض",
  ...
}
```

---

### 6. PUT /api/jobs/{id}/complete

**What it does:** Marks a job as completed and optionally provides a solution description.

**Who uses it:** Craftsmen

**Authorization:** Requires Role: Craftsman

**Where to use it in the frontend:** Craftsman Dashboard → In Progress Jobs → "Mark Complete" button.

---

#### Request

**Route Parameters:** `id` (int, required)

**Request Body:**
```json
{
  "solutionDescription": "تم تغيير الحنفية بالكامل وتركيب حنفية جديدة مع ضمان عدم التسريب"
}
```
`solutionDescription` is optional.

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 1,
  "status": "مكتمل",
  "solutionDescription": "تم تغيير الحنفية بالكامل وتركيب حنفية جديدة",
  "completedAt": "2026-06-10T14:00:00Z",
  ...
}
```

---

### 7. GET /api/jobs/craftsman/{id}

**What it does:** Returns all jobs assigned to a specific craftsman.

**Who uses it:** Craftsman (owner) or Admin

**Authorization:** Requires Login (owner or admin)

**Where to use it in the frontend:** Craftsman Dashboard → My Jobs page.

---

#### Request

**Route Parameters:** `id` (int, required) — the craftsman's ID (not User ID!)

---

#### Response

**Success Response — 200 OK** — Same shape as customer jobs list.

---

### 8. POST /api/conversations (shared)

**What it does:** Craftsmen can also create conversations with customers. Same endpoint as Group 2.

---

### 9. GET /api/conversations (shared)

**What it does:** Craftsmen see their conversations with customers. Same endpoint as Group 2.

---

### 10. SignalR ChatHub (shared)

**What it does:** Craftsmen use the same ChatHub as customers to send and receive real-time messages.

---

## Group 4 — AI Endpoints

---

### 1. GET /api/AI/welcome

**What it does:** Returns a welcome message from the AI assistant.

**Who uses it:** Public (no auth)

**Authorization:** Public

**Where to use it in the frontend:** AI Chat page → Initial greeting.

---

#### Request

**Request Body:** None

---

#### Response

**Success Response — 200 OK**
```json
{
  "message": "أهلاً بك! 👋\nأنا مساعدك الذكي للعثور على أفضل الحرفيين في مصر.\nأخبرني بمشكلتك وسأجد لك الحرفي المناسب فوراً! 🔧"
}
```

---

### 2. POST /api/AI/chat3

**What it does:** The main multi-turn AI chat endpoint. The AI understands the user's intent (find a craftsman or get DIY steps), extracts service type/city/count, and either recommends craftsmen or provides solution steps.

**Who uses it:** Public (no auth)

**Authorization:** Public

**Where to use it in the frontend:** AI Chat page → Send message.

---

#### Request

**Request Body:**
```json
{
  "messages": [
    { "role": "user", "content": "عاوز سباك في القاهرة" }
  ],
  "extractedService": null,
  "extractedCity": null,
  "extractedCount": null,
  "failedServiceAttempts": 0,
  "failedCityAttempts": 0,
  "failedCountAttempts": 0,
  "intent": 0,
  "problemClarificationAttempts": 0,
  "followUpState": 0,
  "lastProblemDescription": null,
  "extractedDistrict": null,
  "userId": null,
  "sessionId": null,
  "solutionSteps": []
}
```

**Field Breakdown:**
| Field | Type | What it does |
|-------|------|--------------|
| messages | array | Chat history (all previous messages) |
| extractedService | string? | Service type identified so far |
| extractedCity | string? | City identified so far |
| extractedCount | int? | Number of craftsmen requested |
| failedServiceAttempts | int | How many times LLM failed to extract service |
| failedCityAttempts | int | How many times LLM failed to extract city |
| failedCountAttempts | int | How many times LLM failed to extract count |
| intent | int | 0=NotAsked, 1=WantCraftsman, 2=WantSteps |
| problemClarificationAttempts | int | How many times we've asked for problem details |
| followUpState | int | 0=None, 1=WaitingAnswer, 2=WaitingDetail, 3=WaitingFeedback |
| lastProblemDescription | string? | Last problem description for solution steps |
| extractedDistrict | string? | District/neighborhood for nearby search |
| userId | int? | User ID (for saving messages to DB) |
| sessionId | string? | Session GUID (for grouping messages) |
| solutionSteps | array | Current solution steps being discussed |

---

#### Response

**Success Response — 200 OK** — Response varies by conversation state. Common response shape:

```json
{
  "isComplete": false,
  "message": "تمام! فهمت إنك محتاج سباكة 👍\nإيه اللي تحب أعمله؟",
  "showServicesList": false,
  "servicesList": [],
  "showCitiesList": false,
  "citiesList": [],
  "showIntentChoice": true,
  "solutionSteps": [],
  "showSolvedQuestion": false,
  "showFeedbackQuestion": false,
  "extractedService": "سباكة",
  "extractedCity": null,
  "extractedCount": null,
  "problemClarificationAttempts": 0,
  "followUpState": 0,
  "lastProblemDescription": null,
  "result": null,
  "latencyMs": 612
}
```

**When complete (craftsmen found):**
```json
{
  "isComplete": true,
  "message": "تمام! وجدت لك أفضل 2 سباكة في القاهرة 🎉\n\n1. أحمد علي - 📍 القاهرة, مدينة نصر\n⭐ 4.5 (3 تقييم)\n💰 150 - 400 ج.م",
  "result": {
    "answer": "1. أحمد علي - 📍 القاهرة, مدينة نصر\n⭐ 4.5...",
    "retrievedCraftsmen": [
      {
        "id": 1,
        "name": "أحمد علي",
        "serviceType": "سباكة",
        "city": "القاهرة",
        "neighborhood": "مدينة نصر",
        "rating": 4.5,
        "experienceYears": 3,
        "priceRangeMin": 150,
        "priceRangeMax": 400,
        "relevantText": "سباك متخصص في تركيب وصيانة شبكات المياه والصرف الصحي",
        "similarityScore": 0.92,
        "isNearby": true,
        "nearbyFromCity": null
      }
    ]
  },
  "latencyMs": 2150
}
```

**When asking for more info:**
```json
{
  "isComplete": false,
  "message": "في أي محافظة أنت محتاج الفني؟",
  "showServicesList": false,
  "showCitiesList": true,
  "citiesList": ["القاهرة", "الإسكندرية", "الجيزة", ...],
  "extractedService": "سباكة",
  "extractedCity": null,
  "extractedCount": null,
  "latencyMs": 612
}
```

---

#### How It Works
1. Checks if the message is in Arabic — if not, returns error
2. Checks for multiple service keywords (e.g. "سباك وكهربائي")
3. If intent is `WantSteps` — gets solution steps from Qdrant/Groq
4. Otherwise, sends conversation to Groq LLM to extract: service type, city, count
5. If info is missing, asks the user (sets `showServicesList` or `showCitiesList`)
6. If all info is present, queries Qdrant vector DB for matching craftsmen
7. Returns results with re-ranking by city proximity

---

#### Frontend Integration Notes
- This is a **stateful multi-turn** chat — always send the full `messages` array with ALL previous messages
- Pass back the `extractedService`, `extractedCity`, etc. from the response into the next request
- Use `isComplete` to know if the conversation has finished (craftsmen found)
- Use `showServicesList` / `showCitiesList` to display choice buttons
- Use `showIntentChoice` to show "Want Craftsman" / "Want Steps" buttons
- `userId` and `sessionId` are optional — if provided, messages are saved to DB for history

---

### 3. POST /api/AI/analyze-media

**What it does:** Analyzes an image (photo of the problem) or audio recording (user describing the problem) using an external n8n webhook, then returns solution steps.

**Who uses it:** Public (no auth)

**Authorization:** Public

**Where to use it in the frontend:** AI Chat → Camera button (take a photo) or Microphone button (record audio).

---

#### Request

**Content-Type:** `multipart/form-data`

| Field | Type | Required | What it does |
|-------|------|----------|--------------|
| images | IFormFile[] | No | List of image files (photo of the problem) |
| audio | IFormFile | No | Audio recording (user describing the problem) |
| userText | string | No | Optional text description |
| extractedService | string | No | Previously extracted service |
| extractedCity | string | No | Previously extracted city |
| extractedCount | int | No | Previously extracted count |
| extractedDistrict | string | No | Previously extracted district |
| userId | int | No | User ID for saving to DB |
| sessionId | string | No | Session ID for saving to DB |

**Note:** At least one of `images` or `audio` is required.

---

#### Response

**Success Response — 200 OK**
```json
{
  "isComplete": false,
  "message": "🔧 فهمت إن المشكلة في تخصص: سباكة\n\n📋 المشكلة: الحنفية بتقطر مياه باستمرار\n\nإليك خطوات عملية يمكنك تجربتها:\n\n✦ الخطوة 1: أقفل محبس المياه الرئيسي\n✦ الخطوة 2: فك الحنفية باستخدام مفتاح\n✦ الخطوة 3: استبدل الجلدة التالفة\n✦ الخطوة 4: ركب الحنفية واختبر التسريب",
  "solutionSteps": ["أقفل محبس المياه", "فك الحنفية", "استبدل الجلدة", "ركب واختبر"],
  "extractedService": "سباكة",
  "extractedCity": null,
  "extractedCount": null,
  "followUpState": 1,
  "lastProblemDescription": "الحنفية بتقطر مياه باستمرار",
  "problemClarificationAttempts": 0,
  "latencyMs": 4500
}
```

| 400 | No media sent | Returns error asking user to send image or audio |

---

### 4. GET /api/AI/sessions/{userId}

**What it does:** Returns a list of all AI chat sessions for a given user.

**Who uses it:** Any user (no auth)

**Authorization:** Public

**Where to use it in the frontend:** AI Chat → History page → Session list.

---

#### Request

**Route Parameters:** `userId` (int, required)

---

#### Response

**Success Response — 200 OK**
```json
[
  {
    "sessionId": "3f139bce-256e-45e4-9583-f3d80a09d895",
    "title": "سباك في القاهرة",
    "lastMessage": "تمام! وجدت لك أفضل سباك...",
    "lastActivity": "2026-06-10T18:30:00Z",
    "messageCount": 8
  }
]
```

---

### 5. GET /api/AI/sessions/{userId}/{sessionId}

**What it does:** Returns the full message history for a specific AI chat session.

**Who uses it:** Any user (no auth)

**Authorization:** Public

**Where to use it in the frontend:** AI Chat → History → Click on a session to view.

---

#### Request

**Route Parameters:** `userId` (int, required), `sessionId` (string, required, GUID format)

---

#### Response

**Success Response — 200 OK**
```json
{
  "sessionId": "3f139bce-256e-45e4-9583-f3d80a09d895",
  "title": "سباك في القاهرة",
  "messages": [
    {
      "id": 1,
      "role": "user",
      "content": "عاوز سباك في القاهرة",
      "createdAt": "2026-06-10T18:00:00Z",
      "images": [],
      "audio": null
    },
    {
      "id": 2,
      "role": "assistant",
      "content": "تمام! فهمت إنك محتاج سباكة 👍...",
      "createdAt": "2026-06-10T18:00:01Z",
      "images": [],
      "audio": null
    }
  ]
}
```

| 404 | Session not found | `{ "error": "المحادثة مش موجودة" }` |

---

### 6. POST /api/AI/sessions/message

**What it does:** Saves a new message to an AI chat session (with optional image/audio attachments).

**Who uses it:** Any user (no auth)

**Authorization:** Public

**Where to use it in the frontend:** AI Chat → Every time a message is sent/received, save it.

---

#### Request

**Content-Type:** `multipart/form-data`

| Field | Type | Required | What it does |
|-------|------|----------|--------------|
| userId | int | Yes | User ID |
| sessionId | string | Yes | Session GUID |
| role | string | Yes | "user" or "assistant" |
| content | string | No | Message text |
| toolUsed | string | No | AI tool used (e.g. "CraftsmanSearchTool") |
| images | IFormFile[] | No | Attached images |
| audio | IFormFile | No | Attached audio file |

---

#### Response

**Success Response — 200 OK**
```json
{
  "id": 98,
  "images": ["/AiChat/images/abc123.jpg"],
  "audio": null
}
```

---

### 7. DELETE /api/AI/sessions/{userId}/{sessionId}

**What it does:** Deletes an entire AI chat session and its attached media files.

**Who uses it:** Any user (no auth)

**Authorization:** Public

**Where to use it in the frontend:** AI Chat → History → "Delete" button.

---

#### Request

**Route Parameters:** `userId` (int, required), `sessionId` (string, required)

---

#### Response

**Success Response — 200 OK**
```json
{
  "deleted": 8
}
```

| 404 | Not found | `{ "error": "المحادثة مش موجودة" }` |

---

### 8. POST /api/AI/craftsman/check-and-submit-solution

**What it does:** Craftsmen submit solution steps for a problem. The LLM checks if the solution is appropriate, fixes spelling, and if accepted, saves it to the database and Qdrant vector DB for future AI recommendations.

**Who uses it:** Craftsmen (no auth requirement)

**Authorization:** Public

**Where to use it in the frontend:** Craftsman Dashboard → "Submit Solution" page.

---

#### Request

**Request Body:**
```json
{
  "userId": 27,
  "serviceType": "سباكة",
  "problemDescription": "الحنفية بتقطر مياه بعد الإغلاق",
  "steps": [
    "أقفل محبس المياه الرئيسي",
    "فك الحنفية باستخدام مفتاح",
    "استبدل الجلدة التالفة",
    "ركب الحنفية واختبر التسريب"
  ],
  "craftsmanId": 2
}
```

---

#### Response

**Success Response (Accepted) — 200 OK**
```json
{
  "accepted": true,
  "message": "تم قبول الخطوات وحفظها بنجاح ✅",
  "jobId": 82,
  "upserted": 1,
  "fixedSteps": [
    "أقفل محبس المياه الرئيسي",
    "فك الحنفية باستخدام مفتاح مناسب",
    "استبدل الجلدة التالفة بأخرى جديدة",
    "ركب الحنفية واختبر التسريب"
  ]
}
```

**Success Response (Rejected) — 200 OK**
```json
{
  "accepted": false,
  "message": "الخطوات مش مناسبة لحل المشكلة دي 🙅 السبب: هذه الخطوات لا تحل مشكلة الحنفية فقط"
}
```

| 400 | Missing required fields | `{ "error": "التخصص مطلوب" }` |

---

### 9. POST /api/AI/ingest/craftsmen

**What it does:** Indexes all craftsman profiles into Qdrant vector database for semantic search.

**Who uses it:** Admin only

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → "Re-index Craftsmen" button.

---

#### Request

**Query Parameters:** `fromId` (int, optional, default 0) — start indexing from this craftsman ID

---

#### Response

**Success Response — 200 OK**
```json
{
  "totalCraftsmen": 20,
  "totalChunksIndexed": 60,
  "message": "تم تحميل 20 حرفي بنجاح"
}
```

---

### 10. POST /api/AI/ingest/jobs

**What it does:** Indexes all completed jobs (their solution descriptions) into Qdrant for RAG.

**Who uses it:** Admin only

**Authorization:** Requires Role: Admin

---

#### Response

**Success Response — 200 OK**
```json
{
  "indexed": 45
}
```

---

### 11. POST /api/AI/ingest/job/{jobId}

**What it does:** Indexes a single job's solution into Qdrant.

**Who uses it:** Public (no auth)

---

#### Response

**Success Response — 200 OK**
```json
{
  "jobId": 1,
  "upserted": 1
}
```

---

### 12. GET /api/AI/vectors/count

**What it does:** Returns the total number of vectors stored in Qdrant.

**Who uses it:** Admin only

**Authorization:** Requires Role: Admin

---

#### Response

**Success Response — 200 OK**
```json
{
  "totalVectors": 80
}
```

---

## SignalR Hubs

### Hub 1: ChatHub — `/hubs/chat`

**Authentication:** JWT Bearer token required (sent as `access_token` query parameter)

**Connection URL:**
```
https://localhost:5108/hubs/chat?access_token={jwt_token}
```

**Group Naming Convention:**
- User group: `user_{userId}` (e.g. `user_27`)
- Conversation group: `conv_{conversationId}` (e.g. `conv_5`)

---

#### Client → Server Methods (call from Angular)

---

##### 1. JoinConversation

```typescript
this.hub.invoke('JoinConversation', conversationId);
```

**What it does:** Joins the SignalR group for a specific conversation to receive real-time messages.

**When to call:** When the user opens a chat conversation page.

**Parameters:**
| Name | Type | What it is |
|------|------|------------|
| conversationId | int | The conversation ID |

**Throws:** `HubException` if the user is not a participant.

---

##### 2. LeaveConversation

```typescript
this.hub.invoke('LeaveConversation', conversationId);
```

**What it does:** Leaves the conversation group.

**When to call:** When the user navigates away from the chat page.

---

##### 3. SendMessage

```typescript
this.hub.invoke('SendMessage', {
  conversationId: 5,
  content: 'تمام هكون عندك بكره',
  messageType: 'text'
});
```

**What it does:** Sends a chat message in real-time. The server saves it to DB and broadcasts to both participants.

**When to call:** When the user clicks "Send" in the chat.

**Message Types:** `text`, `image`, `voice`, `location`

**Validation:** Content is required, max 2000 chars, messageType must be valid.

---

##### 4. DeleteMessage

```typescript
this.hub.invoke('DeleteMessage', conversationId, messageId);
```

**What it does:** Deletes a message. Broadcasts deletion to both participants.

---

##### 5. Typing

```typescript
this.hub.invoke('Typing', conversationId);
```

**What it does:** Sends a typing indicator to the other participant.

**When to call:** When the user is typing (debounced, e.g. every 3 seconds).

---

##### 6. MarkAsRead

```typescript
this.hub.invoke('MarkAsRead', conversationId);
```

**What it does:** Marks all messages in the conversation as read and notifies the other participant.

**When to call:** When the user opens/clicks on a conversation.

---

##### 7. SetOffline

```typescript
this.hub.invoke('SetOffline');
```

**What it does:** Explicitly sets the user as offline, removing their connection.

**When to call:** On app destroy / logout.

---

#### Server → Client Events (listen in Angular)

---

##### 1. ReceiveMessage

```typescript
this.hub.on('ReceiveMessage', (message: MessageDto) => {
  // message contains: id, conversationId, senderId, senderName, 
  // senderAvatar, content, messageType, isRead, sentAt
});
```

**What triggers it:** Another participant sends a message in a conversation you're part of.

**What to do:** Append the message to the conversation's message list.

---

##### 2. MessageDeleted

```typescript
this.hub.on('MessageDeleted', (conversationId: number, messageId: number) => {
  // Remove the message from the UI
});
```

---

##### 3. ConversationUpdated

```typescript
this.hub.on('ConversationUpdated', (conversation: ConversationDto) => {
  // conversation contains: id, jobId, otherUserId, otherUserName, 
  // lastMessage, lastMessageType, lastMessageAt, unreadCount, isOnline
});
```

**What triggers it:** A new message is sent in any conversation you're part of.

**What to do:** Update the conversation list (inbox) with the new last message and unread count.

---

##### 4. UserTyping

```typescript
this.hub.on('UserTyping', (conversationId: number, userId: number) => {
  // Show "user is typing..." indicator
});
```

---

##### 5. MessagesRead

```typescript
this.hub.on('MessagesRead', (conversationId: number, userId: number) => {
  // Update read status of messages in the UI
});
```

---

##### 6. UserOnline / UserOffline

```typescript
this.hub.on('UserOnline', (userId: number) => { ... });
this.hub.on('UserOffline', (userId: number) => { ... });
```

**What to do:** Update the online status indicator in the conversation list.

---

### Hub 2: NotificationHub — `/hubs/notifications`

**Authentication:** JWT Bearer token required (sent as `access_token` query parameter)

**Connection URL:**
```
https://localhost:5108/hubs/notifications?access_token={jwt_token}
```

**Group Name:** `user_{userId}` (e.g. `user_27`)

---

#### Server → Client Events

##### 1. ReceiveNotification

```typescript
this.hub.on('ReceiveNotification', (notification: NotificationDto) => {
  // notification contains: id, title, body, type, relatedJobId, 
  // conversationId, isRead, createdAt
});
```

**What triggers it:** A new notification is created for this user (new message, job accepted, etc.)

**What to do:** Show a toast/popup notification and update the notification badge count.

---

#### Client → Server: None (this hub only receives)

This hub has no client-callable methods. It automatically joins the user to their notification group on connect.

---

## Authentication Flow

### 1. Register
```
POST /api/auth/register
Body: { name, email, password, confirmPassword, role, phone? }
→ Returns: { accessToken, refreshToken, expiresAt, user }
```

### 2. Verify Email
```
POST /api/auth/verify-email
Body: { email, code (6 digits) }
→ Returns: { success, message }
```
A 6-digit code is sent to the user's email automatically after registration. The user must verify before they can log in.

### 3. Login
```
POST /api/auth/login
Body: { email, password }
→ Returns: { accessToken, refreshToken, expiresAt, user }
```

### 4. Use Token for All Requests
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### 5. Refresh Token (when accessToken expires)
```
POST /api/auth/refresh
Body: { refreshToken }
→ Returns: { accessToken, refreshToken, expiresAt, user }
```

### 6. Logout
```
POST /api/auth/logout
Authorization: Bearer {accessToken}
Body: { refreshToken }
→ Returns: { message }
```

### Token Storage Recommendation
```
localStorage.setItem('accessToken', response.accessToken);
localStorage.setItem('refreshToken', response.refreshToken);
localStorage.setItem('user', JSON.stringify(response.user));
```

### Token Expiry
- Access token: 60 minutes (configurable in appsettings.json)
- Refresh token: 30 days
- Use an HTTP interceptor to auto-refresh when getting 401 responses

---

## Common Response Patterns

### Success — 200 OK
```json
// Single object (most endpoints)
{ "field": "value" }

// List
[ { "id": 1, ... }, { "id": 2, ... } ]

// Paginated
{
  "items": [ ... ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

### Created — 201 Created
```json
{
  "id": 81,
  "field": "value",
  ...
}
```

### No Content — 204 (no body)

### Validation Error — 400 Bad Request
```json
{
  "message": {
    "Email": ["صيغة البريد الإلكتروني غير صحيحة"],
    "Password": ["كلمة المرور لا تقل عن 8 أحرف"]
  }
}
```
or simple:
```json
{
  "message": "رسالة خطأ واحدة"
}
```

### Unauthorized — 401
```json
{
  "message": "غير مصرح"
}
```

### Forbidden — 403
Empty body (ASP.NET Core default).

### Not Found — 404
```json
{
  "message": "عذراً، هذا الحرفي غير موجود حالياً."
}
```

---

## Appendix — Data Models

### User
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| name | string | Max 100 chars |
| email | string | Unique, max 200 chars |
| role | string | "admin", "customer", or "craftsman" |
| phone | string? | Max 20 chars |
| isActive | bool | Default: true |
| isDeleted | bool | Default: false (soft delete) |
| isVerified | bool | Default: false |
| profileImageUrl | string? | Max 500 chars |
| deletionReason | string? | Max 500 chars |
| deletedAt | DateTime? | |
| createdAt | DateTime | UTC |

### Craftsman
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| userId | int | FK to User (1:1) |
| serviceType | string | e.g. "سباكة", "كهرباء" |
| city | string | e.g. "القاهرة" |
| neighborhood | string? | e.g. "مدينة نصر" |
| priceRangeMin | decimal? | Min price in EGP |
| priceRangeMax | decimal? | Max price in EGP |
| experience | int | Years of experience |
| isApproved | bool | Default: false |
| isAvailable | bool | Default: true |
| rating | decimal | 0.00 to 5.00 |
| bio | string? | Max 1000 chars |
| nationalIdUrl | string? | URL to uploaded ID |
| rejectionReason | string? | If rejected |

### Job
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| customerId | int | FK to User |
| craftsmanId | int? | FK to Craftsman |
| status | string | مفتوح / قيد التنفيذ / مكتمل / مرفوض / ملغى |
| serviceType | string | e.g. "سباكة" |
| description | string | Min 10, max 2000 chars |
| address | string | Max 500 chars |
| preferredDate | DateTime? | Customer's preferred date |
| problemImageUrl | string? | URL to problem photo |
| problemDescription | string? | Detailed problem description |
| solutionDescription | string? | Craftsman's solution |
| isDisputed | bool | Default: false |
| disputeResolution | string? | Resolution notes |
| createdAt | DateTime | |
| completedAt | DateTime? | |

### Review
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| jobId | int | FK to Job (1:1) |
| customerId | int | FK to User |
| craftsmanId | int | FK to Craftsman |
| stars | int | 1-5 |
| comment | string? | Max 1000 chars |
| isDeleted | bool | Soft delete flag |

### Conversation
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| jobId | int | FK to Job (1:1) |
| customerId | int | FK to User |
| craftsmanId | int | FK to Craftsman |
| lastMessageAt | DateTime? | Updated on each new message |
| createdAt | DateTime | |

### Message
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| conversationId | int | FK to Conversation |
| senderId | int | FK to User |
| content | string | Max 2000 chars |
| messageType | string | text / image / voice / location |
| isRead | bool | |
| sentAt | DateTime | |

### Notification
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| userId | int | FK to User |
| title | string | Max 200 chars |
| body | string | Max 1000 chars |
| type | string? | Notification category |
| relatedJobId | int? | FK to Job |
| isRead | bool | |
| createdAt | DateTime | |

### ServiceType (Admin Config)
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| nameAr | string | Arabic name (unique) |
| nameEn | string | English name (unique) |
| icon | string? | Icon class |
| isActive | bool | |

### City (Admin Config)
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| nameAr | string | Arabic name (unique) |
| nameEn | string | English name (unique) |
| governorate | string? | Governorate name |
| isActive | bool | |

### FeatureFlag (Admin Config)
| Field | Type | Notes |
|-------|------|-------|
| key | string | Primary key |
| isEnabled | bool | |
| updatedAt | DateTime | |

### AIChatMessage
| Field | Type | Notes |
|-------|------|-------|
| id | int | Auto-generated |
| userId | int | FK to User |
| sessionId | string | GUID grouping messages |
| role | string | "user" or "assistant" |
| content | string | Max 4000 chars |
| toolUsed | string? | e.g. "CraftsmanSearchTool" |
| tokensUsed | int? | LLM token count |
| createdAt | DateTime | |
