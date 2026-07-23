using System.Security.Claims;
using ClinicBooking.Core.DTOs.Appointment;
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

    public DoctorDashboardController(IDoctorDashboardService doctorDashboardService)
    {
        _doctorDashboardService = doctorDashboardService;
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
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out int id) ? id : 0;
    }
}
