namespace ClinicBooking.Core.Entities;

public class Doctor
{
    public int DoctorId { get; set; }
    public int UserId { get; set; }
    public int SpecializationId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Bio { get; set; } = string.Empty;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Specialization Specialization { get; set; } = null!;
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
