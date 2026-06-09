using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Chat
{
    public class SendMessageDto
    {
        [Required(ErrorMessage = "ConversationId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ConversationId must be a valid ID.")]
        public int ConversationId { get; set; }

        [Required(ErrorMessage = "Message content cannot be empty.")]
        [MaxLength(2000, ErrorMessage = "Message cannot exceed 2000 characters.")]
        public string Content { get; set; } = string.Empty;

                        [RegularExpression("^(text|image|voice|location)$",
            ErrorMessage = "MessageType must be: text, image, voice, or location.")]
        public string MessageType { get; set; } = "text";
    }
}
