using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _context;
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    
    public LessonRepository(AppDbContext context, CloudinaryDotNet.Cloudinary cloudinary)
    {
        _context = context;
        _cloudinary = cloudinary;
    }
    
    #region Create
    
    public async Task<Lesson> CreateLessonAsync(Lesson lesson, IFormFile image)
    {
        lesson.Id = Guid.NewGuid();
        
        if (image is not null)
        {
            var uploadResult = await UploadImageAsync(image);
            lesson.ImageUrl = uploadResult.SecureUrl.ToString();
        }
        
        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();
        
        return lesson;
    }
    
    public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "lessons",
            PublicId = Guid.NewGuid().ToString()
        };
        
        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
        {
            throw new Exception(result.Error.Message);
        }
        
        return result;
    }
    
    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Lesson>> GetLessonsByCourseIdAsync(Guid courseId)
    {
        if (!_context.Courses.Any(c => c.Id == courseId))
        {
            throw new KeyNotFoundException("Invalid Course ID.");
        }
        return await _context.Lessons
            .Where(l => l.CourseId == courseId)
            .ToListAsync();
    }
    
    public async Task<Lesson?> GetLessonByIdAsync(Guid id)
    {
        if (!_context.Lessons.Any(l => l.Id == id))
        {
            throw new KeyNotFoundException("Invalid Lesson ID.");
        }
        return await _context.Lessons.FindAsync(id);
    }
    
    #endregion
    
    #region Update
    
    public async Task<Lesson?> UpdateLessonAsync(Lesson lesson)
    {
        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();
        return lesson;
    }
    
    #endregion
    
    #region Delete
    
    public async Task DeleteLessonAsync(Guid id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        
        if (lesson is null)
        {
            throw new KeyNotFoundException("Invalid Lesson ID.");
        }
        
        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();
    }
    
    #endregion
}