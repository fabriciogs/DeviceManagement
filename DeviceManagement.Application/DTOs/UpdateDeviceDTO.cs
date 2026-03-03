using DeviceManagement.Application.Validators;

namespace DeviceManagement.Application.DTOs;

public class UpdateDeviceDTO : BaseValidationDTO
{
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? State { get; set; }

    public UpdateDeviceDTO(string name, string brand, string state)
    {
        Name = name;
        Brand = brand;
        State = state;

        Validate(this, new UpdateDeviceValidator());
    }
}