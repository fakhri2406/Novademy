using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface ICourseRepository
{
    Task<Course> CreateCourseAsync(Course course);
    Task<IEnumerable<Course>> GetAllCoursesAsync();
    Task<Course?> GetCourseByIdAsync(Guid id);
    Task<Course?> UpdateCourseAsync(Course course);
    Task DeleteCourseAsync(Guid id);
}