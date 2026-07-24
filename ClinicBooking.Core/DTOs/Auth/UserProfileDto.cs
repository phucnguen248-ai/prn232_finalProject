namespace ClinicBooking.Core.DTOs.Auth;

public class UserProfileDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Patient Fields
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }

    // Doctor Fields
    public string? SpecializationName { get; set; }
    public string? LicenseNumber { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Bio { get; set; }

    // Staff Fields
    public string? Position { get; set; }
}

public class UpdateProfileDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    // Patient Fields
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }

    // Doctor Fields
    public int? YearsOfExperience { get; set; }
    public string? Bio { get; set; }

    // Staff Fields
    public string? Position { get; set; }

    // Change Password Options
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
