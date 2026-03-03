using DeviceManagement.API.Controllers;
using DeviceManagement.Application.DTOs;
using DeviceManagement.Application.Services;
using DeviceManagement.Domain;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DeviceManagement.Tests.UnitTests;

public class DevicesControllerTests
{
    private readonly Mock<IDeviceService> _serviceMock = new();

    private DevicesController CreateController() => new(_serviceMock.Object);

    [Fact]
    public async Task GetById_ReturnsOk_WhenDeviceExists()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var dto = new DeviceDTO { Id = id };
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(dto);
        var controller = CreateController();

        // Act
        var result = await controller.GetById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenDeviceNotExists()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((DeviceDTO?)null);
        var controller = CreateController();

        // Act
        var result = await controller.GetById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAll_WithPaging_ReturnsOk_WithPagedResult()
    {
        // Arrange
        var pagedResult = new PagedResult<DeviceDTO> { Items = new[] { new DeviceDTO() }, TotalCount = 1, PageNumber = 1, PageSize = 10 };
        _serviceMock.Setup(s => s.GetAllAsync(1, 10)).ReturnsAsync(pagedResult);
        var controller = CreateController();

        // Act
        var result = await controller.GetAll(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(pagedResult, okResult.Value);
    }

    [Fact]
    public async Task GetByBrand_ReturnsOk_WithDevices()
    {
        // Arrange
        var brand = "Brand";
        var dtos = new[] { new DeviceDTO { Brand = brand } };
        _serviceMock.Setup(s => s.GetByBrandAsync(brand)).ReturnsAsync(dtos);
        var controller = CreateController();

        // Act
        var result = await controller.GetByBrand(brand);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dtos, okResult.Value);
    }

    [Fact]
    public async Task GetByState_ReturnsOk_WithDevices()
    {
        // Arrange
        var state = "Available";
        var parsedState = DeviceState.Available;
        var dtos = new[] { new DeviceDTO { State = parsedState } };
        _serviceMock.Setup(s => s.GetByStateAsync(parsedState)).ReturnsAsync(dtos);
        var controller = CreateController();

        // Act
        var result = await controller.GetByState(state);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(dtos, okResult.Value);
    }

    [Fact]
    public async Task GetByState_ReturnsBadRequest_InvalidState()
    {
        // Arrange
        var state = "Invalid";
        var controller = CreateController();

        // Act
        var result = await controller.GetByState(state);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid state.", badRequestResult.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateDeviceDTO("", "", "");
        var resultDto = new DeviceDTO { Id = Guid.CreateVersion7() };
        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(resultDto);
        var controller = CreateController();

        // Act
        var result = await controller.Create(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal("GetById", createdResult.ActionName);
        Assert.Equal(resultDto, createdResult.Value);
    }

    [Fact]
    public async Task Update_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var dto = new UpdateDeviceDTO("", "", "");
        _serviceMock.Setup(s => s.UpdateAsync(id, dto)).Returns(Task.CompletedTask);
        var controller = CreateController();

        // Act
        var result = await controller.Update(id, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task PartialUpdate_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var dto = new UpdateDeviceDTO("", "", "");
        _serviceMock.Setup(s => s.PartialUpdateAsync(id, dto)).Returns(Task.CompletedTask);
        var controller = CreateController();

        // Act
        var result = await controller.PartialUpdate(id, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        _serviceMock.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);
        var controller = CreateController();

        // Act
        var result = await controller.Delete(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}