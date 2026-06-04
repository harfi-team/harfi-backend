using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.DTOs.Review
{
    /// Data returned to client after submitting a review
    /// OR when fetching a craftsman's list of reviews.
    /// 
    /// Used by TWO endpoints:
    ///   POST /api/reviews     → returns this after successful submit
    ///   GET  /api/reviews/craftsman/{id} → returns a List of this
    public class ReviewResponseDto
    {
        
        public int Id { get; set; }

       
        public int JobId { get; set; }

  
        public int Stars { get; set; }

      
        public string? Comment { get; set; }

     
        public string CustomerName { get; set; } = string.Empty;

       
        public DateTime CreatedAt { get; set; }
    }
}
