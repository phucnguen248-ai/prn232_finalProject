namespace ClinicBooking.Core.DTOs;

public class DoctorSummaryDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string SpecializationName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Bio { get; set; } = string.Empty;
}
