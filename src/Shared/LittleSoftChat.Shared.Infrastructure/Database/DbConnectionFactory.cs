using System.Data;
using MySql.Data.MySqlClient;

namespace LittleSoftChat.Shared.Infrastructure.Database;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    IDbConnection CreateConnection(string connectionString);
}

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    public IDbConnection CreateConnection(string connectionString)
    {
        return new MySqlConnection(connectionString);
    }
}
