using ClinicBooking.Core.DTOs.Doctor;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace ClinicBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>
    /// Lấy danh sách Bác sĩ (Hỗ trợ OData $filter, $orderby, $select, $top, $skip, $expand)
    /// </summary>
    [HttpGet]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<Doctor>), StatusCodes.Status200OK)]
    public IActionResult GetDoctors()
    {
        return Ok(_doctorService.GetQueryable());
    }

    /// <summary>
    /// Lấy danh sách Bác sĩ dạng DTO đầy đủ thông tin
    /// </summary>
    [HttpGet("list")]
    [ProducesResponseType(typeof(IEnumerable<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDtos()
    {
        var doctors = await _doctorService.GetAllAsync();
        return Ok(doctors);
    }

    /// <summary>
    /// Lấy chi tiết hồ sơ Bác sĩ theo DoctorId
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var doctor = await _doctorService.GetByIdAsync(id);
        if (doctor == null) return NotFound(new { message = "Không tìm thấy hồ sơ bác sĩ." });
        return Ok(doctor);
    }

    /// <summary>
    /// Tạo tài khoản và hồ sơ Bác sĩ mới (Quyền Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
    {
        try
        {
            var result = await _doctorService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.DoctorId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật thông tin Bác sĩ (Quyền Admin)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorDto dto)
    {
        try
        {
            var result = await _doctorService.UpdateAsync(id, dto);
            if (result == null) return NotFound(new { message = "Không tìm thấy hồ sơ bác sĩ." });
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Vô hiệu hóa tài khoản Bác sĩ (Quyền Admin)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _doctorService.DeleteAsync(id);
        if (!success) return NotFound(new { message = "Không tìm thấy hồ sơ bác sĩ." });
        return NoContent();
    }
}
