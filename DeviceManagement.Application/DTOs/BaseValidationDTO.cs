using FluentValidation;
using FluentValidation.Results;
using System.Text.Json.Serialization;

namespace DeviceManagement.Application.DTOs;

public abstract class BaseValidationDTO
{
    [JsonIgnore]
    public bool Valid { get; private set; }

    [JsonIgnore]
    public bool Invalid => !Valid;

    [JsonIgnore]
    public ValidationResult ValidationResult { get; private set; } = new ValidationResult();

    public bool Validate<TModel>(TModel model, AbstractValidator<TModel> validator)
    {
        ValidationResult = validator.Validate(model);
        return Valid = ValidationResult.IsValid;
    }
}