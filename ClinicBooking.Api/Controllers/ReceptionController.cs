using System.Security.Claims;
using ClinicBooking.Core.DTOs.Appointment;
using ClinicBooking.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")]
public class ReceptionController : ControllerBase
{
    private readonly IReceptionService _receptionService;

    public ReceptionController(IReceptionService receptionService)
    {
        _receptionService = receptionService;
    }

    /// <summary>
    /// Tìm kiếm cuộc hẹn nhanh theo SĐT, Tên Bệnh nhân hoặc Mã cuộc hẹn (Dành cho Lễ tân)
    /// </summary>
    [HttpGet("appointments/search")]
    [ProducesResponseType(typeof(IEnumerable<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(Enumerable.Empty<AppointmentDto>());
        }

        var results = await _receptionService.SearchAppointmentsAsync(query);
        return Ok(results);
    }

    /// <summary>
    /// Đón tiếp Bệnh nhân - Thực hiện Check-in (PATCH /api/appointments/{id}/check-in)
    /// </summary>
    [HttpPatch("/api/appointments/{id:int}/check-in")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckIn(int id)
    {
        int userId = GetCurrentUserId();

        try
        {
            var result = await _receptionService.CheckInAsync(id, userId);
            if (result == null) return NotFound(new { message = "Không tìm thấy cuộc hẹn." });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Đánh dấu Bệnh nhân bỏ hẹn - No-Show (PATCH /api/appointments/{id}/no-show)
    /// </summary>
    [HttpPatch("/api/appointments/{id:int}/no-show")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNoShow(int id)
    {
        int userId = GetCurrentUserId();

        try
        {
            var result = await _receptionService.MarkNoShowAsync(id, userId);
            if (result == null) return NotFound(new { message = "Không tìm thấy cuộc hẹn." });
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
