using Harfi.DTOs.Chat;
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
                return BadRequest("Cannot start a conversation with yourself.");

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
            if (id <= 0) return BadRequest("Invalid conversation ID.");

            var conversation = await _convService.GetByIdAsync(id, GetUserId());
            if (conversation == null)
                return NotFound("Conversation not found or access denied.");

            return Ok(conversation);
        }

        // GET /api/conversations/{id}/messages?page=1&pageSize=20
        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetMessages(
            int id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (id <= 0) return BadRequest("Invalid conversation ID.");
            if (page <= 0 || pageSize <= 0 || pageSize > 100)
                return BadRequest("Invalid pagination parameters.");

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
            if (id <= 0) return BadRequest("Invalid conversation ID.");

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
