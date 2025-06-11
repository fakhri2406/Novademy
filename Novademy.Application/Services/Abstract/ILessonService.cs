using Novademy.Contracts.Requests.Lesson;
using Novademy.Contracts.Responses.Lesson;

namespace Novademy.Application.Services.Abstract;

public interface ILessonService
{
    Task<LessonResponse> CreateAsync(CreateLessonRequest request);
    Task<IEnumerable<LessonResponse>> GetByCourseIdAsync(Guid courseId);
    Task<LessonResponse> GetByIdAsync(Guid id);
    Task<LessonResponse> UpdateAsync(Guid id, UpdateLessonRequest request);
    Task DeleteAsync(Guid id);
} 