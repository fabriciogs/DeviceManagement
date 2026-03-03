using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace DeviceManagement.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    public IDbConnection CreateConnection();
}

[ExcludeFromCodeCoverage]
public class SqlServerConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}