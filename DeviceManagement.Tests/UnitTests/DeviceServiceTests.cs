using DeviceManagement.Application.DTOs;
using DeviceManagement.Application.Notifications;
using DeviceManagement.Application.Persistence;
using DeviceManagement.Application.Services;
using DeviceManagement.Domain;
using DeviceManagement.Domain.Entities;
using Moq;
using Xunit;

namespace DeviceManagement.Tests.UnitTests;

public class DeviceServiceTests
{
    private readonly Mock<IDeviceRepository> _repoMock = new();

    private DeviceService CreateService()
    {
        var notificationContextMock = new NotificationContext();
        return new DeviceService(_repoMock.Object, notificationContextMock);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesDevice()
    {
        // Arrange
        var dto = new CreateDeviceDTO("Test", "Brand", "Available");
        var service = CreateService();

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Brand, result.Brand);
        Assert.Equal(DeviceState.Available, result.State);

        _repoMock.Verify(r => r.AddAsync(It.Is<Device>(d =>
            d.Name == dto.Name &&
            d.Brand == dto.Brand &&
            d.State == DeviceState.Available &&
            d.Id != Guid.Empty &&
            d.CreationTime != default
        )), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenDeviceExists()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var device = new Device { Id = id, Name = "Test" };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(device);
        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(device.Name, result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenDeviceNotFound()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Device?)null);
        var service = CreateService();

        // Act
        var result = await service.GetByIdAsync(id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithPaging_ReturnsPagedResult()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 5;
        var devices = Enumerable.Range(1, 10).Select(i => new Device { Id = Guid.CreateVersion7() });
        var totalCount = 20;
        _repoMock.Setup(r => r.GetAllPagedAsync(pageNumber, pageSize)).ReturnsAsync((devices, totalCount));
        var service = CreateService();

        // Act
        var result = await service.GetAllAsync(pageNumber, pageSize);

        // Assert
        Assert.Equal(10, result.Items.Count()); // Since mock returns 10, but in real it would be <= pageSize
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
    }

    [Fact]
    public async Task GetAllAsync_InvalidPageNumber_UsesDefault()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllPagedAsync(1, 10)).ReturnsAsync(([], 0));
        var service = CreateService();

        // Act
        var result = await service.GetAllAsync(0, 10);

        // Assert
        Assert.Equal(1, result.PageNumber);
    }

    [Fact]
    public async Task GetAllAsync_PageSizeTooLarge_LimitsTo100()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllPagedAsync(1, 100)).ReturnsAsync(([], 0));
        var service = CreateService();

        // Act
        var result = await service.GetAllAsync(1, 200);

        // Assert
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task GetByBrandAsync_ReturnsMatchingDtos()
    {
        // Arrange
        var brand = "Brand";
        var devices = new[] { new Device { Brand = brand } };
        _repoMock.Setup(r => r.GetByBrandAsync(brand)).ReturnsAsync(devices);
        var service = CreateService();

        // Act
        var result = await service.GetByBrandAsync(brand);

        // Assert
        Assert.Single(result);
        Assert.Equal(brand, result.First().Brand);
    }

    [Fact]
    public async Task GetByStateAsync_ReturnsMatchingDtos()
    {
        // Arrange
        var state = DeviceState.InUse;
        var devices = new[] { new Device { State = state } };
        _repoMock.Setup(r => r.GetByStateAsync(state)).ReturnsAsync(devices);
        var service = CreateService();

        // Act
        var result = await service.GetByStateAsync(state);

        // Assert
        Assert.Single(result);
        Assert.Equal(state, result.First().State);
    }

    [Fact]
    public async Task UpdateAsync_ValidDto_UpdatesDevice()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var dto = new UpdateDeviceDTO("NewName", "NewBrand", "Inactive");
        var device = new Device { Id = id, State = DeviceState.Available };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(device);
        var service = CreateService();

        // Act
        await service.UpdateAsync(id, dto);

        // Assert
        _repoMock.Verify(r => r.UpdateAsync(It.Is<Device>(d => d.Name == "NewName" && d.State == DeviceState.Inactive)), Times.Once);
    }

    //[Fact]
    //public async Task UpdateAsync_InUseDevice_ThrowsIfNameOrBrandChanged()
    //{
    //    // Arrange
    //    var id = Guid.CreateVersion7();
    //    var dto = new UpdateDeviceDTO("NewName", "NewBrand", "Inactive");
    //    var device = new Device { Id = id, State = DeviceState.InUse };
    //    _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(device);
    //    var service = CreateService();

    //    // Act & Assert
    //    await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(id, dto));
    //}

    //[Fact]
    //public async Task UpdateAsync_DeviceNotFound_Throws()
    //{
    //    // Arrange
    //    var id = Guid.CreateVersion7();
    //    var dto = new UpdateDeviceDTO("", "", "");
    //    _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Device?)null);
    //    var service = CreateService();

    //    // Act & Assert
    //    await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(id, dto));
    //}

    //[Fact]
    //public async Task PartialUpdateAsync_UpdatesOnlyProvidedFields()
    //{
    //    // Arrange
    //    var id = Guid.CreateVersion7();
    //    var dto = new UpdateDeviceDTO("", "", "Inactive");
    //    var device = new Device { Id = id, Name = "OldName", State = DeviceState.Available };
    //    _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(device);
    //    var service = CreateService();

    //    // Act
    //    await service.PartialUpdateAsync(id, dto);

    //    // Assert
    //    _repoMock.Verify(r => r.UpdateAsync(It.Is<Device>(d => d.Name == "OldName" && d.State == DeviceState.Inactive)), Times.Once);
    //}

    [Fact]
    public async Task DeleteAsync_Deletes_WhenNotInUse()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var device = new Device { Id = id, State = DeviceState.Available };
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(device);
        var service = CreateService();

        // Act
        await service.DeleteAsync(id);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    //[Fact]
    //public async Task DeleteAsync_InUseDevice_Throws()
    //{
    //    // Arrange
    //    var id = Guid.CreateVersion7();
    //    var device = new Device { Id = id, State = DeviceState.InUse };
    //    _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(device);
    //    var service = CreateService();

    //    // Act & Assert
    //    await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(id));
    //}

    //[Fact]
    //public async Task DeleteAsync_DeviceNotFound_Throws()
    //{
    //    // Arrange
    //    var id = Guid.CreateVersion7();
    //    _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Device?)null);
    //    var service = CreateService();

    //    // Act & Assert
    //    await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(id));
    //}
}