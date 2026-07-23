using ClinicBooking.Core.Enums;

namespace ClinicBooking.Core.Entities;

public class Appointment
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int ScheduleId { get; set; }
    public int? StaffId { get; set; }
    public string Status { get; set; } = AppointmentStatus.Confirmed.ToString();
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Schedule Schedule { get; set; } = null!;
    public virtual Staff? Staff { get; set; }
}
