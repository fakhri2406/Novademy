using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Novademy.Application.Data.EFCore;
using Novademy.Application.ExternalServices.Cloudinary;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;

namespace Novademy.Application.Repositories.Concrete;

public class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;

    public CourseRepository(AppDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }

    #region Create

    public async Task<Course> CreateAsync(Course course, IFormFile? image)
    {
        if (image is not null)
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(image, "courses");
            course.ImageUrl = uploadResult.SecureUrl.ToString();
        }

        course.CreatedAt = DateTime.UtcNow;
        course.UpdatedAt = DateTime.UtcNow;

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return course;
    }

    #endregion

    #region Read

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _context.Courses.ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            throw new KeyNotFoundException("Invalid Course ID.");
        }
        return course;
    }

    #endregion

    #region Update

    public async Task<Course?> UpdateAsync(Course course, IFormFile? image)
    {
        if (image is not null)
        {
            var imageUrl = course.ImageUrl;
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _cloudinaryService.DeleteFileAsync(imageUrl, ResourceType.Image);
            }
            var uploadResult = await _cloudinaryService.UploadImageAsync(image, "courses");
            course.ImageUrl = uploadResult.SecureUrl.ToString();
        }

        course.UpdatedAt = DateTime.UtcNow;

        _context.Courses.Update(course);
        await _context.SaveChangesAsync();

        return course;
    }

    #endregion
    
    #region Delete
    
    public async Task DeleteAsync(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            throw new KeyNotFoundException("Invalid Course ID.");
        }
        
        var imageUrl = course.ImageUrl;
        if (!string.IsNullOrEmpty(imageUrl))
        {
            await _cloudinaryService.DeleteFileAsync(imageUrl, ResourceType.Image);
        }
        
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
    }
    
    #endregion
}