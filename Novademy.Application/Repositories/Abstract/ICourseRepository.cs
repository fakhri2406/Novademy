using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface ICourseRepository
{
    Task<Course> CreateCourseAsync(Course course, IFormFile image);
    Task<ImageUploadResult> UploadImageAsync(IFormFile file);
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<Course?> GetCourseByIdAsync(Guid id);
    Task<Course?> UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(Guid id);
}