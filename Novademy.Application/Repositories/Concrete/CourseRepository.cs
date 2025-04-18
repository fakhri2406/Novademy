using Dapper;
using Microsoft.AspNetCore.Http;
using Novademy.Application.Data.Dapper;
using Novademy.Application.ExternalServices.AzureBlobStorage;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class CourseRepository : ICourseRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IAzureBlobService _azureBlobService;
    
    public CourseRepository(IDbConnectionFactory connectionFactory, IAzureBlobService azureBlobService)
    {
        _connectionFactory = connectionFactory;
        _azureBlobService = azureBlobService;
    }
    
    #region Create
    
    public async Task<Course> CreateCourseAsync(Course course, IFormFile? image) 
    {
        if (image is not null)
        {
            var uploadResult = await _azureBlobService.UploadFileAsync(image);
            course.ImageUrl = uploadResult;
        }
        
        const string sql = @"
            INSERT INTO Courses (Id, Title, Description, Subject, ImageUrl, CreatedAt, UpdatedAt)
            VALUES (@Id, @Title, @Description, @Subject, @ImageUrl, @CreatedAt, @UpdatedAt)";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, course);
        
        return course;
    }
    
    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        const string sql = "SELECT * FROM Courses";
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Course>(sql);
    }
    
    public async Task<Course?> GetCourseByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT *
            FROM Courses
            WHERE Id = @Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var course = await connection.QueryFirstOrDefaultAsync<Course>(sql, new { Id = id });
        
        if (course == null)
        {
            throw new KeyNotFoundException("Invalid Course ID.");
        }
        
        return course;
    }
    
    #endregion
    
    #region Update
    
    public async Task<Course?> UpdateCourseAsync(Course course, IFormFile? image)
    {
        if (image is not null)
        {
            var uploadResult = await _azureBlobService.UploadFileAsync(image);
            course.ImageUrl = uploadResult;
        }
        
        const string sql = @"
            UPDATE Courses 
            SET Title = @Title,
                Description = @Description,
                Subject = @Subject,
                ImageUrl = @ImageUrl,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
            
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(sql, course);
        
        return course;
    }
    
    #endregion
    
    #region Delete
    
    public async Task DeleteCourseAsync(Guid id)
    {
        const string findImageSql = "SELECT ImageUrl FROM Courses WHERE Id = @Id";
        const string deleteCourseSql = "DELETE FROM Courses WHERE Id = @Id";
    
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var imageUrl = await connection.QueryFirstOrDefaultAsync<string>(findImageSql, new { Id = id });
        
        if (!string.IsNullOrEmpty(imageUrl))
        {
            var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            await _azureBlobService.DeleteFileAsync(fileName);
        }
        
        var affectedRows = await connection.ExecuteAsync(deleteCourseSql, new { Id = id });

        if (affectedRows == 0)
        {
            throw new KeyNotFoundException("Invalid Course ID.");
        }
    }
    
    #endregion
}