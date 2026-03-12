using DeviceManagement.Application.DTOs;
using DeviceManagement.Application.Notifications;
using DeviceManagement.Application.Persistence;
using DeviceManagement.Domain;
using DeviceManagement.Domain.Entities;

namespace DeviceManagement.Application.Services;

public class DeviceService(IDeviceRepository repository, NotificationContext notificationContext) : IDeviceService
{
    public async Task<DeviceDTO?> GetByIdAsync(Guid id)
    {
        var device = await repository.GetByIdAsync(id);
        return device == null ? null : MapToDto(device);
    }

    public async Task<PagedResult<DeviceDTO>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Limit to prevent abuse

        var (devices, totalCount) = await repository.GetAllPagedAsync(pageNumber, pageSize);
        var items = devices.Select(MapToDto);

        return new PagedResult<DeviceDTO>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<DeviceDTO>> GetByBrandAsync(string brand)
    {
        var devices = await repository.GetByBrandAsync(brand);
        return devices.Select(MapToDto);
    }

    public async Task<IEnumerable<DeviceDTO>> GetByStateAsync(DeviceState state)
    {
        var devices = await repository.GetByStateAsync(state);
        return devices.Select(MapToDto);
    }

    public async Task<DeviceDTO?> CreateAsync(CreateDeviceDTO dto)
    {
        if (dto.Invalid)
        {
            notificationContext.AddNotifications(dto.ValidationResult);
            return null;
        }

        var device = new Device
        {
            Id = Guid.CreateVersion7(),
            Name = dto.Name,
            Brand = dto.Brand,
            State = Enum.Parse<DeviceState>(dto.State, true),
            CreationTime = DateTime.UtcNow
        };

        await repository.AddAsync(device);
        return MapToDto(device);
    }

    public async Task UpdateAsync(Guid id, UpdateDeviceDTO dto) => await InternalUpdateAsync(id, dto);

    public async Task PartialUpdateAsync(Guid id, UpdateDeviceDTO dto) => await InternalUpdateAsync(id, dto);

    private async Task InternalUpdateAsync(Guid id, UpdateDeviceDTO dto)
    {
        if (dto.Invalid)
        {
            notificationContext.AddNotifications(dto.ValidationResult);
            return;
        }

        var device = await repository.GetByIdAsync(id);

        if (device is null)
        {
            notificationContext.AddNotification("Error", "Device not found.");
            return;
        }

        if (device.State == DeviceState.InUse && (dto.Name != null || dto.Brand != null))
        {
            notificationContext.AddNotifications(dto.ValidationResult);
        }

        if (dto.Name != null) device.Name = dto.Name;
        if (dto.Brand != null) device.Brand = dto.Brand;
        if (dto.State != null) device.State = Enum.Parse<DeviceState>(dto.State, true);
        // CreationTime cannot be updated - ignored

        await repository.UpdateAsync(device);
    }

    public async Task DeleteAsync(Guid id)
    {
        var device = await repository.GetByIdAsync(id);

        if (device is null)
        {
            notificationContext.AddNotification("Error", "Device not found.");
            return;
        }

        if (device.State == DeviceState.InUse)
        {
            notificationContext.AddNotification("Error", "Cannot delete in-use devices.");
            return;
        }

        await repository.DeleteAsync(id);
    }

    private static DeviceDTO MapToDto(Device device) => new()
    {
        Id = device.Id,
        Name = device.Name,
        Brand = device.Brand,
        State = device.State,
        CreationTime = device.CreationTime
    };

    #region IDisposable implementation

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool _isDisposed;
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                repository?.Dispose();
            }
            _isDisposed = true;
        }
    }

    #endregion
}