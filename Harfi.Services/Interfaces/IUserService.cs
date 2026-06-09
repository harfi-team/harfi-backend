using Harfi.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace Harfi.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, UpdateUserDto updateUserDto);
        Task<string?> UploadProfileImageAsync(int userId, IFormFile file);
    }
}

