using Dapper;
using Novademy.Application.Data.Dapper;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class QuestionRepository : IQuestionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public QuestionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    #region Create
    
    public async Task<Question> CreateQuestionAsync(Question question)
    {
        const string sql = @"
            INSERT INTO Questions (Id, QuizId, Text, CreatedAt, UpdatedAt)
            VALUES (@Id, @QuizId, @Text, @CreatedAt, @UpdatedAt)";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, question);
        
        return question;
    }
    
    #endregion
    
    #region Read
    
    public async Task<Question?> GetQuestionByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT q.*, a.*
            FROM Questions q
            LEFT JOIN Answers a ON q.Id = a.QuestionId
            WHERE q.Id = @Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var questionDictionary = new Dictionary<Guid, Question>();
        var result = await connection.QueryAsync<Question, Answer, Question>(
            sql,
            (question, answer) =>
            {
                if (!questionDictionary.TryGetValue(question.Id, out var questionEntry))
                {
                    questionEntry = question;
                    questionEntry.Answers = new List<Answer>();
                    questionDictionary.Add(questionEntry.Id, questionEntry);
                }
                
                if (answer != null)
                {
                    questionEntry.Answers.Add(answer);
                }
                
                return questionEntry;
            },
            new { Id = id },
            splitOn: "Id"
        );
        
        var question = result.FirstOrDefault();
        if (question == null)
        {
            throw new KeyNotFoundException("Invalid Question ID.");
        }
        
        return question;
    }
    
    #endregion
}