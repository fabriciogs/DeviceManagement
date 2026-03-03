using DeviceManagement.Domain;
using DeviceManagement.Domain.Entities;

namespace DeviceManagement.Application.Persistence;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id);
    Task<(IEnumerable<Device> Devices, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<Device>> GetByBrandAsync(string brand);
    Task<IEnumerable<Device>> GetByStateAsync(DeviceState state);
    Task AddAsync(Device device);
    Task UpdateAsync(Device device);
    Task DeleteAsync(Guid id);
}