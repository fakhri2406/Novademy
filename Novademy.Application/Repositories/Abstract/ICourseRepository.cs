using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface ICourseRepository
{
    Task<Course> CreateAsync(Course course, IFormFile? image);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<Course?> GetByIdAsync(Guid id);
    Task<Course?> UpdateAsync(Course course, IFormFile? image);
    Task DeleteAsync(Guid id);
}