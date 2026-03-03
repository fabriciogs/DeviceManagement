using DeviceManagement.Application.Validators;

namespace DeviceManagement.Application.DTOs;

public class CreateDeviceDTO : BaseValidationDTO
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string State { get; set; } = "Available"; // Enum as string for API

    public CreateDeviceDTO(string name, string brand, string state)
    {
        Name = name;
        Brand = brand;
        State = state;

        Validate(this, new CreateDeviceValidator());
    }
}