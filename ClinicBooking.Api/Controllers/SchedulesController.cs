using System;
using System.Threading.Tasks;
using ClinicBooking.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulesController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public SchedulesController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Lấy danh sách các slot ca khám rảnh của bác sĩ theo ngày
        /// </summary>
        [HttpGet("available-slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] int doctorId, [FromQuery] string date)
        {
            if (doctorId <= 0)
            {
                return BadRequest("Mã Bác sĩ không hợp lệ.");
            }

            if (!DateOnly.TryParse(date, out var slotDate))
            {
                return BadRequest("Định dạng ngày không hợp lệ (Định dạng đúng: YYYY-MM-DD).");
            }

            var slots = await _bookingService.GetAvailableSlotsAsync(doctorId, slotDate);
            return Ok(slots);
        }
    }
}
