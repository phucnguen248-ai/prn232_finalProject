using ClinicBooking.Core.DTOs.Auth;

namespace ClinicBooking.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<UserDto?> GetUserByIdAsync(int userId);

    Task<UserProfileDto?> GetUserProfileAsync(int userId);
    Task<UserProfileDto> UpdateUserProfileAsync(int userId, UpdateProfileDto dto);
    Task<bool> ForgotPasswordResetAsync(ForgotPasswordDto dto);
}
