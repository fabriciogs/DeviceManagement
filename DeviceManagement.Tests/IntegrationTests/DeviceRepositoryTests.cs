using Dapper;
using DeviceManagement.Domain;
using DeviceManagement.Domain.Entities;
using DeviceManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Connections;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.Common;
using Xunit;

namespace DeviceManagement.Tests.IntegrationTests;

public class DeviceRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DapperDeviceRepository _repository;

    static DeviceRepositoryTests()
    {
        SqlMapper.AddTypeHandler(new GuidAsTextHandler());
    }

    private class GuidAsTextHandler : SqlMapper.TypeHandler<Guid>
    {
        public override Guid Parse(object value)
        {
            return value switch
            {
                string str => Guid.Parse(str),
                _ => throw new InvalidCastException($"Cannot convert {value?.GetType()} to Guid")
            };
        }

        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToString("D");  // "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
        }
    }

    public DeviceRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _repository = new DapperDeviceRepository(_connection);

        // Create table for testing (SQLite for in-memory)
        _connection.Execute(@"
            CREATE TABLE Devices (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Brand TEXT NOT NULL,
                State INTEGER NOT NULL,
                CreationTime TEXT NOT NULL
            )");
    }

    public void Dispose()
    {
        _connection.Close();
    }

    [Fact]
    public async Task AddAndGetById_Works()
    {
        // Arrange
        var device = CreateTestDevice();

        // Act
        await _repository.AddAsync(device);
        var retrieved = await _repository.GetByIdAsync(device.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(device.Name, retrieved.Name);
        Assert.Equal(device.Brand, retrieved.Brand);
        Assert.Equal(device.State, retrieved.State);
    }

    //[Fact]
    //public async Task GetAllPagedAsync_ReturnsPagedDevicesAndCount()
    //{
    //    // Arrange
    //    for (int i = 1; i <= 15; i++)
    //    {
    //        await _repository.AddAsync(CreateTestDevice(name: $"Device{i}"));
    //    }

    //    // Act
    //    var (devices, totalCount) = await _repository.GetAllPagedAsync(2, 5);

    //    // Assert
    //    Assert.Equal(5, devices.Count());
    //    Assert.Equal("Device6", devices.First().Name); // Assuming ORDER BY CreationTime DESC, but since all added sequentially, adjust if needed
    //    Assert.Equal(15, totalCount);
    //}

    [Fact]
    public async Task GetByBrand_ReturnsMatchingDevices()
    {
        // Arrange
        var brand = "TestBrand";
        var device = CreateTestDevice(brand: brand);
        await _repository.AddAsync(device);
        await _repository.AddAsync(CreateTestDevice(brand: "Other"));

        // Act
        var result = await _repository.GetByBrandAsync(brand);

        // Assert
        Assert.Single(result);
        Assert.Equal(brand, result.First().Brand);
    }

    [Fact]
    public async Task GetByState_ReturnsMatchingDevices()
    {
        // Arrange
        var state = DeviceState.InUse;
        var device = CreateTestDevice(state: state);
        await _repository.AddAsync(device);
        await _repository.AddAsync(CreateTestDevice(state: DeviceState.Available));

        // Act
        var result = await _repository.GetByStateAsync(state);

        // Assert
        Assert.Single(result);
        Assert.Equal(state, result.First().State);
    }

    [Fact]
    public async Task Update_ChangesDevice()
    {
        // Arrange
        var device = CreateTestDevice();
        await _repository.AddAsync(device);
        device.Name = "UpdatedName";
        device.State = DeviceState.Inactive;

        // Act
        await _repository.UpdateAsync(device);
        var updated = await _repository.GetByIdAsync(device.Id);

        // Assert
        Assert.Equal("UpdatedName", updated?.Name);
        Assert.Equal(DeviceState.Inactive, updated?.State);
    }

    [Fact]
    public async Task Delete_RemovesDevice()
    {
        // Arrange
        var device = CreateTestDevice();
        await _repository.AddAsync(device);

        // Act
        await _repository.DeleteAsync(device.Id);
        var result = await _repository.GetByIdAsync(device.Id);

        // Assert
        Assert.Null(result);
    }

    private static Device CreateTestDevice(Guid? id = null, string name = "Test", string brand = "Brand", DeviceState state = DeviceState.Available)
    {
        return new Device
        {
            Id = id ?? Guid.CreateVersion7(),
            Name = name,
            Brand = brand,
            State = state,
            CreationTime = DateTime.UtcNow
        };
    }
}