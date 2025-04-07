using System.Data;

namespace Novademy.Application.Data.Dapper;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}