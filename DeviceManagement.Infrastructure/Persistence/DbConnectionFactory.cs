using Microsoft.Data.SqlClient;
using System.Data;

namespace DeviceManagement.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    public IDbConnection CreateConnection();
}

public class SqlServerConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection() => new SqlConnection(connectionString);
}