using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;
    
    public CourseRepository(AppDbContext context)
    {
        _context = context;
    }

    #region Create

    public async Task<Course> CreateCourseAsync(Course course)
        {
            course.Id = Guid.NewGuid();
            
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            
            return course;
        }

    #endregion
    
    #region Read
    
    public async Task<IEnumerable<Course>> GetAllCoursesAsync()
    {
        return await _context.Courses.ToListAsync();
    }
    
    public async Task<Course?> GetCourseByIdAsync(Guid id)
    {
        if (!_context.Courses.Any(c => c.Id == id))
        {
            throw new KeyNotFoundException("Invalid Course ID.");
        }
        return await _context.Courses.FindAsync(id);
    }
    
    #endregion
    
    #region Update

    public async Task<Course?> UpdateCourseAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
        return course;
    }
    
    #endregion
    
    #region Delete

    public async Task DeleteCourseAsync(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course is null)
        {
            throw new KeyNotFoundException("Invalid Course ID.");
        }
        
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
    }
    
    #endregion
}