# 🤖 توثيق نظام الذكاء الاصطناعي (AI) في منصة "حرفي" (Harfi)


> **الغرض:** توثيق كامل لسيناريوهات AI، المكونات، التدفقات، وأدوار RAG و LLM.  
> **ملاحظة:** هذا الملف يجمع كافة التفاصيل من الشرح السابق في وثيقة واحدة شاملة.

---




---

## 📋 قائمة endpoints

1. [POST `/api/AI/chat3` – المحادثة الأساسية](#1-post-apiaichat3)
2. [POST `/api/AI/analyze-media` – تحليل الوسائط (صور/صوت)](#2-post-apiaianalyze-media)
3. [GET `/api/AI/welcome` – رسالة ترحيب](#3-get-apiaiwelcome)
4. [GET `/api/AI/sessions/{userId}` – جلب ملخص جلسات المستخدم](#4-get-apiaisessionsuserid)
5. [GET `/api/AI/sessions/{userId}/{sessionId}` – جلب رسائل جلسة محددة](#5-get-apiaisessionsuseridsessionid)
6. [POST `/api/AI/sessions/message` – حفظ رسالة جديدة](#6-post-apiaisessionsmessage)
7. [DELETE `/api/AI/sessions/{userId}/{sessionId}` – حذف جلسة كاملة](#7-delete-apiaisessionsuseridsessionid)
8. [POST `/api/AI/ingest/craftsmen` – إدخال الحرفيين في Qdrant](#8-post-apiaiingestcraftsmen)
9. [POST `/api/AI/ingest/jobs` – إدخال حلول الوظائف المكتملة في Qdrant](#9-post-apiaiingestjobs)
10. [POST `/api/AI/ingest/job/{jobId}` – إدخال حل وظيفة واحدة في Qdrant](#10-post-apiaiingestjobjobid)
11. [GET `/api/AI/vectors/count` – عدد المتجهات في Qdrant](#11-get-apiaivectorscount)
12. [POST `/api/AI/craftsman/submit-solution` – إرسال حل من حرفي (مع تحسين LLM)](#12-post-apiaicraftsman-submit-solution)




## 📋 المحتويات

1. [نظرة عامة](#نظرة-عامة)
2. [المكونات الرئيسية والتقنيات](#المكونات-الرئيسية-والتقنيات)
3. [سيناريوهات AI الأساسية](#سيناريوهات-ai-الأساسية)
   - [1. استخراج النية (Intent Extraction)](#1-استخراج-النية-intent-extraction)
   - [2. خطوات الحل (Solution Steps)](#2-خطوات-الحل-solution-steps)
   - [3. البحث عن حرفي (Craftsman Discovery)](#3-البحث-عن-حرفي-craftsman-discovery)
   - [4. تحليل الوسائط (Analyze Media)](#4-تحليل-الوسائط-analyze-media)
   - [5. جلسات المستخدم والتاريخ (Sessions & History)](#5-جلسات-المستخدم-والتاريخ-sessions--history)
   - [6. إدخال البيانات (Ingestion)](#6-إدخال-البيانات-ingestion)
   - [7. حلقة التغذية الراجعة (Feedback Loop)](#7-حلقة-التغذية-الراجعة-feedback-loop)
4. [تفاصيل المكونات التقنية](#تفاصيل-المكونات-التقنية)
   - [AIController](#aicontroller)
   - [IntentService](#intentservice)
   - [SolutionService](#solutionservice)
   - [RAGService](#ragservice)
   - [EmbeddingService](#embeddingservice)
   - [VectorDbService](#vectordbservice)
   - [GroqRotatingClient](#groqrotatingclient)
5. [قواعد البيانات والتخزين](#قواعد-البيانات-والتخزين)
   - [SQL Server](#sql-server)
   - [Qdrant (Vector DB)](#qdrant-vector-db)
   - [الملفات (Images/Audio)](#الملفات-imagesaudio)
6. [API Endpoints الخاصة بالـ AI](#api-endpoints-الخاصة-بالـ-ai)
7. [تدفق البيانات الكامل (Diagram)](#تدفق-البيانات-الكامل-diagram)
8. [إعدادات البيئة والمتغيرات](#إعدادات-البيئة-والمتغيرات)
9. [ملخص أدوار RAG و LLM](#ملخص-أدوار-rag-و-llm)

---

## نظرة عامة

نظام الذكاء الاصطناعي في "حرفي" هو **مساعد ذكي متخصص في الصيانة المنزلية وإيجاد الحرفيين**. يعتمد على:

- **RAG (Retrieval-Augmented Generation):** استرجاع حلول مشابهة من قاعدة معرفة (Qdrant) باستخدام embeddings (VoyageAI).
- **LLM (Large Language Model):** نموذج Groq (Llama 3.3 70B) لفهم النية، التحقق من الملاءمة، صياغة الإجابات بالعامية المصرية، وتوليد خطوات جديدة.
- **حلقة تغذية راجعة (Feedback Loop):** حفظ الحلول المفيدة تلقائياً لتحسين الأداء بمرور الوقت.

**السيناريوهات الأساسية:**
- 🔍 استخراج نية المستخدم (خدمة، مدينة، عدد)
- 🛠️ تقديم خطوات إصلاح ذاتي (DIY)
- 👷 البحث عن حرفي مناسب (RAG + LLM)
- 🖼️ تحليل الصور والتسجيلات الصوتية (عبر n8n)
- 💾 حفظ واستئناف جلسات المحادثة
- 📚 إدخال البيانات في Qdrant (Indexing)
- ⭐ التعلم من تقييمات المستخدمين

---

## المكونات الرئيسية والتقنيات

| المكون | التقنية | الدور |
|--------|---------|-------|
| `AIController` | ASP.NET Core | مدخل API لجميع طلبات AI |
| `IntentService` | Groq (LLM) | استخراج service_type, city, count من المحادثة |
| `SolutionService` | Qdrant + Groq | جلب أو توليد خطوات حل المشكلة |
| `RAGService` | Qdrant + Groq | البحث عن حرفي مناسب |
| `EmbeddingService` | VoyageAI | تحويل النصوص إلى متجهات (1024-dim) |
| `VectorDbService` | Qdrant | تخزين واسترجاع المتجهات |
| `GroqRotatingClient` | Groq | إدارة مفاتيح API متعددة والتبديل عند rate limit |
| `ChunkingService` | داخلي | تحويل بيانات الحرفي إلى نص مناسب للتضمين |

**التكامل الخارجي:**
- **VoyageAI:** `https://api.voyageai.com/v1/embeddings` (نموذج `voyage-3`)
- **Groq:** `https://api.groq.com/openai/v1/chat/completions` (نموذج `llama-3.3-70b-versatile`)
- **Qdrant:** `http://localhost:6400` (أو خدمة سحابية)
- **n8n (اختياري):** تحليل الصوت والصور (webhook خارجي)

---

## سيناريوهات AI الأساسية

### 1. استخراج النية (Intent Extraction)

**المسؤول:** `IntentService` + Groq (طلب LLM #1)

**المدخلات:** قائمة رسائل المحادثة (`List<ChatMsg>`)

**المخرجات:**
- `service_type` (من قائمة 12 تخصص)
- `city` (من قائمة 28 محافظة)
- `count` (عدد الحرفيين 1-10)
- `missing` (نوع البيانات الناقصة: none, service, city, count, service_city, ...)
- `question_to_ask` (سؤال يوجه للمستخدم)
- `show_services_list` / `show_cities_list` (عرض قوائم اختيار)

**مثال لرد LLM:**
```json
{
  "service_type": "سباك",
  "city": "القاهرة",
  "count": 3,
  "missing": "none",
  "question_to_ask": null,
  "show_services_list": false,
  "show_cities_list": false
}



-------------------------------------------------------------------


# Workflow: خطوات الحل (Solution Steps)

## 1. المستخدم يكتب وصف المشكلة
   ↓
## 2. هل الوصف كافٍ وواضح؟
   │
   ├─ لا → النظام يطلب من المستخدم توضيحاً أكثر (مثال: "اشرح المشكلة بالتفصيل")
   │        ↓
   │        (يعود إلى الخطوة 1)
   │
   └─ نعم → الانتقال إلى البحث (RAG)

## 3. RAG: البحث عن حلول مشابهة
   - يبحث في Qdrant (collection: job_solutions) عن أفضل 5 حلول مشابهة لوصف المشكلة.
   - لكل حل يتم استرجاع: خطوات الحل + وصف المشكلة المرتبطة به.
   ↓
## 4. LLM: تقييم ملاءمة كل حل
   - يمرر الحل ووصف المشكلة الحالية إلى LLM (Groq).
   - يسأل LLM: "هل هذا الحل مناسب لمشكلة المستخدم؟"
   │
   ├─ غير مناسب → ينتقل إلى الحل التالي في القائمة (إن وجد) ويعيد التقييم.----- لو ملقاش فيهم  ال llm يكريت ال خطوات 
   │
   └─ مناسب → ينتقل إلى الخطوة 5.

## 5. LLM: صياغة الخطوات النهائية
   - يأخذ الحل المناسب (خطواته الخام).
   - يعيد صياغة الخطوات بالعامية المصرية، مخصصة للمشكلة الحالية.
   - يخرج نصاً بالخطوات المرقمة (1. كذا، 2. كذا...).
   ↓
## 6. عرض الخطوات على المستخدم + سؤال التأكيد
   - يعرض النظام الخطوات.
   - يسأل: "هل تمكّنت الخطوات من حل المشكلة؟"
   ↓
## 7. تحليل إجابة المستخدم
   │
   ├─ المستخدم يقول: لا (لم تتحل)
   │     ↓
   │     - يطلب النظام تفاصيل أكثر عن الجزء الذي لم يُحل.
   │     - يعود إلى الخطوة 3 (RAG مع التفاصيل الجديدة) ويستمر.
   │
   └─ المستخدم يقول: نعم (اتحلت)
         ↓
         - ينتقل إلى الخطوة 8.

## 8. سؤال تقييم الفائدة
   - يسأل النظام: "هل كانت الخطوات مفيدة؟"
   ↓
## 9. تحليل التقييم
   │
   ├─ يقول: لا (غير مفيدة) → تنتهي العملية (لا حفظ).
   │
   └─ يقول: نعم (مفيدة)
         ↓
         - LLM: يظبط صيغة الحل النهائية (JSON يحتوي: description, problem_description, solution_description).
         - يحفظ في SQL: Job جديد (حالة "AI") + RAGDocument + JobFeedback.
         - يحفظ في Qdrant: embedding للحل الجديد في collection job_solutions.
         - ينتهي (شكر المستخدم).

## ملاحظات:
- RAG يستخدم VoyageAI لتحويل النصوص إلى embeddings.
- LLM هو Groq (Llama 3.3 70B) مع إدارة عدة مفاتيح API.
- الحلول المحفوظة تصبح متاحة للمستخدمين التاليين مباشرة.


---------------------------------------



# Workflow: البحث عن حرفي (Craftsman Discovery)

## 1. المستخدم يكتب طلب البحث عن حرفي
   - مثال: "عاوز سباك في مدينة نصر" أو "محتاج كهربائي في القاهرة"
   - قد يكون الطلب ضمن محادثة أطول (استخرجت منه service_type, city, count مسبقاً)
   ↓
## 2. IntentService (LLM) يستخرج البيانات المطلوبة
   - service_type (من قائمة 12 تخصصاً)
   - city (من قائمة 28 محافظة)
   - count (عدد الحرفيين المطلوب، 1-10)
   - district (الشارع/المنطقة - اختياري، يُستخرج من رسالة المستخدم أو يُطلب لاحقاً)
   ↓
## 3. التحقق من اكتمال البيانات
   │
   ├─ البيانات ناقصة (service, city, count) → يسأل المستخدم ويكمل (يعيد الطلب)
   │
   └─ البيانات كاملة → ينتقل إلى RAGService.QueryAsync()



اول مبيلاقي كل البيانات اكتملت 
بيروح للrag التخصص والمحافظه 



## 4. RAGService: تحويل سؤال المستخدم إلى متجه (embedding)
   - يستخدم EmbeddingService مع نموذج VoyageAI (input_type = "query")
   - يُنتج متجه بطول 1024 رقم (float32)
   ↓
## 5. بناء قائمة المدن للبحث (BuildCityQueue)
   - يبدأ بالمدينة المطلوبة (مثال: "القاهرة")
   - ثم يُضيف المحافظات المجاورة (حسب NearbyMap المعرف مسبقاً في الكود)
   - مثال: القاهرة → [القاهرة, الجيزة, الشرقية, المنوفية, ...]
   ↓
## 6. البحث في Qdrant (حلقة لكل مدينة في القائمة)
   - لكل مدينة:
     - استدعاء VectorDbService.SearchAsync(embedding, topK, serviceType, city)
     - Qdrant يبحث في collection `harfi_craftsmen` مع فلتر service_type و city
     - يُرجع عدداً أكبر من المطلوب (مثلاً count × 5) لضمان حصولنا على مرشحين كافيين
     - يُضاف المرشحون إلى قائمة مؤقتة (مع تجنب تكرار نفس الحرفي عبر المدن)
   - تتوقف الحلقة عندما نحصل على عدد كافٍ من الحرفيين (≥ count) أو تنتهي المدن
   ↓
## 7. التحقق من التخصص بواسطة LLM (VerifyServiceTypeAsync)
   - لكل حرفي مرشح (يحتوي على ServiceType الخاص به من قاعدة البيانات)
   - يُرسل LLM (Groq) طلباً للتحقق مما إذا كان تخصص الحرفي هو المطلوب بالضبط
   - (السبب: الـ semantic search قد يرجع حرفياً تخصصه "نجار" عندما أبحث عن "حداد" بسبب تشابه النصوص)
   - LLM يُرجع JSON: {"verified_ids": [id1, id2, ...]} أو قائمة فارغة
   - يتم الاحتفاظ فقط بالحرفيين الذين اجتازوا الفحص
   ↓
## 8. إعادة الترتيب حسب المنطقة/الشارع (ReRankByDistrictAsync) – إذا وُجد district
   - يتم إرسال قائمة الحرفيين (مع عناوينهم الكاملة المخزنة في SQL) إلى LLM
   - يُطلب من LLM ترتيبهم من الأقرب إلى الأبعد بناءً على تشابه district مع عنوان كل حرفي
   - LLM يُرجع JSON: {"ranked_ids": [id3, id1, id2, ...]}
   - يتم إعادة ترتيب القائمة حسب هذا الترتيب
   ↓

## 10. صياغة الإجابة النهائية بواسطة LLM (CallGroqAsync)
    - يُرسل LLM السياق الذي تم بناؤه وسؤال المستخدم الأصلي
    - يُطلب من LLM كتابة إجابة بالعربية العامية المصرية تعرض أفضل الحرفيين
    - يجب أن تشير الإجابة إلى الحرفيين من المحافظة المطلوبة أولاً، ثم المجاورة (إذا وُجدت)
    ↓
## 11. إعادة الرد إلى المستخدم
    - QueryResponse يحتوي على:
      - Answer: النص النهائي المنسق
      - RetrievedCraftsmen: قائمة مفصلة بالحرفيين (للعرض على واجهة المستخدم)
      - LatencyMs: وقت الاستجابة (للقياس)
    ↓
## 12. (اختياري) إذا لم يجد Qdrant أي حرفي مناسب
    - يتم استدعاء SqlFallbackAsync:
      - بحث في SQL مباشرة (بدون RAG) باستخدام service_type فقط
      - ترتيب تنازلي حسب التقييم (Rating)
      - LLM يصيغ إجابة من هذه القائمة
      - تُعاد النتائج للمستخدم (مع تنبيه ضمني بأنها نتائج عامة، غير محسّنة بالبحث الدلالي)

## ملاحظات إضافية:
- RAG يستخدم VoyageAI لتحويل النص إلى embedding للبحث الدلالي.
- Qdrant يعيد النتائج مع درجة تشابه (score) تُستخدم في الترجيح.
- LLM (Groq) يُستخدم في ثلاث نقاط رئيسية:
  1. التحقق من التخصص (VerifyServiceTypeAsync)
  2. إعادة الترتيب حسب القرب الجغرافي (ReRankByDistrictAsync)
  3. صياغة الإجابة النهائية (CallGroqAsync)
- في حالة فشل Qdrant أو عدم وجود نتائج، يتم اللجوء إلى SQL كخطة احتياطية.
- جميع طلبات LLM تستخدم GroqRotatingClient لإدارة المفاتيح وتجنب rate limits.

## رسم مبسط للتدفق:

طلب المستخدم
      │
      ▼
استخراج service/city/count/district بواسطة IntentService (LLM)
      │
      ▼
البيانات كاملة؟──لا──▶ اسأل المستخدم وأعد الطلب
      │
     نعم
      ▼
RAG: تحويل السؤال إلى embedding (VoyageAI)
      │
      ▼
RAG: البحث في Qdrant (harfi_craftsmen) عن الحرفيين في المحافظه المطلوبة والتخصص ة
      │
      ▼
LLM: تأكيد تطابق التخصص (VerifyServiceTypeAsync)
      │
      ▼
إذا وُجد district → LLM: إعادة الترتيب حسب القرب الجغرافي (ReRankByDistrictAsync)
      │
      ▼
بناء السياق (BuildContext)
      │
      ▼
LLM: صياغة الإجابة النهائية (CallGroqAsync)
      │
      ▼
عرض النتائج للمستخدم



---

## 1. POST `/api/AI/chat3`

**الوصف:** نقطة الدخول الرئيسية للمحادثة مع المساعد الذكي. تُستخدم لاستخراج النية، تقديم خطوات حل المشكلة، البحث عن حرفي، وإدارة حالات المتابعة.

### Request Body (Chat3Request)

| الحقل | النوع | إجباري | الوصف |
|-------|-------|--------|-------|
| `messages` | `List<ChatMsg>` | ✅ | تاريخ المحادثة (`role`: `user`/`assistant`, `content`) |
| `extractedService` | `string` | ❌ | التخصص المستخرج مسبقاً (مثل "سباك") |
| `extractedCity` | `string` | ❌ | المدينة المستخرجة مسبقاً |
| `extractedDistrict` | `string` | ❌ | الحي/الشارع المستخرج مسبقاً |
| `extractedCount` | `int` | ❌ | عدد الحرفيين المطلوب (1-10) |
| `failedServiceAttempts` | `int` | ❌ | عدد محاولات فهم التخصص الفاشلة |
| `failedCityAttempts` | `int` | ❌ | عدد محاولات فهم المدينة الفاشلة |
| `failedCountAttempts` | `int` | ❌ | عدد محاولات فهم العدد الفاشلة |
| `intent` | `int` | ❌ | 0=لم يُسأل، 1=يريد حرفي، 2=يريد خطوات |
| `problemClarificationAttempts` | `int` | ❌ | عدد مرات طلب توضيح المشكلة |
| `followUpState` | `int` | ❌ | 0=None, 1=انتظار "هل اتحلت؟", 2=انتظار تفاصيل, 3=انتظار تقييم |
| `lastProblemDescription` | `string` | ❌ | آخر وصف للمشكلة |
| `solutionSteps` | `List<string>` | ❌ | الخطوات السابقة |
| `userId` | `int` | ❌ | معرف المستخدم |
| `sessionId` | `string` | ❌ | معرف جلسة المحادثة (GUID) |

**مثال طلب:**
json
{
  "messages": [{ "role": "user", "content": "عاوز خطوات أصلح الحنفية" }],
  "extractedService": "سباك",
  "extractedCity": "القاهرة",
  "intent": 2,
  "userId": 5,
  "sessionId": "484684e9-5a0a-4887-8470-6c49e93433b5"
}

2. POST /api/AI/analyze-media   صور ريكورد نص
3. GET /api/AI/welcome
الوصف: يعيد رسالة ترحيبية قصيرة.

Response
json
{
  "message": "أهلاً بك! 👋\nأنا مساعدك الذكي للعثور على أفضل الحرفيين في مصر.\nأخبرني بمشكلتك وسأجد لك الحرفي المناسب فوراً! 🔧"
}



4. GET /api/AI/sessions/{userId}
الوصف: جلب ملخص جميع جلسات المحادثة لمستخدم معين.

[
  {
    "sessionId": "484684e9-5a0a-4887-8470-6c49e93433b5",
    "title": "حنفية بتقطر",
    "lastMessage": "شكراً جزيلاً! الخطوات مفيدة جداً",
    "lastActivity": "2026-06-09T10:30:00Z",
    "messageCount": 12
  },
  {
    "sessionId": "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
    "title": "كهرباء انقطعت",
    "lastMessage": "لسه المشكلة موجودة",
    "lastActivity": "2026-06-08T15:20:00Z",
    "messageCount": 8
  }
]

5. GET /api/AI/sessions/{userId}/{sessionId}
الوصف: جلب جميع رسائل جلسة محددة مع فك ترميز الصور والصوت.
{
  "sessionId": "484684e9-5a0a-4887-8470-6c49e93433b5",
  "title": "حنفية بتقطر",
  "messages": [
    {
      "id": 1,
      "role": "user",
      "content": "الحنفية بتقطر",
      "images": [],
      "audio": null,
      "createdAt": "2026-06-09T10:00:00Z"
    },
    {
      "id": 2,
      "role": "assistant",
      "content": "🔧 إليك خطوات...",
      "images": ["/AiChat/images/abc123.jpg"],
      "audio": "/AiChat/audio/recording.wav",
      "createdAt": "2026-06-09T10:00:05Z"
    }
  ]
}



6. POST /api/AI/sessions/message
الوصف: حفظ رسالة جديدة (قد تحتوي على صور أو صوت) في جلسة محددة. الطلب من نوع multipart/form-data.

Request (JSON representation للتوضيح فقط، لا يرسل كـ JSON):

json
{
  "UserId": 5,
  "SessionId": "484684e9-5a0a-4887-8470-6c49e93433b5",
  "Role": "user",
  "Content": "المشكلة لسه موجودة",
  "ToolUsed": null,
  "Images": [/* file objects */],
  "Audio": /* file object or null */
}
Response JSON:

json
{
  "id": 123,
  "images": ["/AiChat/images/xyz789.jpg"],
  "audio": "/AiChat/audio/newrec.wav"
}


7. DELETE /api/AI/sessions/{userId}/{sessionId}
الوصف: حذف جلسة كاملة (رسائلها وملفاتها).

Request: لا body.

Response JSON:

json
{
  "deleted": 12
}

8. POST /api/AI/ingest/craftsmen
الوصف: إدخال الحرفيين في Qdrant (تحويل إلى embeddings). يمكن تمرير ?fromId=0 للبدء من معرف معين.

Request (لا body): يمكن إضافة query parameter fromId.

Response JSON:

json
{
  "totalCraftsmen": 37,
  "totalChunksIndexed": 37,
  "message": "Ingestion complete ✓"
}

9. POST /api/AI/ingest/jobs
الوصف: إدخال حلول الوظائف المكتملة في Qdrant.

Request: لا body.

Response JSON:

json
{
  "indexed": 25
}
10. POST /api/AI/ingest/job/{jobId}
الوصف: إدخال حل وظيفة واحدة في Qdrant (عادة بعد تقييم إيجابي).

Request: لا body، معرف الوظيفة في المسار.

Response JSON:

json
{
  "jobId": 26,
  "upserted": 1
}


10. POST /api/AI/ingest/job/{jobId}
الوصف: إدخال حل وظيفة واحدة في Qdrant (عادة بعد تقييم إيجابي).

Request: لا body، معرف الوظيفة في المسار.

Response JSON:

json
{
  "jobId": 26,
  "upserted": 1
}
11. GET /api/AI/vectors/count
الوصف: عدد المتجهات (الحرفيين) في Qdrant.

Request: لا body.

Response JSON:

json
{
  "totalVectors": 37
}
12. POST /api/AI/craftsman/submit-solution
الوصف: إرسال خطوات حل من حرفي، يقوم النظام بتحسينها باستخدام LLM ثم حفظها.

Request JSON:

json
{
  "userId": 5,
  "serviceType": "سباك",
  "problemDescription": "الحنفية بتقطر بعد الإغلاق",
  "steps": [
    "قفل المحبس الرئيسي",
    "فك رأس الحنفية",
    "نزع الجلدة القديمة",
    "تركيب جلدة جديدة",
    "إعادة التركيب واختبار التسريب"
  ],
  "craftsmanId": 10
}
Response JSON:

json
{
  "jobId": 31,
  "upserted": 1,
  "originalSteps": [
    "قفل المحبس الرئيسي",
    "فك رأس الحنفية",
    "نزع الجلدة القديمة",
    "تركيب جلدة جديدة",
    "إعادة التركيب واختبار التسريب"
  ],
  "fixedSteps": [
    "1. قفل المحبس الرئيسي تحت الحوض",
    "2. فك رأس الحنفية بالمفتاح المناسب",
    "3. شيل الجلدة القديمة وحط واحدة جديدة",
    "4. ركب كل حاجة وافتح المياه جرب"
  ]
} 
