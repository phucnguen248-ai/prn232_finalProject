using ClinicBooking.Core.DTOs.Schedule;

namespace ClinicBooking.Core.Interfaces;

public interface IScheduleBatchService
{
    Task<IEnumerable<ScheduleDto>> GetSchedulesAsync(int? specializationId, int? doctorId, DateOnly? fromDate, DateOnly? toDate, string? status);
    Task<int> BatchAssignSchedulesAsync(BatchScheduleRequestDto dto);
    Task<bool> DeleteScheduleAsync(int scheduleId);

    // Doctor Slot Cancel Request & Admin Approval
    Task<bool> RequestCancelSlotAsync(int scheduleId, int doctorUserId, string reason);
    Task<IEnumerable<ScheduleDto>> GetPendingCancelRequestsAsync();
    Task<bool> ApproveCancelRequestAsync(int scheduleId);
    Task<bool> RejectCancelRequestAsync(int scheduleId);
}
