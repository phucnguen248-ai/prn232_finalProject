using ClinicBooking.Core.DTOs.Schedule;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Enums;
using ClinicBooking.Core.Interfaces;
using ClinicBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Services;

public class ScheduleBatchService : IScheduleBatchService
{
    private readonly ApplicationDbContext _context;

    public ScheduleBatchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ScheduleDto>> GetSchedulesAsync(int? specializationId, int? doctorId, DateOnly? fromDate, DateOnly? toDate, string? status)
    {
        var query = _context.Schedules
            .Include(s => s.Doctor).ThenInclude(d => d.User)
            .Include(s => s.Doctor).ThenInclude(d => d.Specialization)
            .AsQueryable();

        if (specializationId.HasValue && specializationId.Value > 0)
        {
            query = query.Where(s => s.Doctor.SpecializationId == specializationId.Value);
        }

        if (doctorId.HasValue && doctorId.Value > 0)
        {
            query = query.Where(s => s.DoctorId == doctorId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.SlotDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(s => s.SlotDate <= toDate.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(s => s.Status.ToLower() == status.ToLower());
        }

        var schedules = await query
            .OrderByDescending(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();

        return schedules.Select(MapToScheduleDto);
    }

    public async Task<int> BatchAssignSchedulesAsync(BatchScheduleRequestDto dto)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == dto.DoctorId);
        if (doctor == null)
        {
            throw new InvalidOperationException("Không tìm thấy thông tin Bác sĩ.");
        }

        var newSchedules = new List<Schedule>();
        var existingSchedules = await _context.Schedules
            .Where(s => s.DoctorId == dto.DoctorId && s.SlotDate >= dto.FromDate && s.SlotDate <= dto.ToDate)
            .ToListAsync();

        for (var date = dto.FromDate; date <= dto.ToDate; date = date.AddDays(1))
        {
            foreach (var slot in dto.SelectedSlots)
            {
                bool exists = existingSchedules.Any(s => s.SlotDate == date && s.StartTime == slot.StartTime);
                if (!exists)
                {
                    newSchedules.Add(new Schedule
                    {
                        DoctorId = dto.DoctorId,
                        SlotDate = date,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        Status = ScheduleStatus.Available.ToString(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        if (newSchedules.Any())
        {
            await _context.Schedules.AddRangeAsync(newSchedules);
            await _context.SaveChangesAsync();
        }

        return newSchedules.Count;
    }

    public async Task<bool> DeleteScheduleAsync(int scheduleId)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
        if (schedule == null) return false;

        if (schedule.Status == ScheduleStatus.Booked.ToString())
        {
            throw new InvalidOperationException("Không thể xóa ca trực đã có bệnh nhân đặt lịch.");
        }

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RequestCancelSlotAsync(int scheduleId, int doctorUserId, string reason)
    {
        var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorUserId);
        if (doctor == null) throw new InvalidOperationException("Không tìm thấy thông tin Bác sĩ.");

        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId && s.DoctorId == doctor.DoctorId);
        if (schedule == null) return false;

        if (schedule.Status == ScheduleStatus.Booked.ToString())
        {
            throw new InvalidOperationException("Ca trực đã có bệnh nhân đặt lịch. Không thể xin hủy trực tiếp!");
        }

        schedule.Status = ScheduleStatus.CancelRequested.ToString();
        schedule.CancelReason = reason;
        schedule.CancelRequestedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ScheduleDto>> GetPendingCancelRequestsAsync()
    {
        var schedules = await _context.Schedules
            .Include(s => s.Doctor).ThenInclude(d => d.User)
            .Include(s => s.Doctor).ThenInclude(d => d.Specialization)
            .Where(s => s.Status == ScheduleStatus.CancelRequested.ToString())
            .OrderByDescending(s => s.CancelRequestedAt)
            .ToListAsync();

        return schedules.Select(MapToScheduleDto);
    }

    public async Task<bool> ApproveCancelRequestAsync(int scheduleId)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
        if (schedule == null) return false;

        schedule.Status = ScheduleStatus.Cancelled.ToString();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectCancelRequestAsync(int scheduleId)
    {
        var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
        if (schedule == null) return false;

        schedule.Status = ScheduleStatus.Available.ToString();
        schedule.CancelReason = null;
        schedule.CancelRequestedAt = null;

        await _context.SaveChangesAsync();
        return true;
    }

    private static ScheduleDto MapToScheduleDto(Schedule s)
    {
        return new ScheduleDto
        {
            ScheduleId = s.ScheduleId,
            DoctorId = s.DoctorId,
            DoctorName = s.Doctor?.User?.FullName ?? string.Empty,
            SpecializationName = s.Doctor?.Specialization?.Name ?? string.Empty,
            SlotDate = s.SlotDate,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Status = s.Status,
            CancelReason = s.CancelReason,
            CancelRequestedAt = s.CancelRequestedAt,
            CreatedAt = s.CreatedAt
        };
    }
}
