using Harfi.DTOs.Chat;
using Harfi.Models.Entities;
using Harfi.Repositories.Data;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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

        public ChatHub(
            IConversationService convService,
            IMessageService msgService,
            INotificationService notifService,
            IConversationRepository convRepo,
            AppDbContext db)
        {
            _convService = convService;
            _msgService = msgService;
            _notifService = notifService;
            _convRepo = convRepo;
            _db = db;
        }

        // ── Connection ────────────────────────────────────────────
        public override async Task OnConnectedAsync()
        {
            _db.UserConnections.Add(new UserConnection
            {
                UserId = GetUserId(),
                ConnectionId = Context.ConnectionId,
                IsConnected = true
            });

            await _db.SaveChangesAsync();
            await Clients.Others.SendAsync("UserOnline", GetUserId());
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var conn = await _db.UserConnections
                .FirstOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);

            if (conn != null)
            {
                conn.IsConnected = false;
                conn.DisconnectedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            await Clients.Others.SendAsync("UserOffline", GetUserId());
            await base.OnDisconnectedAsync(exception);
        }

        // ── Join Conversation ─────────────────────────────────────
        public async Task JoinConversation(int conversationId)
        {
            if (conversationId <= 0)
                throw new HubException("Invalid conversationId.");

            if (!await _convService.IsParticipantAsync(conversationId, GetUserId()))
                throw new HubException("Access denied.");

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
                throw new HubException("Invalid conversationId.");

            if (string.IsNullOrWhiteSpace(dto.Content) || dto.Content.Length > 2000)
                throw new HubException("Invalid message content.");

            var allowed = new[] { "text", "image", "system" };
            if (!allowed.Contains(dto.MessageType))
                throw new HubException("MessageType must be: text, image, or system.");

            var senderId = GetUserId();

            if (!await _convService.IsParticipantAsync(dto.ConversationId, senderId))
                throw new HubException("Access denied.");

            // Save message
            var message = await _msgService.SaveMessageAsync(
                dto.ConversationId, senderId, dto.Content, dto.MessageType);

            // Notification للطرف الآخر
            var conversation = await _convRepo.GetByIdAsync(dto.ConversationId);
            if (conversation != null)
            {
                var receiverId = conversation.CustomerId == senderId
                    ? conversation.Craftsman.UserId
                    : conversation.CustomerId;

                await _notifService.CreateMessageNotificationAsync(
                    receiverId,
                    message.SenderName,
                    dto.Content);
            }

            // Broadcast
            await Clients
                .Group(Group(dto.ConversationId))
                .SendAsync("ReceiveMessage", message);
        }

        // ── Typing Indicator ──────────────────────────────────────
        public async Task Typing(int conversationId)
        {
            if (conversationId <= 0) return;

            await Clients
                .OthersInGroup(Group(conversationId))
                .SendAsync("UserTyping", GetUserId());
        }

        // ── Mark as Read ──────────────────────────────────────────
        public async Task MarkAsRead(int conversationId)
        {
            if (conversationId <= 0) return;

            var userId = GetUserId();
            await _msgService.MarkConversationAsReadAsync(conversationId, userId);

            await Clients
                .OthersInGroup(Group(conversationId))
                .SendAsync("MessagesRead", conversationId);
        }

        // ── Helpers ───────────────────────────────────────────────
        private static string Group(int conversationId) => $"conv_{conversationId}";

        private int GetUserId() =>
            int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}
