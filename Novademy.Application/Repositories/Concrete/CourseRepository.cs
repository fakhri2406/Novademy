using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    
    public CourseRepository(AppDbContext context, CloudinaryDotNet.Cloudinary cloudinary)
    {
        _context = context;
        _cloudinary = cloudinary;
    }
    
    #region Create
    
    public async Task<Course> CreateCourseAsync(Course course, IFormFile image) 
    {
        course.Id = Guid.NewGuid();
        
        if (image is not null)
        {
            var uploadResult = await UploadImageAsync(image);
            course.ImageUrl = uploadResult.SecureUrl.ToString();
        }
        
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        
        return course;
    }
    
    public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "courses",
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