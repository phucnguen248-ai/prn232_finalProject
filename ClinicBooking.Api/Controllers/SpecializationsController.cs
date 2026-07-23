using ClinicBooking.Core.DTOs.Specialization;
using ClinicBooking.Core.Entities;
using ClinicBooking.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace ClinicBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecializationsController : ControllerBase
{
    private readonly ISpecializationService _specializationService;

    public SpecializationsController(ISpecializationService specializationService)
    {
        _specializationService = specializationService;
    }

    /// <summary>
    /// Lấy danh sách chuyên khoa (Hỗ trợ OData $filter, $orderby, $select, $top, $skip)
    /// </summary>
    [HttpGet]
    [EnableQuery]
    [ProducesResponseType(typeof(IEnumerable<Specialization>), StatusCodes.Status200OK)]
    public IActionResult GetSpecializations()
    {
        return Ok(_specializationService.GetQueryable());
    }

    /// <summary>
    /// Lấy chi tiết chuyên khoa theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SpecializationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var spec = await _specializationService.GetByIdAsync(id);
        if (spec == null) return NotFound(new { message = "Không tìm thấy chuyên khoa." });
        return Ok(spec);
    }

    /// <summary>
    /// Tạo chuyên khoa mới (Quyền Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SpecializationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSpecializationDto dto)
    {
        var result = await _specializationService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.SpecializationId }, result);
    }

    /// <summary>
    /// Cập nhật thông tin chuyên khoa (Quyền Admin)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SpecializationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateSpecializationDto dto)
    {
        var result = await _specializationService.UpdateAsync(id, dto);
        if (result == null) return NotFound(new { message = "Không tìm thấy chuyên khoa." });
        return Ok(result);
    }

    /// <summary>
    /// Xóa chuyên khoa (Quyền Admin)
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _specializationService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy chuyên khoa." });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
