using Dapper;
using Microsoft.AspNetCore.Http;
using Novademy.Application.Cloudinary;
using Novademy.Application.Data.Dapper;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class LessonRepository : ILessonRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IMediaUpload _mediaUpload;
    
    public LessonRepository(IDbConnectionFactory connectionFactory, IMediaUpload mediaUpload)
    {
        _connectionFactory = connectionFactory;
        _mediaUpload = mediaUpload;
    }
    
    #region Create
    
    public async Task<Lesson> CreateLessonAsync(Lesson lesson, IFormFile video, IFormFile? image)
    {
        var videoUploadResult = await _mediaUpload.UploadVideoAsync(video, "lesson_videos");
        lesson.VideoUrl = videoUploadResult.SecureUrl.ToString();
        
        if (image is not null)
        {
            var imageUploadResult = await _mediaUpload.UploadImageAsync(image, "lesson_images");
            lesson.ImageUrl = imageUploadResult.SecureUrl.ToString();
        }
        
        const string sql = @"
            INSERT INTO Lessons (Id, Title, Description, 
            VideoUrl, Order, IsFree, Transcript, 
            ImageUrl, CreatedAt, UpdatedAt, CourseId)
            VALUES (@Id, @Title, @Description, 
            @VideoUrl, @Order, @IsFree, @Transcript, 
            @ImageUrl, @CreatedAt, @UpdatedAt, @CourseId)";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, lesson);
        
        return lesson;
    }
    
    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Lesson>> GetLessonsByCourseIdAsync(Guid courseId)
    {
        const string sql = @"
            SELECT *
            FROM Lessons
            WHERE CourseId = @CourseId";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var lessons = await connection.QueryAsync<Lesson>(sql, new { CourseId = courseId });
        
        if (!lessons.Any())
        {
            throw new KeyNotFoundException("Invalid Course ID.");
        }
        
        return lessons;
    }
    
    public async Task<Lesson?> GetLessonByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT *
            FROM Lessons
            WHERE Id = @Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var lesson = await connection.QueryFirstOrDefaultAsync<Lesson>(sql, new { Id = id });
        
        if (lesson == null)
        {
            throw new KeyNotFoundException("Invalid Lesson ID.");
        }
        
        return lesson;
    }
    
    #endregion
    
    #region Update
    
    public async Task<Lesson?> UpdateLessonAsync(Lesson lesson, IFormFile video, IFormFile? image)
    {
        var videoUploadResult = await _mediaUpload.UploadVideoAsync(video, "lesson_videos");
        lesson.VideoUrl = videoUploadResult.SecureUrl.ToString();
        
        if (image is not null)
        {
            var imageUploadResult = await _mediaUpload.UploadImageAsync(image, "lesson_images");
            lesson.ImageUrl = imageUploadResult.SecureUrl.ToString();
        }
        
        const string sql = @"
            UPDATE Lessons 
            SET Title = @Title,
                Description = @Description,
                VideoUrl = @VideoUrl,
                Order = @Order,
                Transcript = @Transcript,
                ImageUrl = @ImageUrl,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, lesson);
        
        return lesson;
    }
    
    #endregion
    
    #region Delete
    
    public async Task DeleteLessonAsync(Guid id)
    {
        const string sql = "DELETE FROM Lessons WHERE Id = @Id";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
        
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException("Invalid Lesson ID.");
        }
    }
    
    #endregion
}