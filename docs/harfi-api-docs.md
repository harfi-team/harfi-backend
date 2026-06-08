# Harfi API — Complete Documentation
*Last updated: June 8, 2026*

## Quick Reference
> **Base URL:** `http://localhost:5108/api` (auth/craftsmen/users/conversations/notifications/reviews/jobs/AI)  
> **Admin Base URL:** `http://localhost:5108/api/v1/admin`  
> **SignalR Hubs:** `http://localhost:5108/hubs/chat` and `http://localhost:5108/hubs/notifications`  
> **Authentication:** Bearer JWT token in `Authorization` header  
> **Content-Type:** `application/json` (unless noted otherwise)  
> **All Arabic text uses UTF-8 encoding**  
> **Swagger UI:** `http://localhost:5108/` (root path)

---

## All Endpoints — Master Table

| # | Method | Path | Auth | Group | Description |
|---|--------|------|------|-------|-------------|
| 1 | POST | /api/auth/register | Public | Auth | Register new user (customer/craftsman) |
| 2 | POST | /api/auth/login | Public | Auth | Login and get JWT tokens |
| 3 | POST | /api/auth/refresh | Public | Auth | Refresh access token |
| 4 | POST | /api/auth/logout | Login | Auth | Logout and revoke refresh token |
| 5 | POST | /api/auth/verify-email | Public | Auth | Verify email with 6-digit code |
| 6 | POST | /api/auth/resend-code | Public | Auth | Resend email verification code |
| 7 | POST | /api/auth/send-phone-code | Login | Auth | Send phone verification code |
| 8 | POST | /api/auth/verify-phone | Login | Auth | Verify phone with 6-digit code |
| 9 | POST | /api/auth/resend-phone-code | Login | Auth | Resend phone verification code |
| 10 | GET | /api/users/profile/{id} | Login | Customer/Craftsman | Get user profile by ID |
| 11 | PUT | /api/users/profile/{id} | Login | Customer/Craftsman | Update user profile |
| 12 | POST | /api/users/profile/{id}/upload-image | Login | Customer/Craftsman | Upload profile image |
| 13 | POST | /api/craftsmen/register | Public | Craftsman | Submit craftsman registration |
| 14 | GET | /api/craftsmen/{id} | Public | Customer | Get craftsman public profile |
| 15 | GET | /api/craftsmen/search | Public | Customer | Search/filter approved craftsmen |
| 16 | PUT | /api/craftsmen/{id} | Login | Craftsman | Update own craftsman profile |
| 17 | POST | /api/craftsmen/{id}/upload-image | Login | Craftsman | Upload craftsman profile image |
| 18 | POST | /api/jobs | Customer | Customer | Create a new job |
| 19 | PUT | /api/jobs/{id}/accept | Craftsman | Craftsman | Accept an open job |
| 20 | PUT | /api/jobs/{id}/reject | Craftsman | Craftsman | Reject an open job |
| 21 | PUT | /api/jobs/{id}/complete | Craftsman | Craftsman | Mark job as complete |
| 22 | GET | /api/jobs/customer/{id} | Login | Customer | Get all jobs for a customer |
| 23 | GET | /api/jobs/craftsman/{id} | Login | Craftsman | Get all jobs for a craftsman |
| 24 | POST | /api/reviews | Customer | Customer | Submit a review for completed job |
| 25 | GET | /api/reviews/craftsman/{craftsmanId} | Public | Customer | Get craftsman reviews with stats |
| 26 | POST | /api/reviews/rag-feedback | Login | Customer | Submit AI guide feedback |
| 27 | POST | /api/conversations | Login | Customer/Craftsman | Create or get existing conversation |
| 28 | GET | /api/conversations | Login | Customer/Craftsman | Get user's conversation list |
| 29 | GET | /api/conversations/{id} | Login | Customer/Craftsman | Get conversation details |
| 30 | GET | /api/conversations/{id}/messages | Login | Customer/Craftsman | Get paginated messages |
| 31 | PUT | /api/conversations/{id}/read | Login | Customer/Craftsman | Mark conversation as read |
| 32 | GET | /api/notifications | Login | Customer/Craftsman | Get user notifications |
| 33 | GET | /api/notifications/unread-count | Login | Customer/Craftsman | Get unread notification count |
| 34 | PUT | /api/notifications/{id}/read | Login | Customer/Craftsman | Mark notification as read |
| 35 | PUT | /api/notifications/read-all | Login | Customer/Craftsman | Mark all notifications as read |
| 36 | GET | /api/AI/welcome | Public | AI | Get welcome message |
| 37 | POST | /api/AI/chat3 | Public | AI | AI conversational search for craftsmen |
| 38 | POST | /api/AI/ingest/craftsmen | Login | AI | Ingest craftsmen into vector DB |
| 39 | POST | /api/AI/ingest/jobs | Login | AI | Ingest job solutions into vector DB |
| 40 | GET | /api/AI/vectors/count | Login | AI | Get total vector count |
| 41 | GET | /api/v1/admin/craftsmen/pending | Admin | Admin | Get pending craftsmen |
| 42 | GET | /api/v1/admin/craftsmen/approved | Admin | Admin | Get approved craftsmen |
| 43 | GET | /api/v1/admin/craftsmen/rejected | Admin | Admin | Get rejected craftsmen |
| 44 | GET | /api/v1/admin/craftsmen/{id} | Admin | Admin | Get craftsman details |
| 45 | PUT | /api/v1/admin/craftsmen/{id}/approve | Admin | Admin | Approve a pending craftsman |
| 46 | PUT | /api/v1/admin/craftsmen/{id}/reject | Admin | Admin | Reject a pending craftsman |
| 47 | PUT | /api/v1/admin/craftsmen/{id}/suspend | Admin | Admin | Suspend an approved craftsman |
| 48 | DELETE | /api/v1/admin/craftsmen/{id} | Admin | Admin | Soft-delete a craftsman |
| 49 | GET | /api/v1/admin/users | Admin | Admin | Get all users with filters |
| 50 | GET | /api/v1/admin/users/{id} | Admin | Admin | Get user details |
| 51 | GET | /api/v1/admin/users/{id}/activity | Admin | Admin | Get user activity log |
| 52 | PUT | /api/v1/admin/users/{id}/deactivate | Admin | Admin | Deactivate a user |
| 53 | PUT | /api/v1/admin/users/{id}/reactivate | Admin | Admin | Reactivate a user |
| 54 | DELETE | /api/v1/admin/users/{id} | Admin | Admin | Soft-delete a user |
| 55 | GET | /api/v1/admin/jobs | Admin | Admin | Get all jobs with filters |
| 56 | GET | /api/v1/admin/jobs/{id} | Admin | Admin | Get job details |
| 57 | PUT | /api/v1/admin/jobs/{id}/status | Admin | Admin | Update job status manually |
| 58 | PUT | /api/v1/admin/jobs/{id}/flag-dispute | Admin | Admin | Flag a job as disputed |
| 59 | PUT | /api/v1/admin/jobs/{id}/resolve-dispute | Admin | Admin | Resolve a job dispute |
| 60 | GET | /api/v1/admin/jobs/{id}/chat-metadata | Admin | Admin | Get job chat metadata |
| 61 | GET | /api/v1/admin/jobs/{id}/chat-messages | Admin | Admin | Get job chat messages |
| 62 | GET | /api/v1/admin/reviews | Admin | Admin | Get all reviews with filters |
| 63 | GET | /api/v1/admin/reviews/{id} | Admin | Admin | Get review details |
| 64 | DELETE | /api/v1/admin/reviews/{id} | Admin | Admin | Soft-delete a review |
| 65 | GET | /api/v1/admin/reports | Admin | Admin | Get user reports |
| 66 | PUT | /api/v1/admin/reports/{id}/resolve | Admin | Admin | Resolve a report |
| 67 | GET | /api/v1/admin/ai-logs | Admin | Admin | Get AI interaction logs |
| 68 | GET | /api/v1/admin/analytics/overview | Admin | Admin | Get platform overview stats |
| 69 | GET | /api/v1/admin/analytics/craftsmen | Admin | Admin | Get craftsman analytics |
| 70 | GET | /api/v1/admin/analytics/jobs | Admin | Admin | Get job analytics |
| 71 | GET | /api/v1/admin/analytics/ai | Admin | Admin | Get AI usage analytics |
| 72 | GET | /api/v1/admin/analytics/reviews | Admin | Admin | Get review analytics |
| 73 | GET | /api/v1/admin/analytics/export | Admin | Admin | Export data as CSV |
| 74 | GET | /api/v1/admin/config/service-types | Admin | Admin | Get all service types |
| 75 | POST | /api/v1/admin/config/service-types | Admin | Admin | Create a service type |
| 76 | PUT | /api/v1/admin/config/service-types/{id} | Admin | Admin | Update a service type |
| 77 | DELETE | /api/v1/admin/config/service-types/{id} | Admin | Admin | Delete a service type |
| 78 | GET | /api/v1/admin/config/cities | Admin | Admin | Get all cities |
| 79 | POST | /api/v1/admin/config/cities | Admin | Admin | Create a city |
| 80 | PUT | /api/v1/admin/config/cities/{id} | Admin | Admin | Update a city |
| 81 | DELETE | /api/v1/admin/config/cities/{id} | Admin | Admin | Delete a city |
| 82 | GET | /api/v1/admin/config/feature-flags | Admin | Admin | Get all feature flags |
| 83 | PUT | /api/v1/admin/config/feature-flags/{key} | Admin | Admin | Toggle a feature flag |
| 84 | GET | /api/v1/admin/audit-logs | Admin | Admin | Get audit logs with filters |
| 85 | GET | /api/v1/admin/audit-logs/{id} | Admin | Admin | Get audit log details |

**Total: 85 HTTP endpoints**

**SignalR Hubs:**
| # | Hub | Path | Auth | Description |
|---|-----|------|------|-------------|
| 1 | ChatHub | /hubs/chat | JWT | Real-time messaging |
| 2 | NotificationHub | /hubs/notifications | JWT | Real-time notifications |

---

## Group 1 — Admin Endpoints
All endpoints in this group require `[Authorize(Roles = "admin")]` and are under `api/v1/admin`.

---

### 1. GET /api/v1/admin/craftsmen/pending

**What it does:** Returns a paginated list of craftsmen who registered but have not been approved yet.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Pending Craftsmen page, for reviewing new registrations.

#### Request

**Route Parameters:** None

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| page | int | No | 1 | Which page of results to return |
| pageSize | int | No | 20 | How many results per page |
| city | string | No | — | Filter by city name (partial match) |
| serviceType | string | No | — | Filter by service type (partial match) |

**Request Body:** None — no request body needed

#### Response

**Success Response — 200**
```json
{
  "items": [
    {
      "id": 1,
      "userId": 5,
      "fullName": "أحمد محمد",
      "email": "ahmed@example.com",
      "phone": "01001234567",
      "serviceType": "سباك",
      "city": "القاهرة",
      "neighborhood": "مدينة نصر",
      "experience": 8,
      "nationalIdUrl": "/uploads/national/abc123.jpg",
      "bio": "سباك محترف خبرة 8 سنوات",
      "createdAt": "2026-06-01T10:00:00Z"
    }
  ],
  "totalCount": 15,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 401 | No JWT | { "status": 401, "message": "غير مصرح", "timestamp": "..." } |
| 403 | Not admin | { "status": 403, "message": "ممنوع الوصول", "timestamp": "..." } |

#### How It Works (Step by Step)
1. Request arrives at `AdminController.GetPendingCraftsmen()`
2. `[Authorize(Roles = "admin")]` checks admin role from JWT
3. Controller calls `AdminService.GetPendingCraftsmenAsync(page, pageSize, city, serviceType)`
4. Service queries `CraftsmanRepository.GetAllWithUserQuery()` with filter `!IsApproved && !IsDeleted`
5. If city/serviceType provided, applies additional `.Where()` filters
6. Results ordered by `CreatedAt` descending, then paginated
7. Returns `PagedResult<PendingCraftsmanDto>` with items and pagination metadata

#### Frontend Integration Notes
- Send `Authorization: Bearer {adminToken}` header
- Response is paginated — display page numbers using `totalPages` and `totalCount`
- Show the `nationalIdUrl` as a clickable link to view the uploaded ID card image
- Use `createdAt` to show "registered X days ago"

---

### 2. GET /api/v1/admin/craftsmen/approved

**What it does:** Returns a paginated list of approved craftsmen with optional filters.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Approved Craftsmen page.

#### Request

**Route Parameters:** None

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| page | int | No | 1 | Page number |
| pageSize | int | No | 20 | Results per page |
| city | string | No | — | Filter by city |
| serviceType | string | No | — | Filter by service type |
| minRating | decimal | No | — | Minimum rating filter |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "items": [
    {
      "id": 1,
      "userId": 5,
      "fullName": "أحمد محمد",
      "email": "ahmed@example.com",
      "phone": "01001234567",
      "serviceType": "سباك",
      "city": "القاهرة",
      "neighborhood": "مدينة نصر",
      "experience": 8,
      "rating": 4.5,
      "isAvailable": true,
      "bio": "سباك محترف",
      "createdAt": "2026-06-01T10:00:00Z"
    }
  ],
  "totalCount": 30,
  "page": 1,
  "pageSize": 20,
  "totalPages": 2
}
```

**Possible Error Responses:** Same as endpoint 1.

#### How It Works
1. Request arrives at `AdminController.GetApprovedCraftsmen()`
2. Controller calls `AdminService.GetApprovedCraftsmenAsync()` with filters
3. Service queries `CraftsmanRepository.GetAllWithUserQuery()` with `IsApproved && !IsDeleted`
4. Applies optional city, serviceType, and minRating filters
5. Paginates and returns `PagedResult<ApprovedCraftsmanDto>`

---

### 3. GET /api/v1/admin/craftsmen/rejected

**What it does:** Returns a paginated list of rejected (soft-deleted) craftsmen.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Rejected Craftsmen page.

#### Request

**Route Parameters:** None

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| page | int | No | 1 | Page number |
| pageSize | int | No | 20 | Results per page |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "items": [
    {
      "id": 3,
      "userId": 8,
      "fullName": "محمد علي",
      "email": "mohamed@example.com",
      "phone": "01004567890",
      "serviceType": "كهربائي",
      "city": "الإسكندرية",
      "rejectionReason": "مستندات غير مكتملة",
      "createdAt": "2026-05-20T10:00:00Z",
      "deletedAt": "2026-05-25T10:00:00Z"
    }
  ],
  ...
}
```

---

### 4. GET /api/v1/admin/craftsmen/{id}

**What it does:** Returns full details of a specific craftsman including stats.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Click on a craftsman to see full profile.

#### Request

**Route Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The craftsman ID |

**Query Parameters:** None

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "id": 1,
  "userId": 5,
  "fullName": "أحمد محمد",
  "email": "ahmed@example.com",
  "phone": "01001234567",
  "profileImageUrl": "/profiles/abc.jpg",
  "serviceType": "سباك",
  "city": "القاهرة",
  "neighborhood": "مدينة نصر",
  "priceRangeMin": 200.00,
  "priceRangeMax": 1000.00,
  "experience": 8,
  "isApproved": true,
  "isAvailable": true,
  "isDeleted": false,
  "rating": 4.5,
  "bio": "سباك محترف",
  "nationalIdUrl": "/uploads/national/abc123.jpg",
  "rejectionReason": null,
  "deletionReason": null,
  "createdAt": "2026-06-01T10:00:00Z",
  "updatedAt": "2026-06-05T10:00:00Z",
  "completedJobsCount": 12,
  "totalReviews": 8
}
```

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 404 | Craftsman not found | { "status": 404, "message": "الحرفي غير موجود", "timestamp": "..." } |

---

### 5. PUT /api/v1/admin/craftsmen/{id}/approve

**What it does:** Approves a pending craftsman registration, making them visible in search results.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Pending Craftsmen → Approve button.

#### Request

**Route Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The craftsman ID |

**Request Body:**
```json
{
  "notifyMessage": "optional string — welcome message to send to craftsman"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم اعتماد الحرفي بنجاح",
  "data": null
}
```

#### How It Works
1. `AdminController.ApproveCraftsman()` calls `AdminService.ApproveCraftsmanAsync()`
2. Service sets `IsApproved = true` on the craftsman
3. Creates an `AdminAuditLog` entry with action "approve_craftsman"
4. Sends a notification to the craftsman via `NotificationService`

---

### 6. PUT /api/v1/admin/craftsmen/{id}/reject

**What it does:** Rejects a pending craftsman registration with a reason.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Pending Craftsmen → Reject with reason dialog.

#### Request

**Route Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The craftsman ID |

**Request Body:**
```json
{
  "reason": "required string — سبب الرفض"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم رفض الحرفي بنجاح",
  "data": null
}
```

---

### 7. PUT /api/v1/admin/craftsmen/{id}/suspend

**What it does:** Suspends an approved craftsman, removing them from search results.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Approved Craftsmen → Suspend with reason.

#### Request

**Route Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The craftsman ID |

**Request Body:**
```json
{
  "reason": "required string — سبب الإيقاف"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم إيقاف الحرفي بنجاح",
  "data": null
}
```

---

### 8. DELETE /api/v1/admin/craftsmen/{id}

**What it does:** Soft-deletes a craftsman (sets IsDeleted flag, never actually removes from DB).

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Craftsman detail → Delete with reason.

#### Request

**Route Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The craftsman ID |

**Request Body:**
```json
{
  "reason": "required string — سبب الحذف"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم حذف الحرفي بنجاح",
  "data": null
}
```

---

### 9. GET /api/v1/admin/users

**What it does:** Returns a paginated list of all platform users with optional role, status, and search filters.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → User Management page.

#### Request

**Route Parameters:** None

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| role | string | No | — | Filter by role: admin, customer, craftsman |
| page | int | No | 1 | Page number |
| pageSize | int | No | 20 | Results per page |
| isActive | bool | No | — | Filter by active/inactive status |
| search | string | No | — | Search by name or email |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Esraa Admin",
      "email": "admin@harfi.com",
      "phone": null,
      "role": "admin",
      "isActive": true,
      "isVerified": true,
      "isDeleted": false,
      "profileImageUrl": null,
      "createdAt": "2026-05-24T11:18:00Z"
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

**What it does:** Returns full details of a specific user including their craftsman profile ID and job/review counts.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Click on a user to see full details.

#### Request

**Route Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The user ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "id": 5,
  "name": "أحمد محمد",
  "email": "ahmed@example.com",
  "phone": "01001234567",
  "role": "craftsman",
  "isActive": true,
  "isVerified": true,
  "isDeleted": false,
  "profileImageUrl": "/profiles/abc.jpg",
  "deletionReason": null,
  "deletedAt": null,
  "createdAt": "2026-06-01T10:00:00Z",
  "craftsmanProfileId": 1,
  "jobsCount": 15,
  "reviewsCount": 8
}
```

---

### 11. GET /api/v1/admin/users/{id}/activity

**What it does:** Returns the activity log for a specific user.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → User detail → Activity tab.

#### Request

**Route Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The user ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
[
  {
    "action": "approve_craftsman",
    "details": "تم اعتماد الحرفي بواسطة الأدمن",
    "timestamp": "2026-06-05T10:00:00Z"
  }
]
```

---

### 12. PUT /api/v1/admin/users/{id}/deactivate

**What it does:** Deactivates a user account, preventing login. Craftsmen in-progress jobs are handled.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → User Management → Deactivate button.

#### Request

**Route Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The user ID |

**Request Body:**
```json
{
  "reason": "required string — سبب إلغاء التفعيل"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم إلغاء تفعيل المستخدم بنجاح",
  "data": null
}
```

---

### 13. PUT /api/v1/admin/users/{id}/reactivate

**What it does:** Reactivates a previously deactivated user account.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The user ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم إعادة تفعيل المستخدم بنجاح",
  "data": null
}
```

---

### 14. DELETE /api/v1/admin/users/{id}

**What it does:** Soft-deletes a user account (sets IsDeleted flag).

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The user ID |

**Request Body:**
```json
{
  "reason": "required string — سبب الحذف"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم حذف المستخدم بنجاح",
  "data": null
}
```

---

### 15. GET /api/v1/admin/jobs

**What it does:** Returns a paginated list of all jobs with optional status, craftsman, customer, and date range filters.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Jobs Management page.

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| status | string | No | — | Filter by job status |
| craftsmanId | int | No | — | Filter by craftsman ID |
| customerId | int | No | — | Filter by customer ID |
| page | int | No | 1 | Page number |
| pageSize | int | No | 20 | Results per page |
| from | datetime | No | — | Filter jobs created after this date |
| to | datetime | No | — | Filter jobs created before this date |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "items": [
    {
      "id": 1,
      "customerId": 3,
      "customerName": "محمود حسن",
      "craftsmanId": 1,
      "craftsmanName": "أحمد محمد",
      "status": "قيد التنفيذ",
      "serviceType": "سباك",
      "description": "تسريب مياه في الحمام",
      "address": "12 شارع النصر، مدينة نصر",
      "isDisputed": false,
      "createdAt": "2026-06-01T10:00:00Z",
      "completedAt": null
    }
  ],
  ...
}
```

---

### 16. GET /api/v1/admin/jobs/{id}

**What it does:** Returns full details of a specific job including dispute information.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The job ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "id": 1,
  "customerId": 3,
  "customerName": "محمود حسن",
  "craftsmanId": 1,
  "craftsmanName": "أحمد محمد",
  "status": "مكتمل",
  "serviceType": "سباك",
  "description": "تسريب مياه في الحمام",
  "address": "12 شارع النصر",
  "preferredDate": "2026-06-05T10:00:00Z",
  "problemImageUrl": "/uploads/jobs/img.jpg",
  "problemDescription": "حنفية المطبخ بتقطر",
  "solutionDescription": "تم تغيير الحنفية",
  "isDisputed": false,
  "disputeRaisedAt": null,
  "disputeResolvedAt": null,
  "disputeResolution": null,
  "createdAt": "2026-06-01T10:00:00Z",
  "completedAt": "2026-06-06T10:00:00Z",
  "updatedAt": "2026-06-06T10:00:00Z"
}
```

---

### 17. PUT /api/v1/admin/jobs/{id}/status

**What it does:** Manually updates the status of a job (admin override).

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The job ID |

**Request Body:**
```json
{
  "status": "مكتمل",
  "justification": "required string — مبرر التعديل"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم تحديث حالة الطلب بنجاح",
  "data": null
}
```

---

### 18. PUT /api/v1/admin/jobs/{id}/flag-dispute

**What it does:** Flags a job as disputed with a reason.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The job ID |

**Request Body:**
```json
{
  "reason": "required string — سبب النزاع"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم الإبلاغ عن نزاع بنجاح",
  "data": null
}
```

---

### 19. PUT /api/v1/admin/jobs/{id}/resolve-dispute

**What it does:** Resolves an active dispute on a job with resolution notes and favored party.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The job ID |

**Request Body:**
```json
{
  "resolution": "required string — تفاصيل الحل",
  "favoredParty": "required string — الطرف الرابح (customer/craftsman)"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم حل النزاع بنجاح",
  "data": null
}
```

---

### 20. GET /api/v1/admin/jobs/{id}/chat-metadata

**What it does:** Returns metadata about the conversation associated with a job.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The job ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "conversationId": 1,
  "jobId": 1,
  "customerName": "محمود حسن",
  "craftsmanName": "أحمد محمد",
  "messageCount": 24,
  "createdAt": "2026-06-01T10:00:00Z",
  "lastMessageAt": "2026-06-05T15:30:00Z"
}
```

---

### 21. GET /api/v1/admin/jobs/{id}/chat-messages

**What it does:** Returns all messages from the conversation associated with a job for admin review.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The job ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
[
  {
    "id": 1,
    "conversationId": 1,
    "senderId": 3,
    "senderName": "محمود حسن",
    "senderAvatar": null,
    "content": "السلام عليكم، ممكن تجي النهارده؟",
    "messageType": "text",
    "isRead": true,
    "sentAt": "2026-06-01T10:00:00Z"
  }
]
```

---

### 22. GET /api/v1/admin/reviews

**What it does:** Returns a paginated list of all reviews with optional craftsman and star range filters.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| craftsmanId | int | No | — | Filter by craftsman |
| minStars | int | No | — | Minimum star rating |
| maxStars | int | No | — | Maximum star rating |
| page | int | No | 1 | Page number |
| pageSize | int | No | 20 | Results per page |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "items": [
    {
      "id": 1,
      "jobId": 5,
      "customerId": 3,
      "customerName": "محمود حسن",
      "craftsmanId": 1,
      "craftsmanName": "أحمد محمد",
      "stars": 5,
      "comment": "حرفي محترم وشغله نضيف",
      "isDeleted": false,
      "createdAt": "2026-06-06T10:00:00Z"
    }
  ],
  ...
}
```

---

### 23. GET /api/v1/admin/reviews/{id}

**What it does:** Returns full details of a specific review including deletion information.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The review ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "id": 1,
  "jobId": 5,
  "jobDescription": "تسريب مياه في الحمام",
  "customerId": 3,
  "customerName": "محمود حسن",
  "craftsmanId": 1,
  "craftsmanName": "أحمد محمد",
  "stars": 5,
  "comment": "حرفي محترم",
  "isDeleted": false,
  "deletionReason": null,
  "deletedAt": null,
  "createdAt": "2026-06-06T10:00:00Z"
}
```

---

### 24. DELETE /api/v1/admin/reviews/{id}

**What it does:** Soft-deletes a review with a reason.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The review ID |

**Request Body:**
```json
{
  "reason": "required string — سبب حذف التقييم"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم حذف التقييم بنجاح",
  "data": null
}
```

---

### 25. GET /api/v1/admin/reports

**What it does:** Returns a paginated list of user-submitted reports with optional status and type filters.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Reports page.

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| status | string | No | — | Filter by status: pending, resolved |
| type | string | No | — | Filter by target type |
| page | int | No | 1 | Page number |
| pageSize | int | No | 20 | Results per page |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "items": [
    {
      "id": 1,
      "reportedByUserId": 3,
      "reportedByUserName": "محمود حسن",
      "targetType": "craftsman",
      "targetId": 1,
      "reason": "لم يكمل الشغل",
      "status": "pending",
      "resolvedByAdminId": null,
      "resolutionNotes": null,
      "createdAt": "2026-06-07T10:00:00Z",
      "resolvedAt": null
    }
  ],
  ...
}
```

---

### 26. PUT /api/v1/admin/reports/{id}/resolve

**What it does:** Resolves a user report with an action and notes.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The report ID |

**Request Body:**
```json
{
  "action": "required string — الإجراء المتخذ",
  "notes": "required string — ملاحظات"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم حل البلاغ بنجاح",
  "data": null
}
```

---

### 27. GET /api/v1/admin/ai-logs

**What it does:** Returns a paginated list of AI chat interactions for auditing.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → AI Logs page.

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| page | int | No | 1 | Page number |
| pageSize | int | No | 20 | Results per page |
| from | datetime | No | — | Filter logs after this date |
| to | datetime | No | — | Filter logs before this date |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "items": [
    {
      "id": 1,
      "userId": 3,
      "userName": "محمود حسن",
      "sessionId": "guid-here",
      "role": "user",
      "content": "عاوز سباك في القاهرة",
      "toolUsed": null,
      "tokensUsed": 150,
      "createdAt": "2026-06-07T10:00:00Z"
    }
  ],
  ...
}
```

---

### 28. GET /api/v1/admin/analytics/overview

**What it does:** Returns a high-level summary of platform statistics.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Home/Overview page.

#### Request

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "totalUsers": 150,
  "totalCraftsmen": 45,
  "pendingCraftsmen": 5,
  "activeJobs": 20,
  "completedJobs": 100,
  "disputedJobs": 2,
  "pendingReports": 3,
  "totalReviews": 80,
  "newUsersThisMonth": 25,
  "averageRating": 4.3
}
```

---

### 29. GET /api/v1/admin/analytics/craftsmen

**What it does:** Returns detailed craftsman analytics including distribution by service type and city.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

Where to use it in the frontend: Admin Dashboard → Analytics → Craftsmen tab.

#### Response

**Success Response — 200**
```json
{
  "totalCraftsmen": 45,
  "pendingApproval": 5,
  "approved": 38,
  "rejected": 2,
  "suspended": 0,
  "averageRating": 4.3,
  "byServiceType": { "سباك": 15, "كهربائي": 12, "نجار": 8 },
  "byCity": { "القاهرة": 20, "الإسكندرية": 10, "الجيزة": 8 }
}
```

---

### 30. GET /api/v1/admin/analytics/jobs

**What it does:** Returns job analytics by status and service type.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Response

**Success Response — 200**
```json
{
  "totalJobs": 120,
  "open": 15,
  "inProgress": 20,
  "completed": 80,
  "rejected": 3,
  "disputed": 2,
  "byServiceType": { "سباك": 40, "كهربائي": 35, "نجار": 25 },
  "averageCompletionDays": 3.5
}
```

---

### 31. GET /api/v1/admin/analytics/ai

**What it does:** Returns AI usage analytics including total chats, token usage, and ingested data.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Response

**Success Response — 200**
```json
{
  "totalChats": 500,
  "totalTokensUsed": 150000,
  "totalCraftsmenIngested": 45,
  "totalSolutionsIngested": 80,
  "averageTokensPerChat": 300
}
```

---

### 32. GET /api/v1/admin/analytics/reviews

**What it does:** Returns review analytics including star distribution.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Response

**Success Response — 200**
```json
{
  "totalReviews": 80,
  "averageStars": 4.3,
  "starDistribution": { "1": 2, "2": 3, "3": 10, "4": 25, "5": 40 },
  "deletedReviews": 1
}
```

---

### 33. GET /api/v1/admin/analytics/export

**What it does:** Exports platform data as a CSV file for download.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Analytics → Export Data button.

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| type | string | No | users | Data type to export: users, craftsmen, jobs, reviews |
| from | datetime | No | — | Filter records after this date |
| to | datetime | No | — | Filter records before this date |

**Request Body:** None

#### Response

**Success Response — 200** (CSV file download)
- Content-Type: `text/csv`
- Content-Disposition: `attachment; filename=harfi-users-20260608.csv`
- Body is raw CSV text

---

### 34. GET /api/v1/admin/config/service-types

**What it does:** Returns all service types configured on the platform.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Service Types.

#### Response

**Success Response — 200**
```json
[
  {
    "id": 1,
    "nameAr": "سباك",
    "nameEn": "Plumber",
    "icon": "🔧",
    "isActive": true
  }
]
```

---

### 35. POST /api/v1/admin/config/service-types

**What it does:** Creates a new service type.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request

**Request Body:**
```json
{
  "nameAr": "required string — الاسم بالعربية",
  "nameEn": "required string — الاسم بالإنجليزية",
  "icon": "optional string — رمز",
  "isActive": true
}
```

#### Response

**Success Response — 201**
```json
{
  "id": 2,
  "nameAr": "سباك",
  "nameEn": "Plumber",
  "icon": "🔧",
  "isActive": true
}
```

---

### 36. PUT /api/v1/admin/config/service-types/{id}

**What it does:** Updates an existing service type.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The service type ID |

**Request Body:** Same shape as create (ServiceTypeDto)

#### Response

**Success Response — 200:** Updated ServiceTypeDto

---

### 37. DELETE /api/v1/admin/config/service-types/{id}

**What it does:** Deletes a service type.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم حذف نوع الخدمة بنجاح",
  "data": null
}
```

---

### 38. GET /api/v1/admin/config/cities

**What it does:** Returns all cities configured on the platform.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Cities.

#### Response

**Success Response — 200**
```json
[
  {
    "id": 1,
    "nameAr": "القاهرة",
    "nameEn": "Cairo",
    "governorate": "القاهرة",
    "isActive": true
  }
]
```

---

### 39. POST /api/v1/admin/config/cities

**What it does:** Creates a new city.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request

**Request Body:**
```json
{
  "nameAr": "required string — الاسم بالعربية",
  "nameEn": "required string — الاسم بالإنجليزية",
  "governorate": "optional string — المحافظة",
  "isActive": true
}
```

#### Response

**Success Response — 201:** Created CityDto

---

### 40. PUT /api/v1/admin/config/cities/{id}

**What it does:** Updates an existing city.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Response

**Success Response — 200:** Updated CityDto

---

### 41. DELETE /api/v1/admin/config/cities/{id}

**What it does:** Deletes a city.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم حذف المدينة بنجاح",
  "data": null
}
```

---

### 42. GET /api/v1/admin/config/feature-flags

**What it does:** Returns all feature flags and their enabled/disabled status.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Settings → Feature Flags.

#### Response

**Success Response — 200**
```json
[
  {
    "key": "ai_chat_enabled",
    "isEnabled": true,
    "updatedAt": "2026-06-01T10:00:00Z"
  }
]
```

---

### 43. PUT /api/v1/admin/config/feature-flags/{key}

**What it does:** Enables or disables a feature flag.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| key | string | Yes | The feature flag key |

**Request Body:**
```json
{
  "isEnabled": true
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم تحديث الخاصية بنجاح",
  "data": null
}
```

---

### 44. GET /api/v1/admin/audit-logs

**What it does:** Returns a paginated list of admin audit logs with optional filters.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

**Where to use it in the frontend:** Admin Dashboard → Audit Logs page.

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| adminId | int | No | — | Filter by admin who performed the action |
| action | string | No | — | Filter by action type |
| targetType | string | No | — | Filter by target entity type |
| from | datetime | No | — | Filter logs after this date |
| to | datetime | No | — | Filter logs before this date |
| page | int | No | 1 | Page number |
| pageSize | int | No | 20 | Results per page |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "items": [
    {
      "id": 1,
      "adminId": 1,
      "adminName": "Esraa Admin",
      "action": "approve_craftsman",
      "targetType": "Craftsman",
      "targetId": 1,
      "notes": "تم الاعتماد",
      "ipAddress": "192.168.1.1",
      "createdAt": "2026-06-05T10:00:00Z"
    }
  ],
  ...
}
```

---

### 45. GET /api/v1/admin/audit-logs/{id}

**What it does:** Returns details of a specific audit log entry.

**Who uses it:** Admin

**Authorization:** Requires Role: Admin

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The audit log ID |

**Request Body:** None

#### Response

**Success Response — 200:** Single AuditLogDto

---

## Group 2 — Customer Endpoints
Endpoints used by customers browsing craftsmen, posting jobs, chatting, and leaving reviews.

---

### 1. POST /api/auth/register

**What it does:** Creates a new user account (customer or craftsman).

**Who uses it:** Public — anyone can register

**Authorization:** Public — no token needed

**Where to use it in the frontend:** Registration page (customer or craftsman sign-up).

#### Request

**Request Body:**
```json
{
  "name": "string — required, max 100 chars",
  "email": "string — required, valid email format, max 200 chars",
  "password": "string — required, min 8 chars",
  "confirmPassword": "string — required, must match password",
  "role": "string — required, must be 'customer' or 'craftsman'",
  "phone": "string — optional, valid phone format, max 20 chars"
}
```

#### Response

**Success Response — 201**
```json
{
  "accessToken": "eyJhbGciOiJI...",
  "refreshToken": "base64string...",
  "expiresAt": "2026-06-08T11:00:00Z",
  "user": {
    "id": 10,
    "name": "محمود حسن",
    "email": "mahmoud@example.com",
    "role": "customer",
    "phone": "01001234567",
    "profileImageUrl": null
  }
}
```

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 400 | Invalid role (not customer or craftsman) | { "message": "الدور يجب أن يكون customer أو craftsman" } |
| 400 | Role is "admin" | { "message": "لا يمكن تسجيل حساب أدمن من خلال API التسجيل." } |
| 400 | Email already exists | Exception: "البريد الإلكتروني مسجل مسبقاً. جرب تسجيل الدخول." |
| 400 | Validation failed | ModelState errors |

#### How It Works
1. Request arrives at `AuthController.Register()`
2. If role is "admin", returns 400
3. Controller calls `AuthService.RegisterAsync(dto)`
4. Service checks if email is already registered
5. Creates `User` entity via `UserManager.CreateAsync()`
6. Generates email verification code (6 digits) + Identity token
7. Sends verification email via `EmailService` (non-blocking)
8. Returns `AuthResponseDto` with JWT tokens

#### Frontend Integration Notes
- Store both `accessToken` and `refreshToken` in localStorage or secure storage
- `accessToken` expires in 60 minutes (configurable)
- `refreshToken` lasts 30 days
- After registration, show the user a message to check their email and verify using endpoint #5
- Use `expiresAt` to know when the access token will expire

---

### 2. POST /api/auth/login

**What it does:** Authenticates a user and returns JWT tokens.

**Who uses it:** Public — any registered user

**Authorization:** Public — no token needed

**Where to use it in the frontend:** Login page.

#### Request

**Request Body:**
```json
{
  "email": "string — required, valid email",
  "password": "string — required"
}
```

#### Response

**Success Response — 200:** Same shape as register response (AuthResponseDto)

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 401 | Wrong credentials | Exception: "البريد الإلكتروني أو كلمة المرور غير صحيحة." |
| 401 | Account inactive | Exception: "الحساب غير مفعّل. تواصل مع الدعم." |
| 401 | Email not verified | Exception: "البريد الإلكتروني غير مفعّل. تحقق من بريدك الإلكتروني." |

#### How It Works
1. `AuthController.Login()` calls `AuthService.LoginAsync(dto)`
2. Finds user by email via `UserManager.FindByEmailAsync()`
3. Checks password via `UserManager.CheckPasswordAsync()`
4. Checks `IsActive` and `IsVerified` flags
5. If all pass, builds JWT with claims (userId, email, role, name)
6. Creates new refresh token, returns `AuthResponseDto`

#### Frontend Integration Notes
- On success, store tokens as described in endpoint #1
- If 401 with "البريد الإلكتروني غير مفعّل", redirect to verification page
- The JWT includes the user's role in claims — use it for frontend routing

---

### 3. POST /api/auth/refresh

**What it does:** Exchanges a valid refresh token for a new access token.

**Who uses it:** Any logged-in user (when access token expires)

**Authorization:** Public — uses refresh token instead

**Where to use it in the frontend:** HTTP interceptor — automatically when 401 response received.

#### Request

**Request Body:**
```json
{
  "refreshToken": "string — required, the refresh token from previous auth response"
}
```

#### Response

**Success Response — 200:** Same shape as login response (new tokens, user info)

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 401 | Token invalid, revoked, or expired | Exception: "رمز التحديث غير صالح أو منتهي الصلاحية." |

#### How It Works
1. `AuthService.RefreshTokenAsync()` finds the refresh token in DB
2. Validates it's not revoked and not expired
3. Revokes the old token (rotation — one-time use)
4. Generates and returns new access + refresh tokens

---

### 4. POST /api/auth/logout

**What it does:** Revokes the refresh token, effectively logging the user out.

**Who uses it:** Any logged-in user

**Authorization:** Requires Login

**Where to use it in the frontend:** Logout button.

#### Request

**Request Body:**
```json
{
  "refreshToken": "string — required"
}
```

#### Response

**Success Response — 200**
```json
{
  "message": "تم تسجيل الخروج بنجاح"
}
```

#### Frontend Integration Notes
- Always call this on logout to invalidate the refresh token
- Clear both tokens from storage on the client side as well

---

### 5. POST /api/auth/verify-email

**What it does:** Verifies a user's email using the 6-digit code sent after registration.

**Who uses it:** Public — any newly registered user

**Authorization:** Public — no token needed

**Where to use it in the frontend:** Email verification page (shown after registration).

#### Request

**Request Body:**
```json
{
  "email": "string — required, valid email",
  "code": "string — required, exactly 6 digits"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم تفعيل البريد الإلكتروني بنجاح."
}
```

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 400 | Invalid or expired code | Exception: "الكود غير صحيح أو منتهي الصلاحية." |
| 400 | Email already verified | Exception: "البريد الإلكتروني مفعّل مسبقاً." |
| 404 | User not found | Exception: "المستخدم غير موجود." |

#### Frontend Integration Notes
- Code expires in 10 minutes
- Allow user to request a new code via endpoint #6
- After verification, the `IsVerified` flag is set and the user can now log in

---

### 6. POST /api/auth/resend-code

**What it does:** Resends a new 6-digit email verification code.

**Who uses it:** Public — users who didn't receive or expired their code

**Authorization:** Public — no token needed

**Where to use it in the frontend:** Email verification page → "إعادة إرسال الكود" button.

#### Request

**Request Body:**
```json
{
  "email": "string — required, valid email"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم إعادة إرسال الكود بنجاح."
}
```

#### How It Works
1. Invalidates all previous unused codes for this user
2. Generates new Identity token + new 6-digit code
3. Sends new code via email

---

### 7. POST /api/auth/send-phone-code

**What it does:** Sends a 6-digit phone verification code (currently via email — SMS gateway coming soon).

**Who uses it:** Any logged-in user

**Authorization:** Requires Login

**Where to use it in the frontend:** Profile settings → Verify phone number.

#### Request

**Request Body:**
```json
{
  "email": "string — required, valid email",
  "phoneNumber": "string — required, valid phone number, max 20 chars"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم إرسال الكود بنجاح."
}
```

---

### 8. POST /api/auth/verify-phone

**What it does:** Verifies a phone number using the 6-digit code.

**Who uses it:** Any logged-in user

**Authorization:** Requires Login

**Where to use it in the frontend:** Profile settings → Verify phone → enter code.

#### Request

**Request Body:**
```json
{
  "email": "string — required, valid email",
  "phoneNumber": "string — required, valid phone, max 20 chars",
  "code": "string — required, exactly 6 digits"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم تفعيل رقم الهاتف بنجاح."
}
```

---

### 9. POST /api/auth/resend-phone-code

**What it does:** Resends a new 6-digit phone verification code.

**Who uses it:** Any logged-in user

**Authorization:** Requires Login

#### Request

**Request Body:**
```json
{
  "email": "string — required, valid email",
  "phoneNumber": "string — required, valid phone, max 20 chars"
}
```

#### Response

**Success Response — 200**
```json
{
  "success": true,
  "message": "تم إعادة إرسال الكود بنجاح."
}
```

---

### 10. GET /api/users/profile/{id}

**What it does:** Returns the public profile of a user.

**Who uses it:** Customer or Craftsman — any logged-in user

**Authorization:** Requires Login

**Where to use it in the frontend:** Profile page, settings page.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The user ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "id": 10,
  "name": "محمود حسن",
  "email": "mahmoud@example.com",
  "role": "customer",
  "phone": "01001234567",
  "profileImageUrl": null,
  "isActive": true,
  "createdAt": "2026-06-01T10:00:00Z"
}
```

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 404 | User not found | { "message": "عذراً، هذا المستخدم غير موجود." } |

---

### 11. PUT /api/users/profile/{id}

**What it does:** Updates the user's name and phone number.

**Who uses it:** Customer or Craftsman — the user themselves

**Authorization:** Requires Login

**Where to use it in the frontend:** Profile settings → Edit profile form.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The user ID |

**Request Body:**
```json
{
  "name": "string — required, max 100 chars",
  "phone": "string — required, valid phone, max 20 chars"
}
```

#### Response

**Success Response — 200**
```json
{
  "message": "تم تحديث بيانات الملف الشخصي بنجاح."
}
```

---

### 12. POST /api/users/profile/{id}/upload-image

**What it does:** Uploads a profile image for the user.

**Who uses it:** Customer or Craftsman

**Authorization:** Requires Login

**Where to use it in the frontend:** Profile settings → Change profile photo.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The user ID |

**Request Body:** `multipart/form-data`
- Field name: `file`
- Supported formats: jpg, jpeg, png, webp
- Max size: 5 MB

#### Response

**Success Response — 200**
```json
{
  "url": "/profiles/guid-filename.jpg"
}
```

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 400 | No file selected | { "message": "الرجاء اختيار صورة للرفع." } |
| 400 | Unsupported format | Exception: "تنسيق الصورة غير مدعوم." |
| 400 | File too large | Exception: "حجم الصورة لا يمكن أن يتجاوز 5 ميغابايت." |
| 404 | User not found | { "message": "عذراً، هذا المستخدم غير موجود." } |

#### Frontend Integration Notes
- Use `FormData` — NOT JSON
- Set `Content-Type` to `multipart/form-data` (browser sets this automatically)
- The returned URL is relative — prepend base URL: `http://localhost:5108/profiles/...`

---

### 13. POST /api/craftsmen/register

**What it does:** Submits a craftsman registration application (pending admin approval).

**Who uses it:** Public — requires a registered user account first

**Authorization:** Public — no token needed (but requires existing user)

**Where to use it in the frontend:** Craftsman registration form (after user account is created).

#### Request

**Request Body:**
```json
{
  "userId": "int — required, the user's ID from registration",
  "serviceType": "string — required, max 50 chars, e.g. سباك",
  "city": "string — required, max 100 chars",
  "neighborhood": "string — optional, max 100 chars",
  "priceRangeMin": "decimal — optional, minimum price",
  "priceRangeMax": "decimal — optional, maximum price",
  "experience": "int — required, years of experience",
  "bio": "string — optional, max 1000 chars, about me",
  "nationalIdUrl": "string — required, max 500 chars, URL of uploaded national ID image"
}
```

#### Response

**Success Response — 200**
```json
{
  "message": "تم تقديم طلبك بنجاح وهو قيد المراجعة حالياً."
}
```

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 400 | User not found | Exception: "المستخدم غير موجود." |
| 400 | User role is not craftsman | Exception: "هذا المستخدم ليس لديه صلاحية التسجيل كحرفي." |
| 400 | Already registered as craftsman | Exception: "هذا المستخدم مسجل كحرفي مسبقاً." |

#### How It Works
1. `CraftsmanController.Register()` calls `CraftsmanService.RegisterCraftsmanAsync()`
2. Service finds user by ID and validates their role is "craftsman"
3. Checks no existing craftsman record for this user (1:1 relationship)
4. Creates `Craftsman` entity with `IsApproved = false`
5. Admin must approve before craftsman appears in search

---

### 14. GET /api/craftsmen/{id}

**What it does:** Returns a craftsman's full public profile.

**Who uses it:** Public — no login needed (customer browsing)

**Authorization:** Public — no token needed

**Where to use it in the frontend:** Craftsman profile page.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The craftsman ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "id": 1,
  "userId": 5,
  "fullName": "أحمد محمد",
  "email": "ahmed@example.com",
  "phone": "01001234567",
  "profileImageUrl": "/profiles/abc.jpg",
  "serviceType": "سباك",
  "city": "القاهرة",
  "neighborhood": "مدينة نصر",
  "priceRangeMin": 200.00,
  "priceRangeMax": 1000.00,
  "experience": 8,
  "isApproved": true,
  "isAvailable": true,
  "rating": 4.5,
  "bio": "سباك محترف خبرة 8 سنوات",
  "nationalIdUrl": null,
  "createdAt": "2026-06-01T10:00:00Z"
}
```

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 404 | Craftsman not found | { "message": "عذراً، هذا الحرفي غير موجود حالياً." } |

#### Frontend Integration Notes
- RTL rendering: Arabic text in fields like `fullName`, `bio`, `serviceType`, `city`
- Show `rating` as stars (e.g., 4.5 → ★★★★½)
- Show `isAvailable` as a green/red indicator
- `priceRangeMin` to `priceRangeMax` can be formatted as "200 - 1000 ج.م"

---

### 15. GET /api/craftsmen/search

**What it does:** Searches and filters approved craftsmen by service type, city, minimum rating, and minimum experience.

**Who uses it:** Public — no login needed

**Authorization:** Public — no token needed

**Where to use it in the frontend:** Home page search, Browse Craftsmen page, category filter.

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| serviceType | string | No | — | Filter by trade/service (partial match in Arabic) |
| city | string | No | — | Filter by city (partial match in Arabic) |
| minRating | decimal | No | — | Only show craftsmen with rating >= this value |
| minExperience | int | No | — | Only show craftsmen with experience >= this value |

**Request Body:** None

#### Response

**Success Response — 200**
```json
[
  {
    "id": 1,
    "userId": 5,
    "fullName": "أحمد محمد",
    "email": "ahmed@example.com",
    "phone": "01001234567",
    "profileImageUrl": "/profiles/abc.jpg",
    "serviceType": "سباك",
    "city": "القاهرة",
    "neighborhood": "مدينة نصر",
    "priceRangeMin": 200.00,
    "priceRangeMax": 1000.00,
    "experience": 8,
    "isApproved": true,
    "isAvailable": true,
    "rating": 4.5,
    "bio": "سباك محترف",
    "createdAt": "2026-06-01T10:00:00Z"
  }
]
```

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 404 | No results found | { "message": "لم يتم العثور على أي حرفيين يطابقون محددات البحث الحالية." } |

#### How It Works
1. `CraftsmanController.Search()` calls `CraftsmanService.GetFilteredCraftsmenAsync()`
2. Service delegates to `CraftsmanRepository.GetFilteredCraftsmenAsync()`
3. Repository queries only `IsApproved == true` craftsmen
4. Applies filters using `.Contains()` for Arabic partial matching
5. Orders by rating descending (highest rated first)
6. Returns list of `CraftsmanDto`

#### Frontend Integration Notes
- This is the core discovery endpoint — use it on home page, search, and browse pages
- Results are sorted by rating (highest first) automatically
- Partial matching works for Arabic text (e.g., "سبا" will match "سباك")
- If you need pagination, this endpoint returns all results — implement client-side pagination or contact backend to add it

---

### 16. GET /api/reviews/craftsman/{craftsmanId}

**What it does:** Returns all reviews for a specific craftsman with summary statistics (average stars, total count).

**Who uses it:** Public — no login needed

**Authorization:** Public — no token needed

**Where to use it in the frontend:** Craftsman profile page → Reviews section.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| craftsmanId | int | Yes | The craftsman ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "craftsmanId": 1,
  "totalReviews": 8,
  "averageStars": 4.6,
  "reviews": [
    {
      "id": 1,
      "jobId": 5,
      "stars": 5,
      "comment": "حرفي محترم وشغله نضيف",
      "customerName": "محمود حسن",
      "createdAt": "2026-06-06T10:00:00Z"
    }
  ]
}
```

---

### 17. POST /api/reviews

**What it does:** Submits a star rating (1-5) and optional comment for a completed job.

**Who uses it:** Customer — requires customer role

**Authorization:** Requires Role: Customer

**Where to use it in the frontend:** Job detail page → "تقييم" button (only after job is completed).

#### Request

**Request Body:**
```json
{
  "jobId": "int — required, the completed job ID",
  "stars": "int — required, 1 to 5",
  "comment": "string — optional, max 1000 chars"
}
```

#### Response

**Success Response — 200**
```json
{
  "message": "تم إرسال تقييمك بنجاح",
  "data": {
    "id": 1,
    "jobId": 5,
    "stars": 5,
    "comment": "حرفي محترم",
    "customerName": "محمود حسن",
    "createdAt": "2026-06-06T10:00:00Z"
  }
}
```

**Business Rules (can return 400):**
- Job must be completed (status = "مكتمل")
- Only the job's customer can submit a review
- One review per job (duplicate check)
- Stars must be between 1 and 5
- Comment max 1000 chars

---

### 18. POST /api/reviews/rag-feedback

**What it does:** Submits feedback on an AI self-fix guide (helpful or need craftsman).

**Who uses it:** Any logged-in user

**Authorization:** Requires Login

**Where to use it in the frontend:** AI chat interface → after receiving solution steps, user clicks feedback buttons.

#### Request

**Request Body:**
```json
{
  "ragDocumentId": "int — required, the AI document ID",
  "feedbackType": "string — required, must be 'ساعدني' or 'محتاج حرفي'"
}
```

#### Response

**Success Response — 200**
```json
{
  "message": "شكراً! سعداء أن المحتوى أفادك ✓"
}
```

**Business Rules:**
- `feedbackType` must be one of: `ساعدني` or `محتاج حرفي`
- One feedback per user per RAG document (no duplicates)
- If user role is "craftsman", returns redirect message

---

### 19. POST /api/jobs

**What it does:** Creates a new job request from a customer to a specific craftsman.

**Who uses it:** Customer — requires customer role

**Authorization:** Requires Role: Customer

**Where to use it in the frontend:** Craftsman profile → "طلب خدمة" button → Job creation form.

#### Request

**Request Body:**
```json
{
  "craftsmanId": "int — required, which craftsman",
  "serviceType": "string — required, e.g. سباك",
  "description": "string — required, what needs to be done",
  "address": "string — required, job location",
  "preferredDate": "datetime — optional, ISO 8601 format",
  "problemImageUrl": "string — optional, URL of problem photo",
  "problemDescription": "string — optional, extra details"
}
```

#### Response

**Success Response — 201**
```json
{
  "id": 1,
  "customerId": 3,
  "craftsmanId": 1,
  "status": "مفتوح",
  "serviceType": "سباك",
  "description": "تسريب مياه في الحمام",
  "address": "12 شارع النصر، مدينة نصر",
  "preferredDate": "2026-06-10T10:00:00Z",
  "problemImageUrl": null,
  "problemDescription": "حنفية المطبخ بتقطر",
  "solutionDescription": null,
  "createdAt": "2026-06-08T10:00:00Z",
  "completedAt": null,
  "updatedAt": "2026-06-08T10:00:00Z"
}
```

#### Frontend Integration Notes
- `craftsmanId` comes from the craftsman profile page URL
- The customer's ID is extracted from the JWT token automatically (not sent in body)
- Job status values in Arabic: `مفتوح` (open), `قيد التنفيذ` (in-progress), `مكتمل` (done), `مرفوض` (rejected), `ملغى` (cancelled)

---

### 20. GET /api/jobs/customer/{id}

**What it does:** Returns all jobs for a specific customer.

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login

**Where to use it in the frontend:** Customer profile → My Jobs tab / "طلباتي" page.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The customer's user ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
[
  {
    "id": 1,
    "customerId": 3,
    "craftsmanId": 1,
    "status": "قيد التنفيذ",
    "serviceType": "سباك",
    "description": "تسريب مياه",
    "address": "12 شارع النصر",
    "preferredDate": "2026-06-10T10:00:00Z",
    "problemImageUrl": null,
    "problemDescription": null,
    "solutionDescription": null,
    "createdAt": "2026-06-08T10:00:00Z",
    "completedAt": null,
    "updatedAt": "2026-06-08T10:00:00Z"
  }
]
```

---

### 21. POST /api/conversations

**What it does:** Creates a new conversation (chat) or returns an existing one for a job between customer and craftsman.

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login

**Where to use it in the frontend:** After creating a job → "محادثة" button, or from inbox → new message.

#### Request

**Request Body:**
```json
{
  "jobId": "int — required, must be >= 1",
  "craftsmanId": "int — required, must be >= 1"
}
```

#### Response

**Success Response — 200**
```json
{
  "id": 1,
  "jobId": 1,
  "otherUserId": 5,
  "otherUserName": "أحمد محمد",
  "otherUserAvatar": "/profiles/abc.jpg",
  "lastMessage": null,
  "lastMessageAt": null,
  "unreadCount": 0
}
```

**Business Rules:**
- Cannot create conversation with yourself
- Job must exist and not be rejected or cancelled
- If conversation already exists for this job, returns it (idempotent)

---

### 22. GET /api/conversations

**What it does:** Returns all conversations for the currently logged-in user (their inbox).

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login

**Where to use it in the frontend:** Inbox page / "الرسائل" page.

#### Request

**Query Parameters:** None

**Request Body:** None

#### Response

**Success Response — 200**
```json
[
  {
    "id": 1,
    "jobId": 1,
    "otherUserId": 5,
    "otherUserName": "أحمد محمد",
    "otherUserAvatar": "/profiles/abc.jpg",
    "lastMessage": "تمام هاجي بكره",
    "lastMessageAt": "2026-06-08T10:00:00Z",
    "unreadCount": 2
  }
]
```

#### Frontend Integration Notes
- `otherUserId` and `otherUserName` are dynamically set based on who is viewing
- If customer views, `otherUser` = craftsman; if craftsman views, `otherUser` = customer
- `unreadCount` shows how many messages the user hasn't read yet — show as badge

---

### 23. GET /api/conversations/{id}

**What it does:** Returns details of a specific conversation (with other user info and last message).

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login (must be participant)

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The conversation ID (must be > 0) |

**Request Body:** None

#### Response

**Success Response — 200:** Single ConversationDto

**Possible Error Responses:**
| Code | When | Response |
|------|------|----------|
| 400 | Invalid ID | "معرف المحادثة غير صالح." |
| 403 | Not a participant | Forbid |
| 404 | Not found | "المحادثة غير موجودة أو الوصول مرفوض." |

---

### 24. GET /api/conversations/{id}/messages

**What it does:** Returns paginated messages for a conversation.

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login (must be participant)

**Where to use it in the frontend:** Chat window inside a conversation.

#### Request
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| id | int | Yes | — | The conversation ID |
| page | int | No | 1 | Page number (must be >= 1) |
| pageSize | int | No | 20 | Results per page (1-100) |

**Request Body:** None

#### Response

**Success Response — 200**
```json
[
  {
    "id": 1,
    "conversationId": 1,
    "senderId": 3,
    "senderName": "محمود حسن",
    "senderAvatar": null,
    "content": "السلام عليكم",
    "messageType": "text",
    "isRead": true,
    "sentAt": "2026-06-08T09:00:00Z"
  }
]
```

#### Frontend Integration Notes
- Messages are returned newest-first? Check ordering — actually they should be ordered by `SentAt`
- Use SignalR (ChatHub) for real-time messages instead of polling
- This endpoint is for loading history on page open
- If page 1 + pageSize 20 returns fewer than 20 items, there are no more pages

---

### 25. PUT /api/conversations/{id}/read

**What it does:** Marks all messages in a conversation as read for the current user.

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login (must be participant)

**Where to use it in the frontend:** When user opens a conversation in the chat window.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The conversation ID (must be > 0) |

**Request Body:** None

#### Response

**Success Response — 204:** No content (empty body)

---

### 26. GET /api/notifications

**What it does:** Returns all notifications for the currently logged-in user.

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login

**Where to use it in the frontend:** Notifications dropdown or Notifications page.

#### Request

**Request Body:** None

#### Response

**Success Response — 200**
```json
[
  {
    "id": 1,
    "title": "رسالة جديدة من أحمد محمد",
    "body": "تمام هاجي بكره...",
    "type": "new_message",
    "relatedJobId": 1,
    "isRead": false,
    "createdAt": "2026-06-08T10:00:00Z"
  }
]
```

---

### 27. GET /api/notifications/unread-count

**What it does:** Returns the count of unread notifications for the user.

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login

**Where to use it in the frontend:** Navigation bar → notification bell badge.

#### Request

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "unreadCount": 3
}
```

---

### 28. PUT /api/notifications/{id}/read

**What it does:** Marks a single notification as read.

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The notification ID (must be > 0) |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "message": "تم تحديث الإشعار بنجاح"
}
```

**Possible Error Responses:**
| Code | When |
|------|------|
| 400 | Invalid ID |
| 404 | Notification not found or doesn't belong to user |

---

### 29. PUT /api/notifications/read-all

**What it does:** Marks all notifications as read for the current user.

**Who uses it:** Shared endpoint — Customer or Craftsman

**Authorization:** Requires Login

**Where to use it in the frontend:** "تحديد الكل كمقروء" button.

#### Response

**Success Response — 204:** No content

---

### 30. POST /api/AI/welcome

**What it does:** Returns the AI assistant's welcome message.

**Who uses it:** Public — no login needed

**Authorization:** Public

**Where to use it in the frontend:** AI assistant chat → initial welcome bubble.

#### Response

**Success Response — 200**
```json
{
  "message": "أهلاً بك! 👋\nأنا مساعدك الذكي للعثور على أفضل الحرفيين في مصر.\nأخبرني بمشكلتك وسأجد لك الحرفي المناسب فوراً! 🔧"
}
```

---

### 31. POST /api/AI/chat3

**What it does:** The main AI conversational endpoint. Users describe their problem, the AI identifies the service type and city needed, and either returns solution steps or finds matching craftsmen.

**Who uses it:** Public — no login needed

**Authorization:** Public

**Where to use it in the frontend:** AI Assistant chat page (the main conversational interface).

#### Request

**Request Body:**
```json
{
  "messages": [
    {
      "role": "user",
      "content": "عاوز سباك في القاهرة"
    }
  ],
  "extractedService": "string or null — pass what was extracted in previous response",
  "extractedCity": "string or null",
  "extractedCount": "int or null",
  "failedServiceAttempts": 0,
  "failedCityAttempts": 0,
  "failedCountAttempts": 0,
  "intent": 0,
  "problemClarificationAttempts": 0,
  "followUpState": 0,
  "lastProblemDescription": null
}
```

**ChatMsg fields:**
| Field | Required | Description |
|-------|----------|-------------|
| role | Yes | "user" or "assistant" |
| content | Yes | The message text |

**State fields (pass from previous response):**
| Field | Type | Description |
|-------|------|-------------|
| extractedService | string | Service type identified so far |
| extractedCity | string | City identified so far |
| extractedCount | int | Number of craftsmen requested |
| intent | int | 0=not asked, 1=want craftsman, 2=want steps |
| followUpState | int | 0=none, 1=waiting answer, 2=waiting detail |
| lastProblemDescription | string | For solution follow-up flow |

#### Response

**Success Response — 200** (the response shape changes based on conversation stage)

**Full result (when search is complete):**
```json
{
  "isComplete": true,
  "message": "تمام! وجدت لك أفضل 3 سباك في القاهرة 🎉\n\nإليك النتائج: ...",
  "showServicesList": false,
  "servicesList": [],
  "showCitiesList": false,
  "citiesList": [],
  "showIntentChoice": false,
  "solutionSteps": [],
  "showSolvedQuestion": false,
  "extractedService": "سباك",
  "extractedCity": "القاهرة",
  "extractedCount": 3,
  "problemClarificationAttempts": 0,
  "followUpState": 0,
  "lastProblemDescription": null,
  "result": {
    "answer": "text answer from AI",
    "retrievedCraftsmen": [
      {
        "id": 1,
        "name": "أحمد محمد",
        "serviceType": "سباك",
        "city": "القاهرة",
        "neighborhood": "مدينة نصر",
        "rating": 4.5,
        "experienceYears": 8,
        "priceRangeMin": 200.00,
        "priceRangeMax": 1000.00,
        "relevantText": "description text",
        "similarityScore": 0.95,
        "isNearby": true,
        "nearbyFromCity": null
      }
    ],
    "latencyMs": 1500.0
  },
  "latencyMs": 1500.0
}
```

**Asking for input (when more info needed):**
```json
{
  "isComplete": false,
  "message": "ما هي المدينة التي تبحث فيها؟",
  "showServicesList": true,
  "servicesList": ["سباك", "كهربائي", "نجار", ...],
  "showCitiesList": true,
  "citiesList": ["القاهرة", "الإسكندرية", ...],
  "showIntentChoice": true,
  "solutionSteps": [],
  ...
}
```

**Solution steps response:**
```json
{
  "isComplete": false,
  "message": "🔧 إليك خطوات عملية يمكنك تجربتها:\n\n✦ الخطوة 1: ...\n✦ الخطوة 2: ...",
  "solutionSteps": ["أغلق المحبس الرئيسي", "افتح الحنفية"],
  "followUpState": 2,
  ...
}
```

#### How It Works (Detailed Flow)
1. Request arrives at `AIController.Chat3()`
2. **Language check:** If message is not Arabic, returns polite Arabic request
3. **Multi-service check:** If message mentions multiple trades, asks user to pick one
4. **Intent analysis:** If user wants steps (self-fix), goes to solution flow
5. **LLM extraction:** Calls `IntentService.ExtractAsync()` to identify service type, city, count
6. **Missing info:** If any info is missing, asks user with relevant question
7. **Complete search:** When service + city + count are known, calls `RAGService.QueryAsync()` to search vector DB
8. Returns matched craftsmen with similarity scores

#### Frontend Integration Notes
- This is a **stateful conversation** — pass all state fields back and forth each time
- Store the response's state fields and send them back with the next request
- When `showServicesList` is true, display the list as clickable buttons
- When `showCitiesList` is true, display cities as clickable buttons
- When `showIntentChoice` is true, show two buttons: "عاوز حرفي" and "عاوز خطوات حل"
- When `isComplete` is true, display the `result.retrievedCraftsmen` as craftsman cards
- `latencyMs` helps you show a loading indicator proportional to expected time

---

## Group 3 — Craftsman Endpoints
Endpoints used by craftsmen to manage their profile, respond to jobs, and view their work.

---

### 1. POST /api/craftsmen/register

*(Documented in Group 2 — Craftsman endpoint as well)*
Same endpoint: used by craftsmen to register their profile after creating a user account.

**Shared endpoint**

---

### 2. PUT /api/craftsmen/{id}

**What it does:** Updates the craftsman's professional profile (city, prices, experience, bio).

**Who uses it:** Craftsman — updating their own profile

**Authorization:** Requires Login

**Where to use it in the frontend:** Craftsman profile settings → Edit professional info.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The craftsman ID |

**Request Body:**
```json
{
  "fullname": "string — full name",
  "profileImageUrl": "string — profile image URL",
  "city": "string — city name",
  "neighborhood": "string or null — neighborhood",
  "priceRangeMin": "decimal or null — minimum price",
  "priceRangeMax": "decimal or null — maximum price",
  "experience": "int — years of experience",
  "bio": "string or null — biography, max 1000 chars"
}
```

#### Response

**Success Response — 200**
```json
{
  "message": "تم تحديث بيانات الملف الشخصي بنجاح."
}
```

---

### 3. POST /api/craftsmen/{id}/upload-image

**What it does:** Uploads a profile image for the craftsman (same as user image upload but linked to craftsman).

**Who uses it:** Craftsman

**Authorization:** Requires Login

**Where to use it in the frontend:** Craftsman profile settings → Change photo.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The craftsman ID |

**Request Body:** `multipart/form-data` with field `file` (same constraints as user upload)

#### Response

**Success Response — 200**
```json
{
  "url": "/profiles/guid-filename.jpg"
}
```

---

### 4. PUT /api/jobs/{id}/accept

**What it does:** Accepts an open job, changing its status from "مفتوح" to "قيد التنفيذ".

**Who uses it:** Craftsman — requires craftsman role

**Authorization:** Requires Role: Craftsman

**Where to use it in the frontend:** Craftsman dashboard → Open jobs → "قبول" button.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The job ID |

**Request Body:** None

#### Response

**Success Response — 200:** JobResponseDto (status changes to "قيد التنفيذ")

**Business Rules:**
- Job must be in status "مفتوح"
- Craftsman must be the one assigned to the job (craftsmanId match)
- Sends notification to customer: "تم قبول طلبك"

---

### 5. PUT /api/jobs/{id}/reject

**What it does:** Rejects an open job.

**Who uses it:** Craftsman — requires craftsman role

**Authorization:** Requires Role: Craftsman

**Where to use it in the frontend:** Craftsman dashboard → Open jobs → "رفض" button.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The job ID |

**Request Body:** None

#### Response

**Success Response — 200:** JobResponseDto (status becomes "مرفوض")

---

### 6. PUT /api/jobs/{id}/complete

**What it does:** Marks a job as complete ("مكتمل"), allowing the customer to submit a review.

**Who uses it:** Craftsman — requires craftsman role

**Authorization:** Requires Role: Craftsman

**Where to use it in the frontend:** Craftsman dashboard → In-progress jobs → "تم الإنجاز" button.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The job ID |

**Request Body:**
```json
{
  "solutionDescription": "string — optional, summary of what was done"
}
```

#### Response

**Success Response — 200:** JobResponseDto (status becomes "مكتمل")

**Business Rules:**
- Job must be in status "قيد التنفيذ"
- Sends notification to customer: "تم إنجاز طلبك. يمكنك الآن تقييم الخدمة"

---

### 7. GET /api/jobs/craftsman/{id}

**What it does:** Returns all jobs for a specific craftsman.

**Who uses it:** Craftsman

**Authorization:** Requires Login

**Where to use it in the frontend:** Craftsman dashboard → "وظائفي" page.

#### Request
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id | int | Yes | The craftsman ID |

**Request Body:** None

#### Response: Same as endpoint 20 in Group 2

---

### 8. PUT /api/auth/craftsman-only

**What it does:** A demo endpoint showing role-based access — returns a craftsman welcome message.

**Who uses it:** Craftsman

**Authorization:** Requires Role: Craftsman

**Where to use it in the frontend:** Not used in production — test endpoint.

#### Request

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "message": "أهلاً بالحرفي 🔧"
}
```

---

*(Endpoints documented in Group 2 that are also used by craftsmen:)*
- GET /api/users/profile/{id} — get own profile
- PUT /api/users/profile/{id} — update own name/phone
- POST /api/users/profile/{id}/upload-image — upload profile picture
- GET /api/conversations — inbox
- GET /api/conversations/{id} — conversation detail
- GET /api/conversations/{id}/messages — chat messages
- PUT /api/conversations/{id}/read — mark as read
- POST /api/conversations — create conversation
- GET /api/notifications — notifications
- GET /api/notifications/unread-count — badge count
- PUT /api/notifications/{id}/read — mark notification read
- PUT /api/notifications/read-all — mark all read

All are **shared endpoints** between Customer and Craftsman.

---

## Group 4 — AI Endpoints

---

### 1. POST /api/AI/chat3

*(Full documentation in Group 2, endpoint 31)*

**AI-powered conversational interface** using Groq (LLaMA 3.3 70B), Voyage AI embeddings, and Qdrant vector database. Main AI endpoint.

---

### 2. POST /api/AI/ingest/craftsmen

**What it does:** Ingests all approved craftsmen into the Qdrant vector database for AI-powered search. Creates embeddings using Voyage AI.

**Who uses it:** Admin/AI — internal management tool

**Authorization:** Requires Login

**Where to use it in the frontend:** Admin dashboard → AI Settings → "مزامنة البيانات" button.

#### Request

**Query Parameters:**
| Parameter | Type | Required | Default | What it does |
|-----------|------|----------|---------|--------------|
| fromId | int | No | 0 | Start ingestion from this craftsman ID |

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "totalCraftsmen": 45,
  "totalChunksIndexed": 135,
  "message": "تم فهرسة 45 حرفي بنجاح"
}
```

---

### 3. POST /api/AI/ingest/jobs

**What it does:** Ingests completed job solutions into the vector database for RAG-based solution suggestions.

**Who uses it:** Admin/AI — internal management

**Authorization:** Requires Login

#### Request

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "indexed": 80
}
```

---

### 4. GET /api/AI/vectors/count

**What it does:** Returns the total number of vectors stored in the Qdrant database.

**Who uses it:** Admin/AI — monitoring

**Authorization:** Requires Login

#### Request

**Request Body:** None

#### Response

**Success Response — 200**
```json
{
  "totalVectors": 215
}
```

---

## SignalR Hubs

### ChatHub

**Hub URL:** `/hubs/chat`

**Authentication:** JWT token passed as query parameter: `?access_token={token}`

**How to connect (JavaScript):**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5108/hubs/chat", {
    accessTokenFactory: () => localStorage.getItem("accessToken")
  })
  .build();
```

#### Server Methods (called by client)

| Method | Parameters | What it does | When to call |
|--------|-----------|-------------|--------------|
| JoinConversation | conversationId: int | Join the SignalR group for a conversation | When user opens chat window |
| LeaveConversation | conversationId: int | Leave the SignalR group | When user closes chat window |
| SendMessage | SendMessageDto object | Send a message in real-time | On send button click |
| Typing | conversationId: int | Tell the other user you're typing | On keystroke in input |
| MarkAsRead | conversationId: int | Mark messages as read | When user views messages |

#### Client Methods (sent by server, handled in frontend)

| Event | Data | What triggers it | Which component listens |
|-------|------|-----------------|----------------------|
| ReceiveMessage | MessageDto | Another user sent a message | Chat window — append to message list |
| UserTyping | userId: int | Other user is typing | Chat window — show "جارٍ الكتابة..." indicator |
| MessagesRead | conversationId: int | Other user read messages | Chat window — update read receipts |
| UserOnline | userId: int | User connected | Inbox — show green dot |
| UserOffline | userId: int | User disconnected | Inbox — remove green dot |

#### SendMessageDto (for calling SendMessage):
```json
{
  "conversationId": "int — required",
  "content": "string — required, max 2000 chars",
  "messageType": "string — optional, default 'text', allowed: text|image|system"
}
```

#### MessageDto (received from ReceiveMessage):
```json
{
  "id": 1,
  "conversationId": 1,
  "senderId": 3,
  "senderName": "محمود حسن",
  "senderAvatar": null,
  "content": "السلام عليكم",
  "messageType": "text",
  "isRead": false,
  "sentAt": "2026-06-08T10:00:00Z"
}
```

#### Connection Lifecycle
1. `OnConnectedAsync` — saves user connection to DB, broadcasts "UserOnline"
2. `OnDisconnectedAsync` — marks connection as disconnected, broadcasts "UserOffline"
3. Call `JoinConversation` before sending/receiving messages in a specific conversation

#### Frontend Integration Notes
- Connect to the hub immediately after login
- Keep the connection alive as long as the user is on the app
- On `ReceiveMessage`, update the inbox `lastMessage` field for the conversation
- On `ReceiveMessage`, if the conversation is not open, increment the conversation's unread count
- The `MessageType` field: "text" for normal, "image" for image messages, "system" for system messages

---

### NotificationHub

**Hub URL:** `/hubs/notifications`

**Authentication:** JWT token passed as query parameter: `?access_token={token}`

**How to connect (JavaScript):**
```javascript
const notifConnection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5108/hubs/notifications", {
    accessTokenFactory: () => localStorage.getItem("accessToken")
  })
  .build();
```

#### Server Methods

None — this hub auto-joins the user to their notification group on connect.

#### Client Methods

| Event | Data | What triggers it | Which component listens |
|-------|------|-----------------|----------------------|
| (None — server uses Groups to send) | — | — | — |

This hub is used by the server to push notifications to specific users. The server calls `Clients.Group("user_{userId}").SendAsync("ReceiveNotification", notificationDto)`.

#### How Notifications Work
1. Hub `OnConnectedAsync` automatically adds user to group `user_{userId}`
2. Server-side services (e.g., JobService, MessageService) call `NotificationService` methods
3. NotificationService saves notification to DB AND sends it via SignalR
4. Frontend receives real-time notification without polling

#### Frontend Integration Notes
- Listen for the "ReceiveNotification" event name (as defined by your server push implementation)
- Update the notification bell badge count when a new notification arrives
- The same notification is saved to DB and can be fetched via REST endpoints (GET /api/notifications)

---

## Authentication Flow

### Step-by-Step Guide

#### 1. Register
```
POST /api/auth/register
Body: { "name", "email", "password", "confirmPassword", "role": "customer|craftsman" }
Response: 201 — AuthResponseDto (includes tokens)
```

After registration, the user receives tokens immediately AND a verification email is sent.

#### 2. Verify Email
```
POST /api/auth/verify-email
Body: { "email", "code": "6-digit code from email" }
Response: 200 — { "success": true, "message": "تم تفعيل البريد الإلكتروني بنجاح." }
```

**Important:** User cannot log in until email is verified. The login endpoint checks `IsVerified`.

#### 3. Login
```
POST /api/auth/login
Body: { "email", "password" }
Response: 200 — AuthResponseDto
```

#### 4. Store Tokens
The `AuthResponseDto` contains:
- `accessToken` — JWT string, expires in 60 minutes
- `refreshToken` — long-lived token (30 days), for getting new access tokens
- `expiresAt` — when the access token expires
- `user` — user info (id, name, email, role, phone, profileImageUrl)

**Store both tokens** in `localStorage` or secure storage.

#### 5. Use the Access Token
Include in all API requests:
```
Authorization: Bearer {accessToken}
```

#### 6. Refresh When Expired
When you get a **401 Unauthorized** response, call:
```
POST /api/auth/refresh
Body: { "refreshToken": "the stored refresh token" }
Response: 200 — new AuthResponseDto (new access + refresh tokens)
```
Replace stored tokens with the new ones.

#### 7. Logout
```
POST /api/auth/logout
Body: { "refreshToken": "the stored refresh token" }
Response: 200 — { "message": "تم تسجيل الخروج بنجاح" }
```
Clear both tokens from storage on the client side.

---

## Common Response Patterns

### Success Response Shape
Successful responses return:
- **201** for creation (POST to collection)
- **200** for everything else
- **204** for operations returning no content (PUT mark-as-read)

### Error Response Shape

**Model Validation Errors (400):**
```json
{
  "fieldName": [
    "ErrorMessage1",
    "ErrorMessage2"
  ]
}
```

**Service Layer Errors (via GlobalExceptionMiddleware):**
```json
{
  "status": 400,
  "message": "Arabic error message from service/exception",
  "timestamp": "2026-06-08T10:00:00Z"
}
```

**HTTP Status Codes Used:**
| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 204 | No Content (success) |
| 400 | Bad Request — validation error or business rule violation |
| 401 | Unauthorized — missing or invalid JWT |
| 403 | Forbidden — valid JWT but wrong role |
| 404 | Not Found — resource doesn't exist |
| 500 | Internal Server Error — "خطأ في الخادم، حاول مرة أخرى." |

### Paged Response Shape
```json
{
  "items": [...],
  "totalCount": 50,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

### Admin Action Response
```json
{
  "success": true,
  "message": "Arabic success/error message",
  "data": null
}
```

---

## Appendix — Data Models

### User (AspNetUsers table via Identity)
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK, auto-increment |
| UserName | string | Same as email |
| Name | string | Required, max 100 |
| Email | string | Unique, required |
| PasswordHash | string | Handled by Identity |
| Phone | string? | Max 20 |
| Role | string | "admin" \| "craftsman" \| "customer" |
| IsActive | bool | Default true |
| IsDeleted | bool | Soft delete flag |
| IsVerified | bool | Email verified |
| ProfileImageUrl | string? | Max 500 |
| DeletedAt | datetime? | |
| DeletedByAdminId | int? | |
| DeletionReason | string? | Max 500 |
| CreatedAt | datetime | Default GETUTCDATE() |
| EmailConfirmed | bool | Identity field |
| PhoneNumberConfirmed | bool | Identity field |

### Craftsman
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK, auto-increment |
| UserId | int | FK → Users, unique (1:1) |
| ServiceType | string | Required, max 50 |
| City | string | Required, max 100 |
| Neighborhood | string? | Max 100 |
| PriceRangeMin | decimal(10,2)? | |
| PriceRangeMax | decimal(10,2)? | |
| Experience | int | Default 0 |
| IsApproved | bool | Default false |
| IsAvailable | bool | Default true |
| IsDeleted | bool | Soft delete |
| DeletedAt | datetime? | |
| DeletedByAdminId | int? | |
| DeletionReason | string? | Max 500 |
| RejectionReason | string? | Max 500 |
| Rating | decimal(3,2) | Computed from reviews |
| Bio | string? | Max 1000 |
| NationalIdUrl | string? | Max 500 |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

### Job
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| CustomerId | int | FK → Users |
| CraftsmanId | int? | FK → Craftsmen |
| Status | string | "مفتوح" \| "قيد التنفيذ" \| "مكتمل" \| "مرفوض" \| "ملغى" |
| ServiceType | string | Required, max 50 |
| Description | string | Required, max 2000 |
| Address | string | Required, max 500 |
| PreferredDate | datetime? | |
| ProblemImageUrl | string? | Max 500 |
| ProblemDescription | string? | Max 2000 |
| SolutionDescription | string? | Max 2000 |
| IsDisputed | bool | Default false |
| DisputeRaisedAt | datetime? | |
| DisputeResolvedAt | datetime? | |
| DisputeResolution | string? | Max 500 |
| CreatedAt | datetime | |
| CompletedAt | datetime? | |
| UpdatedAt | datetime | |

### Review
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| JobId | int | FK → Jobs, unique (one review per job) |
| CustomerId | int | FK → Users |
| CraftsmanId | int | FK → Craftsmen |
| Stars | int | 1-5 (validated in service) |
| Comment | string? | Max 1000 |
| IsDeleted | bool | |
| DeletedAt | datetime? | |
| DeletedByAdminId | int? | |
| DeletionReason | string? | Max 500 |
| CreatedAt | datetime | |

### Conversation
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| JobId | int | FK → Jobs, unique (1:1) |
| CustomerId | int | FK → Users |
| CraftsmanId | int | FK → Craftsmen |
| LastMessageAt | datetime? | Updated on each message |
| CreatedAt | datetime | |
| UpdatedAt | datetime | |

### Message
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| ConversationId | int | FK → Conversations |
| SenderId | int | FK → Users |
| Content | string | Required, max 2000 |
| MessageType | string | "text" \| "image" \| "system" |
| IsRead | bool | Default false |
| SentAt | datetime | |

### Notification
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| UserId | int | FK → Users |
| Title | string | Required, max 200 |
| Body | string | Required, max 1000 |
| IsRead | bool | Default false |
| Type | string? | "new_message" \| "job_accepted" \| "job_completed" \| "approved" |
| RelatedJobId | int? | FK → Jobs |
| CreatedAt | datetime | |

### RefreshToken
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| UserId | int | FK → Users |
| Token | string | Required, max 500, unique |
| ExpiresAt | datetime | |
| IsRevoked | bool | Default false |
| CreatedAt | datetime | |
| IsExpired | (not mapped) | Computed: ExpiresAt < UtcNow |
| IsActive | (not mapped) | Computed: !IsRevoked && !IsExpired |

### MediaFile
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| FileName | string | Required, max 255 |
| FileUrl | string | Required, max 500 (Cloudinary URL) |
| FileType | string | e.g., "image/jpeg" |
| EntityType | string | "user" \| "craftsman" \| "job" |
| EntityId | int | Polymorphic reference |
| UploadedBy | int | FK → Users |
| CreatedAt | datetime | |

### AIChatMessage
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| UserId | int | FK → Users |
| SessionId | string | GUID grouping messages |
| Role | string | "user" \| "assistant" |
| Content | string | Required, max 4000 |
| ToolUsed | string? | "CraftsmanSearchTool" \| "SelfFixGuideTool" \| "PricingEstimatorTool" |
| TokensUsed | int? | |
| CreatedAt | datetime | |

### RAGDocument
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| JobId | int | FK → Jobs |
| ChromaDocumentId | string | Required, max 200 |
| ChunkType | string | "problem" \| "solution" |
| EmbeddingModel | string | Default "text-embedding-3-small" |
| CreatedAt | datetime | |

### JobFeedback
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| UserId | int | FK → Users |
| RAGDocumentId | int? | FK → RAGDocuments |
| FeedbackType | string | "ساعدني" \| "محتاج حرفي" |
| CreatedAt | datetime | |

### UserConnection (SignalR)
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| UserId | int | FK → Users |
| ConnectionId | string | SignalR connection ID, unique |
| IsConnected | bool | Default true |
| ConnectedAt | datetime | |
| DisconnectedAt | datetime? | |
| CreatedAt | datetime | |

### EmailVerification
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| UserId | int | FK → Users |
| Code | string | 6-digit code |
| IdentityToken | string? | Identity email confirmation token |
| ExpiresAt | datetime | 10 minutes from creation |
| IsUsed | bool | |
| CreatedAt | datetime | |

### PhoneVerification
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| UserId | int | FK → Users |
| PhoneNumber | string | |
| Code | string | 6-digit code |
| IdentityToken | string? | Identity phone change token |
| ExpiresAt | datetime | 10 minutes |
| IsUsed | bool | |
| CreatedAt | datetime | |

### AdminAuditLog
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| AdminId | int | FK → Users |
| Action | string | e.g., "approve_craftsman" |
| TargetType | string | e.g., "Craftsman" |
| TargetId | int | |
| Notes | string? | Max 1000 |
| IpAddress | string? | Max 50 |
| CreatedAt | datetime | |

### Report
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| ReportedByUserId | int | |
| TargetType | string | e.g., "craftsman", "user" |
| TargetId | int | |
| Reason | string | Required, max 500 |
| Status | string | "pending" \| "resolved" |
| ResolvedByAdminId | int? | |
| ResolutionNotes | string? | Max 500 |
| CreatedAt | datetime | |
| ResolvedAt | datetime? | |

### ServiceType
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| NameAr | string | Required, max 100, unique |
| NameEn | string | Required, max 100, unique |
| Icon | string? | Max 200 |
| IsActive | bool | Default true |

### City
| Field | Type | Notes |
|-------|------|-------|
| Id | int | PK |
| NameAr | string | Required, max 100, unique |
| NameEn | string | Required, max 100, unique |
| Governorate | string? | Max 100 |
| IsActive | bool | Default true |

### FeatureFlag
| Field | Type | Notes |
|-------|------|-------|
| Key | string | PK, max 100 |
| IsEnabled | bool | |
| UpdatedAt | datetime | |

---

## Job Status Constants
| Arabic | English | Meaning |
|--------|---------|---------|
| مفتوح | Open | Job created, awaiting craftsman response |
| قيد التنفيذ | In Progress | Craftsman accepted the job |
| مكتمل | Done | Job completed, customer can review |
| مرفوض | Rejected | Craftsman rejected the job |
| ملغى | Cancelled | Job was cancelled |
