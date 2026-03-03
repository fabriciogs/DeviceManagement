using DeviceManagement.Application.DTOs;
using FluentValidation;

namespace DeviceManagement.Application.Validators;

public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceDTO>
{
    public UpdateDeviceValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name != null);
        RuleFor(x => x.Brand).MaximumLength(100).When(x => x.Brand != null);
        RuleFor(x => x.State).Must(BeValidState).WithMessage("Invalid device state.").When(x => x.State != null);
    }

    private bool BeValidState(string? state) => state == null || Enum.TryParse<Domain.DeviceState>(state, true, out _);
}