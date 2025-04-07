using System.Data;
using Microsoft.Data.SqlClient;

namespace Novademy.Application.Data.Dapper;

public class MsSqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;
    public MsSqlConnectionFactory(string connectionString) => _connectionString = connectionString;
    
    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}