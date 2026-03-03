namespace DeviceManagement.Application.DTOs;

public class CreateDeviceDTO
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string State { get; set; } = "Available"; // Enum as string for API
}