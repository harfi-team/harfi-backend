using Harfi.DTOs.Craftsman;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Harfi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CraftsmenController : ControllerBase
    {
        private readonly ICraftsmanService _craftsmanService;

        public CraftsmenController(ICraftsmanService craftsmanService)
        {
            _craftsmanService = craftsmanService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] CreateCraftsmanDto dto)
        {
            var result = await _craftsmanService.RegisterCraftsmanAsync(dto);
            if (!result) return BadRequest("He failed to submit the registration application.");
            return Ok("Your application has been successfully submitted and is currently under review.");
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(int id)
        {
            var profile = await _craftsmanService.GetCraftsmanProfileAsync(id);
            if (profile == null) return NotFound("The Craftsman is not Present");
            return Ok(profile);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] CraftsmanFilterDto filter)
        {
            var results = await _craftsmanService.GetFilteredCraftsmenAsync(filter);
            return Ok(results);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateCraftsmanDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _craftsmanService.UpdateCraftsmanAsync(id, dto);
            if (!result) return NotFound(new { message = "Craftsman not found" });

            return Ok(new { message = "Profile updated successfully" });
        }

    }
}