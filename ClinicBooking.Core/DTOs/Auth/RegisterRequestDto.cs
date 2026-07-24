using ClinicBooking.Core.Enums;

namespace ClinicBooking.Core.DTOs.Auth;

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = UserRole.Patient.ToString();

    // Optional Patient info if Role is Patient
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
}
