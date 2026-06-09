using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Harfi.Services.Implementations
{
    public class Imageservice : IImageservice
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public Imageservice(
            IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

                public Task<string> SaveImageAsync(IFormFile image, string folder)
        {
            return SaveFileAsync(
                image,
                folder,
                AllowedExtensions,
                5 * 1024 * 1024,
                "تنسيق الصورة غير مدعوم.",
                "حجم الصورة لا يمكن أن يتجاوز 5 ميغابايت.");
        }

        public async Task<string> SaveFileAsync(
            IFormFile file,
            string folder,
            string[] allowedExtensions,
            long maxSizeBytes,
            string invalidTypeMessage,
            string invalidSizeMessage)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("لا يوجد ملف مرفوع.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var normalizedExtensions = allowedExtensions
                .Select(e => e.ToLowerInvariant())
                .ToArray();

            if (!normalizedExtensions.Contains(extension))
                throw new ArgumentException(invalidTypeMessage);

            if (file.Length > maxSizeBytes)
                throw new ArgumentException(invalidSizeMessage);

            var fileName = $"{Guid.NewGuid()}{extension}";

            var webRootPath =
                _webHostEnvironment.WebRootPath ??
                Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

            var folderPath = Path.Combine(webRootPath, folder);

            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folder}/{fileName}";
        }

        public void DeleteImage(string imagePath, string folder)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            var imageName = Path.GetFileName(imagePath);

            var rootPath =
                _webHostEnvironment.WebRootPath ??
                Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

            var fullPath = Path.Combine(rootPath, folder, imageName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}