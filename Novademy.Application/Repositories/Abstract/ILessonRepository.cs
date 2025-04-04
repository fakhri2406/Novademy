using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface ILessonRepository
{
    Task<Lesson> CreateLessonAsync(Lesson lesson, IFormFile video, IFormFile? image);
    Task<IEnumerable<Lesson>> GetLessonsByCourseIdAsync(Guid courseId);
    Task<Lesson?> GetLessonByIdAsync(Guid id);
    Task<Lesson?> UpdateLessonAsync(Lesson lesson, IFormFile video, IFormFile? image);
    Task DeleteLessonAsync(Guid id);
}