using Harfi.DTOs.Chat;
using Harfi.Models.Constants;
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

        public ConversationsController(
            IConversationService convService,
            IMessageService msgService)
        {
            _convService = convService;
            _msgService = msgService;
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

            try
            {
                await _convService.ValidateJobForConversationAsync(
                    dto.JobId, dto.CraftsmanId);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

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

        private int GetUserId() =>
            int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
