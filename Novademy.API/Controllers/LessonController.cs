using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Requests.Lesson;
using Novademy.API.Mapping;

namespace Novademy.API.Controllers;

[ApiController]
[Authorize]
public class LessonController : ControllerBase
{
    private readonly ILessonRepository _repo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IValidator<CreateLessonRequest> _createValidator;
    private readonly IValidator<UpdateLessonRequest> _updateValidator;
    
    public LessonController(
        ILessonRepository repo,
        ISubscriptionRepository subscriptionRepo,
        IValidator<CreateLessonRequest> createValidator,
        IValidator<UpdateLessonRequest> updateValidator)
    {
        _repo = repo;
        _subscriptionRepo = subscriptionRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    #region GET
    
    /// <summary>
    /// Get all lessons of a specific course
    /// </summary>
    /// <param name="courseId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Lesson.GetLessons)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLessons([FromRoute] Guid courseId)
    {
        try
        {
            var lessons = await _repo.GetLessonsByCourseIdAsync(courseId);
            var responses = lessons.Select(l => l.MapToLessonResponse());
            return responses.Any() ? Ok(responses) : NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Get one lesson by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Lesson.GetLesson)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLesson([FromRoute] Guid id)
    {
        try
        {
            var lesson = await _repo.GetLessonByIdAsync(id);
            
            var isAdmin = User.IsInRole("Admin");
            var isTeacher = User.IsInRole("Teacher");
            
            if ((!isAdmin || !isTeacher) && !lesson!.IsFree)
            {
                var userId = Guid.Parse(User.FindFirst("id")?.Value ?? string.Empty);
                var hasAccess = await _subscriptionRepo.HasActiveSubscriptionForLessonAsync(userId, id);
                if (!hasAccess)
                {
                    return Forbid("You do not have access to this lesson.");
                }
            }
            
            var response = lesson!.MapToLessonResponse();
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region POST
    
    /// <summary>
    /// Create a lesson
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Lesson.CreateLesson)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateLesson([FromForm] CreateLessonRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        
        var lesson = request.MapToLesson();
        try
        {
            var createdLesson = await _repo.CreateLessonAsync(lesson, request.Video, request.Image!);
            
            var response = createdLesson.MapToLessonResponse();
            return CreatedAtAction(nameof(GetLesson), new { id = response.Id },
                $"Lesson with ID {response.Id} created successfully for Course {response.CourseId}.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region PUT
    
    /// <summary>
    /// Update an existing lesson
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    [Route(ApiEndPoints.Lesson.UpdateLesson)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateLesson([FromRoute] Guid id, [FromForm] UpdateLessonRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        
        try
        {
            var lessonToUpdate = await _repo.GetLessonByIdAsync(id);
            
            lessonToUpdate!.Title = request.Title;
            lessonToUpdate.Description = request.Description;
            lessonToUpdate.Order = request.Order;
            lessonToUpdate.Transcript = request.Transcript;
            lessonToUpdate.UpdatedAt = DateTime.UtcNow;
            
            var updatedLesson = await _repo.UpdateLessonAsync(lessonToUpdate, request.Video, request.Image ?? null);
            
            var response = updatedLesson!.MapToLessonResponse();
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region DELETE
    
    /// <summary>
    /// Delete a lesson by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route(ApiEndPoints.Lesson.DeleteLesson)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteLesson([FromRoute] Guid id)
    {
        try
        {
            await _repo.DeleteLessonAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
}