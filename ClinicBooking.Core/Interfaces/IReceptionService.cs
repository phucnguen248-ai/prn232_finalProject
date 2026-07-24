using ClinicBooking.Core.DTOs.Appointment;

namespace ClinicBooking.Core.Interfaces;

public interface IReceptionService
{
    Task<IEnumerable<AppointmentDto>> SearchAppointmentsAsync(string query);
    Task<AppointmentDto?> CheckInAsync(int appointmentId, int userId);
    Task<AppointmentDto?> MarkNoShowAsync(int appointmentId, int userId);
}
