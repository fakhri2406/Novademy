using Dapper;
using Novademy.Application.Data.Dapper;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class QuizRepository : IQuizRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public QuizRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    #region Create
    
    public async Task<Quiz> CreateQuizAsync(Quiz quiz)
    {
        const string sql = @"
            INSERT INTO Quizzes (Id, LessonId, Title, Description, CreatedAt, UpdatedAt)
            VALUES (@Id, @LessonId, @Title, @Description, @CreatedAt, @UpdatedAt)";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, quiz);
        
        return quiz;
    }
    
    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Quiz>> GetQuizzesByLessonIdAsync(Guid lessonId)
    {
        const string sql = @"
            SELECT q.*, qu.*, a.*
            FROM Quizzes q
            LEFT JOIN Questions qu ON q.Id = qu.QuizId
            LEFT JOIN Answers a ON qu.Id = a.QuestionId
            WHERE q.LessonId = @LessonId";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var quizDictionary = new Dictionary<Guid, Quiz>();
        var result = await connection.QueryAsync<Quiz, Question, Answer, Quiz>(
            sql,
            (quiz, question, answer) =>
            {
                if (!quizDictionary.TryGetValue(quiz.Id, out var quizEntry))
                {
                    quizEntry = quiz;
                    quizEntry.Questions = new List<Question>();
                    quizDictionary.Add(quizEntry.Id, quizEntry);
                }
                
                if (question != null)
                {
                    var existingQuestion = quizEntry.Questions.FirstOrDefault(q => q.Id == question.Id);
                    if (existingQuestion == null)
                    {
                        question.Answers = new List<Answer>();
                        quizEntry.Questions.Add(question);
                        existingQuestion = question;
                    }
                    
                    if (answer != null)
                    {
                        existingQuestion.Answers.Add(answer);
                    }
                }
                
                return quizEntry;
            },
            new { LessonId = lessonId },
            splitOn: "Id,Id"
        );
        
        return quizDictionary.Values;
    }
    
    public async Task<Quiz?> GetQuizByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT q.*, qu.*, a.*
            FROM Quizzes q
            LEFT JOIN Questions qu ON q.Id = qu.QuizId
            LEFT JOIN Answers a ON qu.Id = a.QuestionId
            WHERE q.Id = @Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var quizDictionary = new Dictionary<Guid, Quiz>();
        var result = await connection.QueryAsync<Quiz, Question, Answer, Quiz>(
            sql,
            (quiz, question, answer) =>
            {
                if (!quizDictionary.TryGetValue(quiz.Id, out var quizEntry))
                {
                    quizEntry = quiz;
                    quizEntry.Questions = new List<Question>();
                    quizDictionary.Add(quizEntry.Id, quizEntry);
                }
                
                if (question != null)
                {
                    var existingQuestion = quizEntry.Questions.FirstOrDefault(q => q.Id == question.Id);
                    if (existingQuestion == null)
                    {
                        question.Answers = new List<Answer>();
                        quizEntry.Questions.Add(question);
                        existingQuestion = question;
                    }
                    
                    if (answer != null)
                    {
                        existingQuestion.Answers.Add(answer);
                    }
                }
                
                return quizEntry;
            },
            new { Id = id },
            splitOn: "Id,Id"
        );
        
        var quiz = result.FirstOrDefault();
        if (quiz == null)
        {
            throw new KeyNotFoundException("Invalid Quiz ID.");
        }
        
        return quiz;
    }
    
    #endregion
    
    #region Update
    
    public async Task<Quiz?> UpdateQuizAsync(Quiz quiz)
    {
        const string sql = @"
            UPDATE Quizzes 
            SET LessonId = @LessonId,
                Title = @Title,
                Description = @Description,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, quiz);
        
        return quiz;
    }
    
    #endregion
    
    #region Delete
    
    public async Task DeleteQuizAsync(Guid id)
    {
        const string sql = "DELETE FROM Quizzes WHERE Id = @Id";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException("Invalid Quiz ID.");
        }
    }
    
    #endregion
}