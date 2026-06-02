using Harfi.DTOs.User;
using Harfi.Models.Entities;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Harfi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // 1. جلب بيانات البروفايل للمستخدم
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

        // 2. تحديث بيانات البروفايل
        public async Task<bool> UpdateUserProfileAsync(int userId, UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            user.Name = updateUserDto.Name;
            user.Phone = updateUserDto.Phone;
            if (!string.IsNullOrEmpty(updateUserDto.ProfileImageUrl))
            {
                user.ProfileImageUrl = updateUserDto.ProfileImageUrl;
            }

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}