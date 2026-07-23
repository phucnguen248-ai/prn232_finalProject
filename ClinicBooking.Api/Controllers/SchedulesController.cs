using ClinicBooking.Core.DTOs.Schedule;
using ClinicBooking.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleBatchService _scheduleBatchService;

    public SchedulesController(IScheduleBatchService scheduleBatchService)
    {
        _scheduleBatchService = scheduleBatchService;
    }

    /// <summary>
    /// Lấy danh sách lịch trực theo bộ lọc (Admin, Staff, Doctor)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff,Doctor")]
    [ProducesResponseType(typeof(IEnumerable<ScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedules(
        [FromQuery] int? specializationId,
        [FromQuery] int? doctorId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string? status)
    {
        var result = await _scheduleBatchService.GetSchedulesAsync(specializationId, doctorId, fromDate, toDate, status);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách yêu cầu xin hủy ca trực đang chờ Admin duyệt (Quyền Admin)
    /// </summary>
    [HttpGet("cancel-requests")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<ScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCancelRequests()
    {
        var requests = await _scheduleBatchService.GetPendingCancelRequestsAsync();
        return Ok(requests);
    }

    /// <summary>
    /// Admin đồng ý duyệt hủy ca trực của Bác sĩ (Quyền Admin)
    /// </summary>
    [HttpPost("{id:int}/approve-cancel")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveCancel(int id)
    {
        bool success = await _scheduleBatchService.ApproveCancelRequestAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy yêu cầu hủy ca trực." });
        return Ok(new { message = "Đã duyệt hủy ca trực thành công." });
    }

    /// <summary>
    /// Admin từ chối yêu cầu hủy ca trực của Bác sĩ (Quyền Admin)
    /// </summary>
    [HttpPost("{id:int}/reject-cancel")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectCancel(int id)
    {
        bool success = await _scheduleBatchService.RejectCancelRequestAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy yêu cầu hủy ca trực." });
        return Ok(new { message = "Đã từ chối yêu cầu hủy ca trực." });
    }

    /// <summary>
    /// Phân công ca trực tự động theo khoảng ngày/ca trực cho Bác sĩ (Quyền Admin)
    /// </summary>
    [HttpPost("batch-assign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BatchAssign([FromBody] BatchScheduleRequestDto dto)
    {
        try
        {
            int createdCount = await _scheduleBatchService.BatchAssignSchedulesAsync(dto);
            return Ok(new { message = $"Đã phân công thành công {createdCount} slot ca trực mới cho Bác sĩ.", count = createdCount });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Xóa ca trực rảnh chưa được đặt lịch (Quyền Admin)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool success = await _scheduleBatchService.DeleteScheduleAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy ca trực." });
            return Ok(new { message = "Xóa ca trực thành công." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
