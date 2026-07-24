using ClinicBooking.Core.DTOs.Appointment;

namespace ClinicBooking.Core.Interfaces;

public interface IDoctorDashboardService
{
    Task<IEnumerable<AppointmentDto>> GetTodayAppointmentsForDoctorAsync(int userId);
    Task<AppointmentDto?> CompleteAppointmentAsync(int appointmentId, int userId, CompleteAppointmentDto dto);
}
