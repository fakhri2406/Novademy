using Dapper;
using Microsoft.AspNetCore.Http;
using Novademy.Application.Data.Dapper;
using Novademy.Application.ExternalServices.AzureBlobStorage;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class LessonRepository : ILessonRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IAzureBlobService _azureBlobService;
    
    public LessonRepository(IDbConnectionFactory connectionFactory, IAzureBlobService azureBlobService)
    {
        _connectionFactory = connectionFactory;
        _azureBlobService = azureBlobService;
    }
    
    #region Create
    
    public async Task<Lesson> CreateLessonAsync(Lesson lesson, IFormFile video, IFormFile? image)
    {
        var videoUploadResult = await _azureBlobService.UploadFileAsync(video);
        lesson.VideoUrl = videoUploadResult;
        
        if (image is not null)
        {
            var imageUploadResult = await _azureBlobService.UploadFileAsync(image);
            lesson.ImageUrl = imageUploadResult;
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
        var videoUploadResult = await _azureBlobService.UploadFileAsync(video);
        lesson.VideoUrl = videoUploadResult;
        
        if (image is not null)
        {
            var imageUploadResult = await _azureBlobService.UploadFileAsync(image);
            lesson.ImageUrl = imageUploadResult;
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
        const string findVideoSql = "SELECT VideoUrl FROM Lessons WHERE Id = @Id";
        const string findImageSql = "SELECT ImageUrl FROM Lessons WHERE Id = @Id";
        const string deleteLessonSql = "DELETE FROM Lessons WHERE Id = @Id";
    
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var videoUrl = await connection.QueryFirstOrDefaultAsync<string>(findVideoSql, new { Id = id });

        if (!string.IsNullOrEmpty(videoUrl))
        {
            var fileName = Path.GetFileName(new Uri(videoUrl).LocalPath);
            await _azureBlobService.DeleteFileAsync(fileName);
        }
        
        var imageUrl = await connection.QueryFirstOrDefaultAsync<string>(findImageSql, new { Id = id });
        
        if (!string.IsNullOrEmpty(imageUrl))
        {
            var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            await _azureBlobService.DeleteFileAsync(fileName);
        }
        
        var affectedRows = await connection.ExecuteAsync(deleteLessonSql, new { Id = id });

        if (affectedRows == 0)
        {
            throw new KeyNotFoundException("Invalid Lesson ID.");
        }
    }
    
    #endregion
}