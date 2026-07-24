using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ClinicBooking.Core.DTOs;
using ClinicBooking.Core.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ClinicBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IValidator<CreateBookingDto> _validator;

        public AppointmentsController(IBookingService bookingService, IValidator<CreateBookingDto> validator)
        {
            _bookingService = bookingService;
            _validator = validator;
        }

        private int GetCurrentUserId()
        {
            // Trích xuất UserId từ JWT Claim
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
            if (int.TryParse(claim, out var userId))
            {
                return userId;
            }

            // Fallback: đọc header X-User-Id nếu dev/test chưa qua Auth middleware
            if (Request.Headers.TryGetValue("X-User-Id", out var headerVal) && int.TryParse(headerVal, out var headerUserId))
            {
                return headerUserId;
            }

            // Mặc định Patient UserId = 4 (như seed data) để dev test nhanh
            return 4;
        }

        /// <summary>
        /// API Đặt lịch khám bệnh (Atomic DB Transaction chống trùng slot)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BookAppointment([FromBody] CreateBookingDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { Message = "Dữ liệu đầu vào không hợp lệ.", Errors = validationResult.Errors });
            }

            var currentUserId = GetCurrentUserId();
            var result = await _bookingService.BookAppointmentAsync(dto, currentUserId);

            if (result.IsConflict)
            {
                return Conflict(new { Message = result.Message });
            }

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }

            return CreatedAtAction(nameof(GetHistory), new { id = result.AppointmentId }, new
            {
                Message = result.Message,
                AppointmentId = result.AppointmentId
            });
        }

        /// <summary>
        /// API Hủy lịch khám bệnh (Áp dụng Grace Period 15 phút & Luật trước 2h)
        /// </summary>
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "Mã cuộc hẹn không hợp lệ." });
            }

            var currentUserId = GetCurrentUserId();
            var result = await _bookingService.CancelAppointmentAsync(id, currentUserId);

            if (result.IsBadRequest)
            {
                return BadRequest(new { Message = result.Message });
            }

            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }

            return Ok(new { Message = result.Message });
        }

        /// <summary>
        /// API Lấy lịch sử hẹn khám của bệnh nhân hiện tại
        /// </summary>
        [HttpGet("my-history")]
        public async Task<IActionResult> GetHistory()
        {
            var currentUserId = GetCurrentUserId();
            var history = await _bookingService.GetPatientHistoryAsync(currentUserId);
            return Ok(history);
        }
    }
}
