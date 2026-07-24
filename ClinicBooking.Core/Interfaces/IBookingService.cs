using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicBooking.Core.DTOs;

namespace ClinicBooking.Core.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<ScheduleSlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date);
        Task<BookingResult> BookAppointmentAsync(CreateBookingDto dto, int currentUserId);
        Task<CancellationResult> CancelAppointmentAsync(int appointmentId, int currentUserId);
        Task<IEnumerable<AppointmentHistoryDto>> GetPatientHistoryAsync(int currentUserId);
    }

    public class BookingResult
    {
        public bool Success { get; set; }
        public int AppointmentId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsConflict { get; set; }
    }

    public class CancellationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsBadRequest { get; set; }
    }
}
