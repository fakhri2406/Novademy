using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface ILessonRepository
{
    Task<Lesson> CreateLessonAsync(Lesson lesson);
    Task<IEnumerable<Lesson>> GetLessonsByCourseIdAsync(Guid courseId);
    Task<Lesson?> GetLessonByIdAsync(Guid id);
    Task<Lesson?> UpdateLessonAsync(Lesson lesson);
    Task DeleteLessonAsync(Guid id);
}