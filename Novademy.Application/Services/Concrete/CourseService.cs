using FluentValidation;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Mapping;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Course;
using Novademy.Contracts.Responses.Course;

namespace Novademy.Application.Services.Concrete;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repo;
    private readonly IValidator<CreateCourseRequest> _createValidator;
    private readonly IValidator<UpdateCourseRequest> _updateValidator;

    public CourseService(
        ICourseRepository repo,
        IValidator<CreateCourseRequest> createValidator,
        IValidator<UpdateCourseRequest> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<CourseResponse>> GetAllAsync()
    {
        var courses = await _repo.GetAllCoursesAsync();
        return courses.Select(c => c.MapToCourseResponse());
    }

    public async Task<CourseResponse> GetByIdAsync(Guid id)
    {
        var course = await _repo.GetCourseByIdAsync(id);
        return course.MapToCourseResponse();
    }

    public async Task<CourseResponse> CreateAsync(CreateCourseRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        
        var course = request.MapToCourse();
        var created = await _repo.CreateCourseAsync(course, request.Image);
        
        return created.MapToCourseResponse();
    }

    public async Task<CourseResponse> UpdateAsync(Guid id, UpdateCourseRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        
        var courseToUpdate = await _repo.GetCourseByIdAsync(id);
        
        courseToUpdate.Title = request.Title;
        courseToUpdate.Description = request.Description;
        courseToUpdate.Subject = request.Subject;
        courseToUpdate.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _repo.UpdateCourseAsync(courseToUpdate, request.Image);
        return updated.MapToCourseResponse();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteCourseAsync(id);
    }
} 