using Harfi.DTOs.Chat;
using Harfi.Models.Constants;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Harfi.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _convService;
        private readonly IMessageService _msgService;
        private readonly IJobRepository _jobRepo;
        private readonly IImageservice _imageService;

        public ConversationsController(
            IConversationService convService,
            IMessageService msgService,
            IJobRepository jobRepo,
            IImageservice imageService)
        {
            _convService = convService;
            _msgService = msgService;
            _jobRepo = jobRepo;
            _imageService = imageService;
        }

        // POST /api/conversations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateConversationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customerId = GetUserId();

            if (customerId == dto.CraftsmanId)
                return BadRequest("لا يمكنك إنشاء محادثة مع نفسك.");

            var job = await _jobRepo.GetByIdAsync(dto.JobId);
            if (job == null)
                return NotFound("الوظيفة غير موجودة.");

            if (job.Status == JobStatusConstants.Rejected || job.Status == JobStatusConstants.Cancelled)
                return BadRequest( $"لا يمكن بدء محادثة على وظيفة {job.Status}.");

            var conversation = await _convService
                .GetOrCreateAsync(dto.JobId, customerId, dto.CraftsmanId);

            return Ok(conversation);
        }

        // GET /api/conversations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var conversations = await _convService
                .GetUserConversationsAsync(GetUserId());
            return Ok(conversations);
        }

        // GET /api/conversations/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0) return BadRequest("معرف المحادثة غير صالح.");

            var conversation = await _convService.GetByIdAsync(id, GetUserId());
            if (conversation == null)
                return NotFound("المحادثة غير موجودة أو الوصول مرفوض.");
            return Ok(conversation);
        }

        // GET /api/conversations/{id}/messages?page=1&pageSize=20
        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetMessages(
            int id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (id <= 0) return BadRequest("معرف المحادثة غير صالح.");
            if (page <= 0 || pageSize <= 0 || pageSize > 100)
                return BadRequest("معلمات الترقيم غير صالحة.");

            var userId = GetUserId();

            if (!await _convService.IsParticipantAsync(id, userId))
                return Forbid();

            var messages = await _msgService
                .GetMessagesAsync(id, userId, page, pageSize);
            return Ok(messages);
        }

        // PUT /api/conversations/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (id <= 0) return BadRequest("معرف المحادثة غير صالح.");

            var userId = GetUserId();

            if (!await _convService.IsParticipantAsync(id, userId))
                return Forbid();

            await _msgService.MarkConversationAsReadAsync(id, userId);
            return NoContent();
        }

                // DELETE /api/conversations/{id} – per-user hide
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0) return BadRequest("معرف المحادثة غير صالح.");

            var userId = GetUserId();

            if (!await _convService.IsParticipantAsync(id, userId))
                return Forbid();

            var hidden = await _convService.HideConversationAsync(id, userId);
            if (!hidden) return NotFound();

            return NoContent();
        }

        // DELETE /api/conversations/{id}/messages/{messageId}
        [HttpDelete("{id}/messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int id, int messageId)
        {
            if (id <= 0 || messageId <= 0)
                return BadRequest("معرف الرسالة أو المحادثة غير صالح.");

            var userId = GetUserId();

            if (!await _convService.IsParticipantAsync(id, userId))
                return Forbid();

            var deleted = await _msgService.DeleteMessageAsync(id, messageId, userId);
            if (!deleted)
                return BadRequest("لا يمكن حذف هذه الرسالة.");

            return NoContent();
        }

        // POST /api/conversations/upload-image
        [HttpPost("upload-image")]

        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "يرجى اختيار صورة للرفع." });

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest(new { message = "نوع الملف غير مدعوم. الأنواع المسموحة: JPG, PNG, WEBP." });

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "حجم الصورة يجب أن لا يتجاوز 5 ميجابايت." });

            var url = await _imageService.SaveImageAsync(file, "chat");
            return Ok(new { url });
        }

        // POST /api/conversations/upload-voice
                [HttpPost("upload-voice")]
        public async Task<IActionResult> UploadVoice([FromForm] IFormFile voice)
        {
            if (voice == null || voice.Length == 0)
                return BadRequest("يرجى اختيار ملف صوتي للرفع.");

            var allowed = new[] { ".mp3", ".wav", ".ogg", ".webm" };

            try
            {
                var url = await _imageService.SaveFileAsync(
                    voice,
                    "chat-voices",
                    allowed,
                    10 * 1024 * 1024,
                    "نوع الملف الصوتي غير مدعوم. الأنواع المسموحة: MP3, WAV, OGG, WEBM.",
                    "حجم الملف الصوتي يجب أن لا يتجاوز 10 ميجابايت.");

                return Ok(new { url });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
