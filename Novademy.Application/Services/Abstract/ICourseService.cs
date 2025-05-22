using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novademy.Contracts.Requests.Course;
using Novademy.Contracts.Responses.Course;

namespace Novademy.Application.Services.Abstract;

public interface ICourseService
{
    Task<IEnumerable<CourseResponse>> GetAllAsync();
    Task<CourseResponse> GetByIdAsync(Guid id);
    Task<CourseResponse> CreateAsync(CreateCourseRequest request);
    Task<CourseResponse> UpdateAsync(Guid id, UpdateCourseRequest request);
    Task DeleteAsync(Guid id);
} 