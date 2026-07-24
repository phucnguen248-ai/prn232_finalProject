using System;

namespace ClinicBooking.Core.DTOs
{
    public class CreateBookingDto
    {
        public int DoctorId { get; set; }
        public int ScheduleId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public DateTime? PatientDob { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
