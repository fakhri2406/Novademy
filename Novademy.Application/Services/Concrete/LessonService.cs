using FluentValidation;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Mapping;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Lesson;
using Novademy.Contracts.Responses.Lesson;

namespace Novademy.Application.Services.Concrete;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _repo;
    private readonly IValidator<CreateLessonRequest> _createValidator;
    private readonly IValidator<UpdateLessonRequest> _updateValidator;

    public LessonService(
        ILessonRepository repo,
        IValidator<CreateLessonRequest> createValidator,
        IValidator<UpdateLessonRequest> updateValidator)
    {
        _repo = repo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    #region Create
    
    public async Task<LessonResponse> CreateAsync(CreateLessonRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        
        var lesson = request.MapToLesson();
        var created = await _repo.CreateAsync(lesson, request.Video, request.Image);
        
        return created.MapToLessonResponse();
    }
    
    #endregion
    
    #region Read

    public async Task<IEnumerable<LessonResponse>> GetByCourseIdAsync(Guid courseId)
    {
        var lessons = await _repo.GetByCourseIdAsync(courseId);
        return lessons.Select(l => l.MapToLessonResponse());
    }

    public async Task<LessonResponse> GetByIdAsync(Guid id)
    {
        var lesson = await _repo.GetByIdAsync(id);
        return lesson.MapToLessonResponse();
    }
    
    #endregion
    
    #region Update

    public async Task<LessonResponse> UpdateAsync(Guid id, UpdateLessonRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        
        var lessonToUpdate = await _repo.GetByIdAsync(id);
        
        lessonToUpdate.Title = request.Title;
        lessonToUpdate.Description = request.Description;
        lessonToUpdate.Order = request.Order;
        lessonToUpdate.Transcript = request.Transcript;
        lessonToUpdate.UpdatedAt = DateTime.UtcNow;
        
        var updated = await _repo.UpdateAsync(lessonToUpdate, request.Video, request.Image);
        return updated.MapToLessonResponse();
    }
    
    #endregion
    
    #region Delete

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }
    
    #endregion
} 