namespace ClinicBooking.Core.DTOs.Doctor;

public class DoctorDto
{
    public int DoctorId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int SpecializationId { get; set; }
    public string SpecializationName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Bio { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateDoctorDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int SpecializationId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Bio { get; set; } = string.Empty;
}

public class UpdateDoctorDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int SpecializationId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Bio { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
