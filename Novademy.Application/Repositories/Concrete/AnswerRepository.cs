using Dapper;
using Novademy.Application.Data.Dapper;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class AnswerRepository : IAnswerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public AnswerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    #region Create
    
    public async Task<Answer> CreateAnswerAsync(Answer answer)
    {
        const string sql = @"
            INSERT INTO Answers (Id, QuestionId, Text, IsCorrect, CreatedAt, UpdatedAt)
            VALUES (@Id, @QuestionId, @Text, @IsCorrect, @CreatedAt, @UpdatedAt)";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, answer);
        
        return answer;
    }
    
    #endregion
}