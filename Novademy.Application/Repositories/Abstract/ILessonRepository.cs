using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface ILessonRepository
{
    Task<Lesson> CreateAsync(Lesson lesson, IFormFile video, IFormFile? image);
    Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId);
    Task<Lesson?> GetByIdAsync(Guid id);
    Task<Lesson?> UpdateAsync(Lesson lesson, IFormFile video, IFormFile? image);
    Task DeleteAsync(Guid id);
}