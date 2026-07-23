using ClinicBooking.Core.DTOs.Specialization;
using FluentValidation;

namespace ClinicBooking.Core.Validators;

public class CreateSpecializationDtoValidator : AbstractValidator<CreateSpecializationDto>
{
    public CreateSpecializationDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên chuyên khoa không được để trống.")
            .MaximumLength(100).WithMessage("Tên chuyên khoa tối đa 100 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả tối đa 500 ký tự.");
    }
}
