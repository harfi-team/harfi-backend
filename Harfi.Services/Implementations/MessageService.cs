using Harfi.DTOs.Chat;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _msgRepo;
        private readonly IConversationRepository _convRepo;
        private readonly IImageservice _imageService;

        public MessageService(
            IMessageRepository msgRepo,
            IConversationRepository convRepo,
            IImageservice imageService)
        {
            _msgRepo = msgRepo;
            _convRepo = convRepo;
            _imageService = imageService;
        }

        public async Task<MessageDto> SaveMessageAsync(
            int conversationId, int senderId, string content, string messageType)
        {
            var saved = await _msgRepo.AddAsync(new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                MessageType = messageType
            });
            await _msgRepo.SaveChangesAsync();

            // Load sender بعد الـ save
            await _msgRepo.LoadReferenceAsync(saved, m => m.Sender);

            // تحديث LastMessageAt في الـ Conversation
            var conversation = await _convRepo.GetByIdWithDetailsAsync(conversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = saved.SentAt;
                conversation.UpdatedAt = DateTime.UtcNow;
                _convRepo.Update(conversation);
                await _convRepo.SaveChangesAsync();
            }

            return MapToDto(saved);
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(
            int conversationId, int userId, int page, int pageSize)
        {
            var messages = await _msgRepo
                .GetByConversationAsync(conversationId, page, pageSize);
            return messages.Select(MapToDto);
        }

                public Task MarkConversationAsReadAsync(int conversationId, int userId)
            => _msgRepo.MarkConversationAsReadAsync(conversationId, userId);

        public async Task<bool> DeleteMessageAsync(int conversationId, int messageId, int userId)
        {
            var message = await _msgRepo.FirstOrDefaultAsync(m =>
                m.Id == messageId &&
                m.ConversationId == conversationId);

            if (message == null) return false;
            if (message.SenderId != userId) return false;

            _msgRepo.Remove(message);
            await _msgRepo.SaveChangesAsync();

            DeleteAttachmentIfExists(message);
            return true;
        }

        private void DeleteAttachmentIfExists(Message message)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
                return;

            if (!message.Content.StartsWith('/'))
                return;

            var type = message.MessageType?.Trim().ToLowerInvariant();

            if (type == "image")
            {
                _imageService.DeleteImage(message.Content, "chat");
                return;
            }

            if (type == "voice")
            {
                _imageService.DeleteImage(message.Content, "chat-voices");
            }
        }


        // ── Mapper ────────────────────────────────────────────────
        private static MessageDto MapToDto(Message m) => new()
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.Name ?? string.Empty,
            SenderAvatar = m.Sender?.ProfileImageUrl,
            Content = m.Content,
            MessageType = m.MessageType,
            IsRead = m.IsRead,
            SentAt = DateTime.SpecifyKind(m.SentAt, DateTimeKind.Utc)
        };
    }
}
