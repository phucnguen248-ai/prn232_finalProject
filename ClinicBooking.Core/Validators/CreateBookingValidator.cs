using FluentValidation;
using ClinicBooking.Core.DTOs;

namespace ClinicBooking.Core.Validators
{
    public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingValidator()
        {
            RuleFor(x => x.DoctorId)
                .GreaterThan(0)
                .WithMessage("Bác sĩ không hợp lệ.");

            RuleFor(x => x.ScheduleId)
                .GreaterThan(0)
                .WithMessage("Ca khám không hợp lệ.");

            RuleFor(x => x.PatientName)
                .NotEmpty()
                .WithMessage("Họ tên bệnh nhân không được để trống.")
                .MaximumLength(200)
                .WithMessage("Họ tên không vượt quá 200 ký tự.");

            RuleFor(x => x.PatientPhone)
                .NotEmpty()
                .WithMessage("Số điện thoại không được để trống.")
                .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$")
                .WithMessage("Số điện thoại không đúng định dạng Việt Nam (10 số, bắt đầu bằng 03, 05, 07, 08, 09).");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Lý do khám không được để trống.")
                .Length(10, 500)
                .WithMessage("Lý do khám phải từ 10 đến 500 ký tự.");
        }
    }
}
