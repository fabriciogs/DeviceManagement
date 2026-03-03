using Dapper;
using DeviceManagement.Application.Persistence;
using DeviceManagement.Domain;
using DeviceManagement.Domain.Entities;
using System.Data;

namespace DeviceManagement.Infrastructure.Persistence;

public class DapperDeviceRepository(IDbConnection connection) : IDeviceRepository
{
    public async Task<Device?> GetByIdAsync(Guid id)
    {
        return await connection.QuerySingleOrDefaultAsync<Device>("SELECT * FROM Devices WHERE Id = @Id", new { Id = id });
    }

    public async Task<(IEnumerable<Device> Devices, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize)
    {
        var offset = (pageNumber - 1) * pageSize;

        var dataSql = @"
            SELECT Id, Name, Brand, State, CreationTime
            FROM Devices
            ORDER BY CreationTime DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var countSql = "SELECT COUNT(1) FROM Devices";

        var devices = await connection.QueryAsync<Device>(dataSql, new { Offset = offset, PageSize = pageSize });
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

        return (devices, totalCount);
    }

    public async Task<IEnumerable<Device>> GetByBrandAsync(string brand)
    {
        return await connection.QueryAsync<Device>("SELECT * FROM Devices WHERE Brand = @Brand", new { Brand = brand });
    }

    public async Task<IEnumerable<Device>> GetByStateAsync(DeviceState state)
    {
        return await connection.QueryAsync<Device>("SELECT * FROM Devices WHERE State = @State", new { State = (int)state });
    }

    public async Task AddAsync(Device device)
    {
        await connection.ExecuteAsync(
            @"INSERT INTO Devices (Id, Name, Brand, State, CreationTime)
              VALUES (@Id, @Name, @Brand, @State, @CreationTime)", device);
    }

    public async Task UpdateAsync(Device device)
    {
        await connection.ExecuteAsync(@"UPDATE Devices SET Name = @Name, Brand = @Brand, State = @State WHERE Id = @Id", device);
    }

    public async Task DeleteAsync(Guid id)
    {
        await connection.ExecuteAsync("DELETE FROM Devices WHERE Id = @Id", new { Id = id });
    }
}