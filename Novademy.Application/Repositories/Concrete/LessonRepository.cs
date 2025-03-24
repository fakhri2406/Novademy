using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _context;
    
    public LessonRepository(AppDbContext context)
    {
        _context = context;
    }
    
    #region Create
    
    public async Task<Lesson> CreateLessonAsync(Lesson lesson)
        {
            lesson.Id = Guid.NewGuid();
            
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            
            return lesson;
        }
    
    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Lesson>> GetLessonsByCourseIdAsync(Guid courseId)
    {
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