using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harfi.DTOs.User;
using Harfi.Models.Entities;
using Harfi.Repositories.Interfaces;
using Harfi.Services.Interfaces;

namespace Harfi.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository; 

        public UserService(IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        // 1. جلب بيانات البروفايل للمستخدم
        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            // تحويل الـ Entity إلى DTO
            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
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
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // تحديث الحقول المسموح بتعديلها فقط
            user.Name = updateUserDto.Name;
            user.Phone = updateUserDto.Phone;
            if (!string.IsNullOrEmpty(updateUserDto.ProfileImageUrl))
            {
                user.ProfileImageUrl = updateUserDto.ProfileImageUrl;
            }

             _userRepository.Update( user);
            return true;
        }
    }
}