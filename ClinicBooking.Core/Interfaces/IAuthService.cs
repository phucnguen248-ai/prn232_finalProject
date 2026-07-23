using ClinicBooking.Core.DTOs.Auth;

namespace ClinicBooking.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<UserDto?> GetUserByIdAsync(int userId);
}
