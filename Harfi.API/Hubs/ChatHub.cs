using Harfi.DTOs.Chat;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Harfi.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IConversationService _convService;
        private readonly IMessageService _msgService;
        private readonly INotificationService _notifService;
        private readonly IConversationRepository _convRepo;
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub> _notifHubContext;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IConversationService convService,
            IMessageService msgService,
            INotificationService notifService,
            IConversationRepository convRepo,
            AppDbContext db,
            IHubContext<NotificationHub> notifHubContext,
            ILogger<ChatHub> logger)
        {
            _convService = convService;
            _msgService = msgService;
            _notifService = notifService;
            _convRepo = convRepo;
            _db = db;
            _notifHubContext = notifHubContext;
            _logger = logger;
        }

        // ── Connection ────────────────────────────────────────────
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();

            try
            {
                // نتحقق لو الاتصال ده مش متسجل قبل كده لمنع التكرار
                var alreadyExists = await _db.UserConnections
                    .AnyAsync(c => c.ConnectionId == Context.ConnectionId);

                if (!alreadyExists)
                {
                    _db.UserConnections.Add(new UserConnection
                    {
                        UserId = userId,
                        ConnectionId = Context.ConnectionId,
                        IsConnected = true
                    });

                    await _db.SaveChangesAsync();
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
                await Clients.Others.SendAsync("UserOnline", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for UserId={UserId}", userId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                int? userId = null;

                var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim)) userId = int.Parse(userIdClaim);

                if (userId == null)
                {
                    var conn = await _db.UserConnections.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);
                    userId = conn?.UserId;
                }

                await _db.UserConnections
                    .Where(c => c.ConnectionId == Context.ConnectionId)
                    .ExecuteDeleteAsync();

                if (userId.HasValue)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserGroup(userId.Value));

                    var hasOtherConnections = await _db.UserConnections
                        .AsNoTracking()
                        .AnyAsync(c => c.UserId == userId.Value && c.IsConnected);

                    if (!hasOtherConnections)
                    {
                        await Clients.Others.SendAsync("UserOffline", userId.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ── Join Conversation ─────────────────────────────────────
        public async Task JoinConversation(int conversationId)
        {
            if (conversationId <= 0)
                throw new HubException("معرف المحادثة غير صالح.");

            if (!await _convService.IsParticipantAsync(conversationId, GetUserId()))
                throw new HubException("الوصول مرفوض.");

            await Groups.AddToGroupAsync(
                Context.ConnectionId, Group(conversationId));
        }

        // ── Leave Conversation ────────────────────────────────────
        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId, Group(conversationId));
        }

        // ── Send Message ──────────────────────────────────────────
        public async Task SendMessage(SendMessageDto dto)
        {
            // Manual validation
            if (dto.ConversationId <= 0)
                throw new HubException("معرف المحادثة غير صالح.");

            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Length > 2000)
                throw new HubException("محتوى الرسالة غير صالح.");

            var allowed = new[] { "text", "image", "voice", "location" };
            if (!allowed.Contains(dto.MessageType))
                throw new HubException("نوع الرسالة غير صالح. يجب أن يكون: نص أو صورة أو صوت أو موقع.");
            var senderId = GetUserId();

            if (!await _convService.IsParticipantAsync(dto.ConversationId, senderId))
                throw new HubException("الوصول مرفوض.");
            // Save message
            var message = await _msgService.SaveMessageAsync(
                dto.ConversationId, senderId, dto.Content, dto.MessageType);

            // Notification للطرف الآخر
            var conversation = await _convRepo.GetByIdWithDetailsAsync(dto.ConversationId);
            if (conversation == null)
                throw new HubException("المحادثة غير موجودة.");

            var receiverId = conversation.CustomerId == senderId
                ? conversation.Craftsman.UserId
                : conversation.CustomerId;

            // Broadcast message body for open chat screens (critical path)
            await Clients
                .Groups(UserGroup(senderId), UserGroup(receiverId))
                .SendAsync("ReceiveMessage", message);

            // Notifications are non-critical for send acknowledgement
            try
            {
                var notifDto = await _notifService.CreateMessageNotificationAsync(
                    receiverId,
                    message.SenderName,
                    dto.Content,
                    dto.MessageType,
                    dto.ConversationId);

                await _notifHubContext.Clients
                    .Group(UserGroup(receiverId))
                    .SendAsync("ReceiveNotification", notifDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create/send message notification. ConversationId={ConversationId}, SenderId={SenderId}, ReceiverId={ReceiverId}",
                    dto.ConversationId,
                    senderId,
                    receiverId);
            }

            // Conversation list snapshots are non-critical for send acknowledgement
            try
            {
                var senderConversation = await _convService.GetByIdAsync(dto.ConversationId, senderId);
                var receiverConversation = await _convService.GetByIdAsync(dto.ConversationId, receiverId);

                if (senderConversation != null)
                {
                    await Clients
                        .Group(UserGroup(senderId))
                        .SendAsync("ConversationUpdated", senderConversation);
                }

                if (receiverConversation != null)
                {
                    await Clients
                        .Group(UserGroup(receiverId))
                        .SendAsync("ConversationUpdated", receiverConversation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send conversation snapshot updates. ConversationId={ConversationId}, SenderId={SenderId}, ReceiverId={ReceiverId}",
                    dto.ConversationId,
                    senderId,
                    receiverId);
            }
        }

        // ── Delete Message ────────────────────────────────────────
        public async Task DeleteMessage(int conversationId, int messageId)
        {
            if (conversationId <= 0 || messageId <= 0)
                throw new HubException("معرف الرسالة أو المحادثة غير صالح.");

            var userId = GetUserId();

            if (!await _convService.IsParticipantAsync(conversationId, userId))
                throw new HubException("الوصول مرفوض.");

            var deleted = await _msgService.DeleteMessageAsync(conversationId, messageId, userId);
            if (!deleted)
                throw new HubException("لا يمكن حذف هذه الرسالة.");

            await Clients
                .Group(Group(conversationId))
                .SendAsync("MessageDeleted", conversationId, messageId);
        }

        // ── Typing Indicator ──────────────────────────────────────
        public async Task Typing(int conversationId)
        {
            if (conversationId <= 0) return;

            var userId = GetUserId();
            if (!await _convService.IsParticipantAsync(conversationId, userId))
                return;

            var conversation = await _convRepo.GetByIdWithDetailsAsync(conversationId);
            if (conversation == null) return;

            var otherUserId = conversation.CustomerId == userId
                ? conversation.Craftsman.UserId
                : conversation.CustomerId;

            await Clients
                .Group(UserGroup(otherUserId))
                .SendAsync("UserTyping", conversationId, userId);
        }

        // ── Mark as Read ──────────────────────────────────────────
        public async Task MarkAsRead(int conversationId)
        {
            if (conversationId <= 0) return;

            var userId = GetUserId();
            if (!await _convService.IsParticipantAsync(conversationId, userId))
                return;

            await _msgService.MarkConversationAsReadAsync(conversationId, userId);

            var conversation = await _convRepo.GetByIdWithDetailsAsync(conversationId);
            if (conversation == null) return;

            var otherUserId = conversation.CustomerId == userId
                ? conversation.Craftsman.UserId
                : conversation.CustomerId;

            await Clients
                .Group(UserGroup(otherUserId))
                .SendAsync("MessagesRead", conversationId, userId);

            var updatedConversation = await _convService.GetByIdAsync(conversationId, userId);
            if (updatedConversation != null)
            {
                await Clients
                    .Group(UserGroup(userId))
                    .SendAsync("ConversationUpdated", updatedConversation);
            }
        }
        [HubMethodName("SetOffline")]
        public async Task SetOffline()
        {
            try
            {
                var userId = int.Parse(Context.User!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

                await _db.UserConnections
                  .Where(c => c.ConnectionId == Context.ConnectionId)
                  .ExecuteDeleteAsync();

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

                var hasOtherConnections = await _db.UserConnections
                    .AsNoTracking()
                    .AnyAsync(c => c.UserId == userId && c.IsConnected);

                if (!hasOtherConnections)
                {
                    await Clients.Others.SendAsync("UserOffline", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetOffline method");
            }
        }

        // ── Helpers ───────────────────────────────────────────────
        private static string Group(int conversationId) => $"conv_{conversationId}";
        private static string UserGroup(int userId) => $"user_{userId}";

        private int GetUserId() =>
            int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
