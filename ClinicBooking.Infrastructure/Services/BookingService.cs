using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ClinicBooking.Core.DTOs;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Enums;
using ClinicBooking.Core.Interfaces;
using ClinicBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _dbContext;

        public BookingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ScheduleSlotDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date)
        {
            var localNow = DateTime.Now;
            var today = DateOnly.FromDateTime(localNow);

            if (date < today)
            {
                return Enumerable.Empty<ScheduleSlotDto>();
            }

            var query = _dbContext.Schedules
                .Where(s => s.DoctorId == doctorId
                    && s.SlotDate == date
                    && s.Status == ScheduleStatus.Available.ToString());

            if (date == today)
            {
                var currentTime = TimeOnly.FromDateTime(localNow);
                query = query.Where(s => s.StartTime > currentTime);
            }

            var slots = await query
                .OrderBy(s => s.StartTime)
                .Select(s => new ScheduleSlotDto
                {
                    ScheduleId = s.ScheduleId,
                    DoctorId = s.DoctorId,
                    SlotDate = s.SlotDate,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Status = s.Status
                })
                .ToListAsync();

            return slots;
        }

        public async Task<BookingResult> BookAppointmentAsync(CreateBookingDto dto, int currentUserId)
        {
            // 1. Tìm hoặc tạo bản ghi Patient tương ứng với UserId
            var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.UserId == currentUserId);
            if (patient == null)
            {
                patient = new Patient
                {
                    UserId = currentUserId,
                    DateOfBirth = dto.PatientDob.HasValue ? DateOnly.FromDateTime(dto.PatientDob.Value) : new DateOnly(2000, 1, 1),
                    Gender = "N/A",
                    Address = "N/A"
                };
                _dbContext.Patients.Add(patient);
                await _dbContext.SaveChangesAsync();
            }

            // 2. Mở Atomic Database Transaction với IsolationLevel.Serializable để chống race condition trùng slot
            using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var schedule = await _dbContext.Schedules
                    .FirstOrDefaultAsync(s => s.ScheduleId == dto.ScheduleId && s.Status == ScheduleStatus.Available.ToString());

                if (schedule == null)
                {
                    await transaction.RollbackAsync();
                    return new BookingResult
                    {
                        Success = false,
                        IsConflict = true,
                        Message = "Khung giờ ca khám này vừa được bệnh nhân khác đặt thành công. Vui lòng chọn slot khác."
                    };
                }

                // Chuyển Slot sang Booked
                if (schedule.DoctorId != dto.DoctorId)
                {
                    await transaction.RollbackAsync();
                    return new BookingResult
                    {
                        Success = false,
                        Message = "The selected slot does not belong to the selected doctor. Please reload the available slots."
                    };
                }

                var slotStartDateTime = schedule.SlotDate.ToDateTime(schedule.StartTime);
                if (slotStartDateTime <= DateTime.Now)
                {
                    await transaction.RollbackAsync();
                    return new BookingResult
                    {
                        Success = false,
                        Message = "Khung giờ khám này đã bắt đầu hoặc đã qua. Vui lòng chọn khung giờ khác."
                    };
                }

                schedule.Status = ScheduleStatus.Booked.ToString();

                // Kiểm tra xem đã có bản ghi Appointment cũ nào cho ScheduleId này bị Hủy chưa (Tái sử dụng bản ghi tránh vi phạm Unique Index)
                var existingAppointment = await _dbContext.Appointments
                    .FirstOrDefaultAsync(a => a.ScheduleId == dto.ScheduleId);

                int appointmentId;

                if (existingAppointment != null)
                {
                    existingAppointment.PatientId = patient.PatientId;
                    existingAppointment.DoctorId = schedule.DoctorId;
                    existingAppointment.Reason = dto.Reason;
                    existingAppointment.Status = AppointmentStatus.Confirmed.ToString();
                    existingAppointment.CreatedAt = DateTime.UtcNow;
                    existingAppointment.CheckedInAt = null;
                    existingAppointment.CompletedAt = null;
                    existingAppointment.CancelledAt = null;
                    existingAppointment.Notes = null;
                    appointmentId = existingAppointment.AppointmentId;
                }
                else
                {
                    var newAppointment = new Appointment
                    {
                        PatientId = patient.PatientId,
                        DoctorId = schedule.DoctorId,
                        ScheduleId = dto.ScheduleId,
                        Reason = dto.Reason,
                        Status = AppointmentStatus.Confirmed.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };
                    _dbContext.Appointments.Add(newAppointment);
                    await _dbContext.SaveChangesAsync();
                    appointmentId = newAppointment.AppointmentId;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return new BookingResult
                {
                    Success = true,
                    AppointmentId = appointmentId,
                    Message = "Đặt lịch hẹn khám bệnh thành công!"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new BookingResult
                {
                    Success = false,
                    Message = $"Đã xảy ra lỗi khi đặt lịch: {ex.Message}"
                };
            }
        }

        public async Task<CancellationResult> CancelAppointmentAsync(int appointmentId, int currentUserId)
        {
            var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.UserId == currentUserId);
            if (patient == null)
            {
                return new CancellationResult
                {
                    Success = false,
                    IsBadRequest = true,
                    Message = "Không tìm thấy hồ sơ bệnh nhân cho tài khoản này."
                };
            }

            var appointment = await _dbContext.Appointments
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.PatientId == patient.PatientId);

            if (appointment == null)
            {
                return new CancellationResult
                {
                    Success = false,
                    IsBadRequest = true,
                    Message = "Không tìm thấy thông tin cuộc hẹn hoặc bạn không có quyền hủy cuộc hẹn này."
                };
            }

            if (appointment.Status == AppointmentStatus.Cancelled.ToString())
            {
                return new CancellationResult
                {
                    Success = false,
                    IsBadRequest = true,
                    Message = "Cuộc hẹn này đã ở trạng thái Hủy từ trước."
                };
            }

            // KIỂM TRA QUY TẮC HỦY LỊCH MỀM DẺO (GRACE PERIOD RULE)
            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;
            
            // Điều kiện 2: Hủy trong vòng 15 phút sau khi đặt thành công (Grace Period)
            var isGracePeriod = (utcNow - appointment.CreatedAt).TotalMinutes <= 15;

            // Điều kiện 1: Hủy trước giờ khám >= 2 tiếng
            var slotStartDateTime = appointment.Schedule.SlotDate.ToDateTime(appointment.Schedule.StartTime);
            var isTwoHoursBefore = (slotStartDateTime - localNow).TotalHours >= 2;

            if (!isGracePeriod && !isTwoHoursBefore)
            {
                return new CancellationResult
                {
                    Success = false,
                    IsBadRequest = true,
                    Message = "Không thể hủy lịch hẹn: Đã vượt quá thời gian ân hạn 15 phút (Grace Period) và ca khám còn ít hơn 2 tiếng nữa là bắt đầu."
                };
            }

            // Thực hiện Hủy lịch & Mở lại Slot ca khám
            appointment.Status = AppointmentStatus.Cancelled.ToString();
            appointment.CancelledAt = utcNow;

            if (appointment.Schedule != null)
            {
                appointment.Schedule.Status = ScheduleStatus.Available.ToString();
            }

            await _dbContext.SaveChangesAsync();

            return new CancellationResult
            {
                Success = true,
                Message = "Hủy lịch hẹn khám thành công. Khung giờ ca khám đã được hoàn trả về trạng thái rảnh cho bệnh nhân khác đăng ký."
            };
        }

        public async Task<IEnumerable<AppointmentHistoryDto>> GetPatientHistoryAsync(int currentUserId)
        {
            var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.UserId == currentUserId);
            if (patient == null)
            {
                return new List<AppointmentHistoryDto>();
            }

            var utcNow = DateTime.UtcNow;
            var localNow = DateTime.Now;

            var appointments = await _dbContext.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.Schedule)
                .Where(a => a.PatientId == patient.PatientId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var result = appointments.Select(a =>
            {
                var isGracePeriod = (utcNow - a.CreatedAt).TotalMinutes <= 15;
                var slotStartDateTime = a.Schedule.SlotDate.ToDateTime(a.Schedule.StartTime);
                var isTwoHoursBefore = (slotStartDateTime - localNow).TotalHours >= 2;
                var canCancel = a.Status == AppointmentStatus.Confirmed.ToString() && (isGracePeriod || isTwoHoursBefore);

                return new AppointmentHistoryDto
                {
                    AppointmentId = a.AppointmentId,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor?.User?.FullName ?? "N/A",
                    SpecializationName = a.Doctor?.Specialization?.Name ?? "Chung",
                    SlotDate = a.Schedule.SlotDate,
                    StartTime = a.Schedule.StartTime,
                    EndTime = a.Schedule.EndTime,
                    Reason = a.Reason,
                    Status = a.Status,
                    CreatedAt = a.CreatedAt,
                    CanCancel = canCancel
                };
            });

            return result;
        }
    }
}
