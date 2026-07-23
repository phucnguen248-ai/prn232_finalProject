using ClinicBooking.Core.Enums;

namespace ClinicBooking.Core.Entities;

public class Schedule
{
    public int ScheduleId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = ScheduleStatus.Available.ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Appointment? Appointment { get; set; }
}
