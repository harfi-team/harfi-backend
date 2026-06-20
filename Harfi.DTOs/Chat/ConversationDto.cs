using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Chat
{
    public class ConversationDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? OtherUserAvatar { get; set; }
        public string? LastMessage { get; set; }
        public string? LastMessageType { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }
}
