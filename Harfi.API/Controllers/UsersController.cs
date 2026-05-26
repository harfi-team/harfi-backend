using Harfi.DTOs.User;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Harfi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var profile = await _userService.GetUserProfileAsync(id);
            if (profile == null) return NotFound("User not found");
            return Ok(profile);
        }

        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateUserDto dto)
        {
            var result = await _userService.UpdateUserProfileAsync(id, dto);
            if (!result) return BadRequest("Data could not be updated ");
            return Ok("The profile has been successfully updated.");
        }
    }
}