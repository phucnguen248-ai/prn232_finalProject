using ClinicBooking.Core.DTOs.Staff;
using ClinicBooking.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly IStaffManagementService _staffManagementService;

    public StaffController(IStaffManagementService staffManagementService)
    {
        _staffManagementService = staffManagementService;
    }

    /// <summary>
    /// Lấy danh sách Nhân viên / Lễ tân (Quyền Admin)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StaffDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var staffList = await _staffManagementService.GetAllAsync();
        return Ok(staffList);
    }

    /// <summary>
    /// Tạo tài khoản Nhân viên / Lễ tân mới (Quyền Admin)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StaffDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStaffDto dto)
    {
        try
        {
            var result = await _staffManagementService.CreateStaffAsync(dto);
            return CreatedAtAction(nameof(GetAll), null, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
