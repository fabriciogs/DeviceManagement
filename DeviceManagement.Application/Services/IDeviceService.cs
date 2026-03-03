using DeviceManagement.Application.DTOs;
using DeviceManagement.Domain;

namespace DeviceManagement.Application.Services;

public interface IDeviceService
{
    Task<DeviceDTO?> GetByIdAsync(Guid id);
    Task<PagedResult<DeviceDTO>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<DeviceDTO>> GetByBrandAsync(string brand);
    Task<IEnumerable<DeviceDTO>> GetByStateAsync(DeviceState state);
    Task<DeviceDTO> CreateAsync(CreateDeviceDTO dto);
    Task UpdateAsync(Guid id, UpdateDeviceDTO dto);
    Task PartialUpdateAsync(Guid id, UpdateDeviceDTO dto); // For PATCH-like
    Task DeleteAsync(Guid id);
}