using ClinicBooking.Core.DTOs.Doctor;
using FluentValidation;

namespace ClinicBooking.Core.Validators;

public class CreateDoctorDtoValidator : AbstractValidator<CreateDoctorDto>
{
    public CreateDoctorDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Email không đúng định dạng.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(6).WithMessage("Mật khẩu tối thiểu 6 ký tự.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên bác sĩ không được để trống.");

        RuleFor(x => x.SpecializationId)
            .GreaterThan(0).WithMessage("Vui lòng chọn Chuyên khoa hợp lệ.");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("Số giấp phép hành nghề không được để trống.");
    }
}
