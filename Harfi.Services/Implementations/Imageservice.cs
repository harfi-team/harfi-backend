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

        public async Task<string> SaveImageAsync(IFormFile image, string folder)
        {
            if (image == null || image.Length == 0)
                throw new ArgumentException("لا يوجد ملف مرفوع.");

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException("تنسيق الصورة غير مدعوم.");

            if (image.Length > 5 * 1024 * 1024)
                throw new ArgumentException("حجم الصورة لا يمكن أن يتجاوز 5 ميغابايت.");

            var imageName = $"{Guid.NewGuid()}{extension}";

            var webRootPath =
                _webHostEnvironment.WebRootPath ??
                Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

            var folderPath = Path.Combine(webRootPath, folder);

            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, imageName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/{folder}/{imageName}";
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