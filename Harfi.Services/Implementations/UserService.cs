using Harfi.DTOs.User;
using Harfi.Models.Entities;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Harfi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IImageservice _imageService;

        public UserService(UserManager<User> userManager, IImageservice imageService)
        {
            _userManager = userManager;
            _imageService = imageService;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return null;

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Role = user.Role,
                Phone = user.Phone,
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            user.Name = updateUserDto.Name;
            user.Phone = updateUserDto.Phone;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<string?> UploadProfileImageAsync(int userId, IFormFile file)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return null;

            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                _imageService.DeleteImage(user.ProfileImageUrl, "profiles");

            var imageUrl = await _imageService.SaveImageAsync(file, "profiles");

            user.ProfileImageUrl = imageUrl;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded ? imageUrl : null;
        }
    }
}