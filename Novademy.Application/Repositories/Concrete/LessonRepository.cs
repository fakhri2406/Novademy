using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Novademy.Application.Cloudinary;
using Novademy.Application.Data.EFCore;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _context;
    private readonly IMediaUpload _mediaUpload;
    
    public LessonRepository(AppDbContext context, IMediaUpload mediaUpload)
    {
        _context = context;
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
        
        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();
        
        return lesson;
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
    
    public async Task<Lesson?> UpdateLessonAsync(Lesson lesson, IFormFile video, IFormFile? image)
    {
        var videoUploadResult = await _mediaUpload.UploadVideoAsync(video, "lesson_videos");
        lesson.VideoUrl = videoUploadResult.SecureUrl.ToString();
        
        if (image is not null)
        {
            var imageUploadResult = await _mediaUpload.UploadImageAsync(image, "lesson_images");
            lesson.ImageUrl = imageUploadResult.SecureUrl.ToString();
        }
        
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