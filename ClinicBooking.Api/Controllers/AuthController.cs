using System.Security.Claims;
using ClinicBooking.Core.DTOs.Auth;
using ClinicBooking.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng nhập tài khoản hệ thống (Admin, Staff, Doctor, Patient)
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác, hoặc tài khoản đã bị khóa." });
        }

        return Ok(result);
    }

    /// <summary>
    /// Đăng ký tài khoản Bệnh nhân mới
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(GetMe), null, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thông tin tài khoản đang đăng nhập
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "Không tìm thấy thông tin người dùng." });
        }

        return Ok(user);
    }

    /// <summary>
    /// Xem chi tiết sơ yếu lý lịch cá nhân theo quyền (Bác sĩ, Bệnh nhân, Lễ tân, Admin)
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        var profile = await _authService.GetUserProfileAsync(userId);
        if (profile == null) return NotFound(new { message = "Không tìm thấy thông tin sơ yếu lý lịch." });

        return Ok(profile);
    }

    /// <summary>
    /// Cập nhật hồ sơ sơ yếu lý lịch cá nhân & Đổi mật khẩu
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        int userId = GetCurrentUserId();
        if (userId <= 0) return Unauthorized();

        try
        {
            var updatedProfile = await _authService.UpdateUserProfileAsync(userId, dto);
            return Ok(updatedProfile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Đổi mật khẩu ngoài màn hình Login qua xác thực Email + SĐT
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            await _authService.ForgotPasswordResetAsync(dto);
            return Ok(new { message = "Đổi mật khẩu thành công! Bạn có thể đăng nhập ngay với mật khẩu mới." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                 ?? User.FindFirst("sub")?.Value 
                 ?? User.FindFirst("nameid")?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }
}
