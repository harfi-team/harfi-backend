using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harfi.Services.Interfaces
{
        public interface IImageservice
    {
        Task<string> SaveImageAsync(IFormFile image, string folder);
        Task<string> SaveFileAsync(
            IFormFile file,
            string folder,
            string[] allowedExtensions,
            long maxSizeBytes,
            string invalidTypeMessage,
            string invalidSizeMessage);
        void DeleteImage(string imagePath, string folder);
    }
}
