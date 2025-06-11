using Novademy.Contracts.Requests.Course;
using Novademy.Contracts.Responses.Course;

namespace Novademy.Application.Services.Abstract;

public interface ICourseService
{
    Task<CourseResponse> CreateAsync(CreateCourseRequest request);
    Task<IEnumerable<CourseResponse>> GetAllAsync();
    Task<CourseResponse> GetByIdAsync(Guid id);
    Task<CourseResponse> UpdateAsync(Guid id, UpdateCourseRequest request);
    Task DeleteAsync(Guid id);
} 