using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Chat
{
    public class CreateConversationDto
    {
        [Required(ErrorMessage = "JobId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "JobId must be a valid ID.")]
        public int JobId { get; set; }

        [Required(ErrorMessage = "CraftsmanId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CraftsmanId must be a valid ID.")]
        public int CraftsmanId { get; set; }
    }
}
