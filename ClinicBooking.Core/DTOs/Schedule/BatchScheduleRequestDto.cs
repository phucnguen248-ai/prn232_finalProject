namespace ClinicBooking.Core.DTOs.Schedule;

public class TimeSlotDto
{
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

public class BatchScheduleRequestDto
{
    public int DoctorId { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public List<TimeSlotDto> SelectedSlots { get; set; } = new();
}

public class RequestCancelSlotDto
{
    public string Reason { get; set; } = string.Empty;
}

public class ScheduleDto
{
    public int ScheduleId { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecializationName { get; set; } = string.Empty;
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CancelReason { get; set; }
    public DateTime? CancelRequestedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
