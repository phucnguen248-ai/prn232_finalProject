using ClinicBooking.Core.DTOs.Appointment;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Enums;
using ClinicBooking.Core.Interfaces;
using ClinicBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Services;

public class ReceptionService : IReceptionService
{
    private readonly ApplicationDbContext _context;

    public ReceptionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AppointmentDto>> SearchAppointmentsAsync(string query)
    {
        query = query.Trim().ToLower();

        var appointments = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Schedule)
            .Include(a => a.Staff).ThenInclude(s => s!.User)
            .Where(a => a.Patient.User.PhoneNumber.Contains(query) ||
                        a.Patient.User.FullName.ToLower().Contains(query) ||
                        a.AppointmentId.ToString() == query)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return appointments.Select(MapToDto);
    }

    public async Task<AppointmentDto?> CheckInAsync(int appointmentId, int userId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Schedule)
            .Include(a => a.Staff).ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        if (appointment == null) return null;

        if (appointment.Status != AppointmentStatus.Confirmed.ToString())
        {
            throw new InvalidOperationException($"Không thể Check-in cuộc hẹn có trạng thái: '{appointment.Status}'.");
        }

        var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.UserId == userId);
        if (staff != null)
        {
            appointment.StaffId = staff.StaffId;
        }

        appointment.Status = AppointmentStatus.CheckedIn.ToString();
        appointment.CheckedInAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> MarkNoShowAsync(int appointmentId, int userId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Schedule)
            .Include(a => a.Staff).ThenInclude(s => s!.User)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        if (appointment == null) return null;

        if (appointment.Status != AppointmentStatus.Confirmed.ToString() && appointment.Status != AppointmentStatus.CheckedIn.ToString())
        {
            throw new InvalidOperationException($"Không thể đánh dấu No-Show cho cuộc hẹn có trạng thái: '{appointment.Status}'.");
        }

        var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.UserId == userId);
        if (staff != null)
        {
            appointment.StaffId = staff.StaffId;
        }

        appointment.Status = AppointmentStatus.NoShow.ToString();
        await _context.SaveChangesAsync();

        return MapToDto(appointment);
    }

    public static AppointmentDto MapToDto(Appointment a)
    {
        return new AppointmentDto
        {
            AppointmentId = a.AppointmentId,
            PatientId = a.PatientId,
            PatientName = a.Patient?.User?.FullName ?? string.Empty,
            PatientPhone = a.Patient?.User?.PhoneNumber ?? string.Empty,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor?.User?.FullName ?? string.Empty,
            SpecializationName = a.Doctor?.Specialization?.Name ?? string.Empty,
            ScheduleId = a.ScheduleId,
            SlotDate = a.Schedule?.SlotDate ?? DateOnly.MinValue,
            StartTime = a.Schedule?.StartTime ?? TimeOnly.MinValue,
            EndTime = a.Schedule?.EndTime ?? TimeOnly.MinValue,
            StaffId = a.StaffId,
            StaffName = a.Staff?.User?.FullName,
            Status = a.Status,
            Reason = a.Reason,
            Notes = a.Notes,
            AttachmentUrl = a.AttachmentUrl,
            CreatedAt = a.CreatedAt,
            CheckedInAt = a.CheckedInAt,
            CompletedAt = a.CompletedAt,
            CancelledAt = a.CancelledAt
        };
    }
}
