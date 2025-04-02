using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Novademy.Application.Models;

namespace Novademy.Application.Repositories.Abstract;

public interface ILessonRepository
{
    Task<Lesson> CreateLessonAsync(Lesson lesson, IFormFile image);
    Task<ImageUploadResult> UploadImageAsync(IFormFile file);
    Task<IEnumerable<Lesson>> GetLessonsByCourseIdAsync(Guid courseId);
    Task<Lesson?> GetLessonByIdAsync(Guid id);
    Task<Lesson?> UpdateLessonAsync(Lesson lesson);
    Task DeleteLessonAsync(Guid id);
}