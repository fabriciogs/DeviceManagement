using DeviceManagement.Application.DTOs;
using FluentValidation;

namespace DeviceManagement.Application.Validators;

public class CreateDeviceValidator : AbstractValidator<CreateDeviceDTO>
{
    public CreateDeviceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).Must(BeValidState).WithMessage("Invalid device state.");
    }

    private bool BeValidState(string state) => Enum.TryParse<Domain.DeviceState>(state, true, out _);
}