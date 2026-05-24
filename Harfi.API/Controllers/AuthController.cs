using System.Security.Claims;
using Harfi.DTOs.Auth;
using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Harfi.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ── POST /api/auth/register ───────────────────────────────
    /// <summary>تسجيل مستخدم جديد (عميل أو حرفي)</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(dto);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // ── POST /api/auth/login ──────────────────────────────────
    /// <summary>تسجيل الدخول والحصول على التوكن</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }

    // ── POST /api/auth/refresh ────────────────────────────────
    /// <summary>تجديد الـ Access Token باستخدام Refresh Token</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return Ok(result);
    }

    // ── POST /api/auth/logout ─────────────────────────────────
    /// <summary>تسجيل الخروج وإلغاء Refresh Token</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        await _authService.LogoutAsync(dto.RefreshToken);
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

    // ── GET /api/auth/me ──────────────────────────────────────
    /// <summary>بيانات المستخدم الحالي من التوكن</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var name = User.FindFirstValue(ClaimTypes.Name);

        return Ok(new { id, name, email, role });
    }

    // ── GET /api/auth/admin-only (example) ───────────────────
    /// <summary>مثال على endpoint مخصص للأدمن فقط</summary>
    [HttpGet("admin-only")]
    [Authorize(Roles = "admin")]
    public IActionResult AdminOnly()
        => Ok(new { message = "أهلاً بالأدمن 👋" });

    // ── GET /api/auth/craftsman-only (example) ────────────────
    /// <summary>مثال على endpoint مخصص للحرفي فقط</summary>
    [HttpGet("craftsman-only")]
    [Authorize(Roles = "craftsman")]
    public IActionResult CraftsmanOnly()
        => Ok(new { message = "أهلاً بالحرفي 🔧" });


    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        var message = await _authService.VerifyEmailAsync(dto);
        return Ok(new { success = true, message });
    }

    [HttpPost("resend-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendCode([FromBody] ResendCodeDto dto)
    {
        var message = await _authService.ResendVerificationCodeAsync(dto);
        return Ok(new { success = true, message });
    }
}
