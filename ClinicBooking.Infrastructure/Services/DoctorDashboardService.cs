using ClinicBooking.Core.DTOs.Appointment;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Enums;
using ClinicBooking.Core.Interfaces;
using ClinicBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Services;

public class DoctorDashboardService : IDoctorDashboardService
{
    private readonly ApplicationDbContext _context;

    public DoctorDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentDto>> GetTodayAppointmentsForDoctorAsync(int userId)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return Enumerable.Empty<AppointmentDto>();

        var appointments = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Schedule)
            .Include(a => a.Staff).ThenInclude(s => s!.User)
            .Where(a => a.DoctorId == doctor.DoctorId)
            .OrderBy(a => a.Schedule.SlotDate)
            .ThenBy(a => a.Schedule.StartTime)
            .ToListAsync();

        return appointments.Select(ReceptionService.MapToDto);
    }

    public async Task<AppointmentDto?> CompleteAppointmentAsync(int appointmentId, int userId, CompleteAppointmentDto dto)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
        if (doctor == null) return null;

        var appointment = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Schedule)
            .Include(a => a.Staff).ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctor.DoctorId);

        if (appointment == null) return null;

        if (appointment.Status != AppointmentStatus.CheckedIn.ToString())
        {
            throw new InvalidOperationException($"Chỉ có thể hoàn thành ca khám khi bệnh nhân đã Check-in. Trạng thái hiện tại: '{appointment.Status}'.");
        }

        appointment.Status = AppointmentStatus.Completed.ToString();
        appointment.Notes = dto.Notes;
        appointment.AttachmentUrl = dto.AttachmentUrl;
        appointment.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ReceptionService.MapToDto(appointment);
    }
}
