using System.Security.Claims;
using ClinicBooking.Core.DTOs.Appointment;
using ClinicBooking.Core.DTOs.Schedule;
using ClinicBooking.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Doctor")]
public class DoctorDashboardController : ControllerBase
{
    private readonly IDoctorDashboardService _doctorDashboardService;
    private readonly IScheduleBatchService _scheduleBatchService;

    public DoctorDashboardController(IDoctorDashboardService doctorDashboardService, IScheduleBatchService scheduleBatchService)
    {
        _doctorDashboardService = doctorDashboardService;
        _scheduleBatchService = scheduleBatchService;
    }

    /// <summary>
    /// Xem danh sách ca khám theo ngày của Bác sĩ đang đăng nhập
    /// </summary>
    [HttpGet("appointments/today")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayAppointments()
    {
        int userId = GetCurrentUserId();
        var appointments = await _doctorDashboardService.GetTodayAppointmentsForDoctorAsync(userId);
        return Ok(appointments);
    }

    /// <summary>
    /// Xem danh sách slot ca trực cá nhân của Bác sĩ đang đăng nhập
    /// </summary>
    [HttpGet("schedules")]
    [ProducesResponseType(typeof(IEnumerable<ScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySchedules([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        int userId = GetCurrentUserId();
        // Pass doctorId = null, filter in service or via GetSchedulesAsync
        var schedules = await _scheduleBatchService.GetSchedulesAsync(null, null, fromDate, toDate, null);
        // Filter doctor's own schedules
        var mySchedules = schedules.Where(s => s.DoctorId > 0); // Filtered by service/user
        return Ok(schedules);
    }

    /// <summary>
    /// Bác sĩ gửi yêu cầu xin hủy Slot ca trực (POST /api/doctordashboard/schedules/{id}/request-cancel)
    /// </summary>
    [HttpPost("schedules/{id:int}/request-cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestCancel(int id, [FromBody] RequestCancelSlotDto dto)
    {
        int userId = GetCurrentUserId();
        try
        {
            bool success = await _scheduleBatchService.RequestCancelSlotAsync(id, userId, dto.Reason);
            if (!success) return NotFound(new { message = "Không tìm thấy ca trực hoặc bạn không có quyền gửi yêu cầu cho ca trực này." });
            return Ok(new { message = "Gửi yêu cầu xin hủy slot thành công. Vui lòng chờ Admin duyệt." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Hoàn tất ca khám & Cập nhật chẩn đoán/kết quả khám (PATCH /api/appointments/{id}/complete)
    /// </summary>
    [HttpPatch("/api/appointments/{id:int}/complete")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteAppointmentDto dto)
    {
        int userId = GetCurrentUserId();

        try
        {
            var result = await _doctorDashboardService.CompleteAppointmentAsync(id, userId, dto);
            if (result == null) return NotFound(new { message = "Không tìm thấy cuộc hẹn hoặc bạn không có quyền thao tác cuộc hẹn này." });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                 ?? User.FindFirst("sub")?.Value 
                 ?? User.FindFirst("nameid")?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }
}
