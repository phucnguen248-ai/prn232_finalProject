namespace ClinicBooking.Core.Entities;

public class Staff
{
    public int StaffId { get; set; }
    public int UserId { get; set; }
    public string Position { get; set; } = string.Empty;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
