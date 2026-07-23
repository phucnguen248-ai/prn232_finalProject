using ClinicBooking.Core.DTOs.Schedule;
using FluentValidation;

namespace ClinicBooking.Core.Validators;

public class BatchScheduleRequestDtoValidator : AbstractValidator<BatchScheduleRequestDto>
{
    public BatchScheduleRequestDtoValidator()
    {
        RuleFor(x => x.DoctorId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Bác sĩ.");

        RuleFor(x => x.FromDate)
            .NotEmpty().WithMessage("Vui lòng chọn Ngày bắt đầu.");

        RuleFor(x => x.ToDate)
            .NotEmpty().WithMessage("Vui lòng chọn Ngày kết thúc.")
            .GreaterThanOrEqualTo(x => x.FromDate).WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng Ngày bắt đầu.");

        RuleFor(x => x.SelectedSlots)
            .NotEmpty().WithMessage("Vui lòng chọn ít nhất một Slot khung giờ ca trực.");
    }
}
