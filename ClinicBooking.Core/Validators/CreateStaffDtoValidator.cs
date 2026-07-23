using ClinicBooking.Core.DTOs.Staff;
using FluentValidation;

namespace ClinicBooking.Core.Validators;

public class CreateStaffDtoValidator : AbstractValidator<CreateStaffDto>
{
    public CreateStaffDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ và tên không được để trống.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .Matches(@"^[0-9]{10,11}$").WithMessage("Số điện thoại phải từ 10-11 chữ số.");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Chức vụ không được để trống.");
    }
}
