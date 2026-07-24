using System;

namespace ClinicBooking.Core.DTOs
{
    public class ScheduleSlotDto
    {
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public DateOnly SlotDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
