using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Requests.Lesson;
using Novademy.API.Mapping;

namespace Novademy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LessonController : ControllerBase
{
    private readonly ILessonRepository _repo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    
    public LessonController(ILessonRepository repo, ISubscriptionRepository subscriptionRepo)
    {
        _repo = repo;
        _subscriptionRepo = subscriptionRepo;
    }
    
    #region GET
    
    /// <summary>
    /// Get all lessons of a specific course
    /// </summary>
    /// <param name="courseId"></param>
    /// <returns></returns>
    [HttpGet("course/{courseId}")]
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
    [HttpGet("{id}")]
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
            
            if (!lesson!.IsFree)
            {
                var userId = Guid.Parse(User.FindFirst("id")?.Value ?? string.Empty);
                var hasAccess = await _subscriptionRepo.HasActiveSubscriptionForLessonAsync(userId, id);
                if (!hasAccess)
                {
                    return Forbid("You do not have access to this lesson.");
                }
            }
            
            var response = lesson.MapToLessonResponse();
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateLesson([FromForm] CreateLessonRequest request)
    {
        var lesson = request.MapToLesson();
        try
        {
            var createdLesson = await _repo.CreateLessonAsync(lesson);
            
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
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateLesson([FromRoute] Guid id, [FromBody] UpdateLessonRequest request)
    {
        try
        {
            var lessonToUpdate = await _repo.GetLessonByIdAsync(id);
            
            lessonToUpdate!.Title = request.Title;
            lessonToUpdate.Description = request.Description;
            lessonToUpdate.VideoUrl = request.VideoUrl;
            lessonToUpdate.Order = request.Order;
            lessonToUpdate.Transcript = request.Transcript;
            lessonToUpdate.ImageUrl = request.ImageUrl;
            lessonToUpdate.UpdatedAt = DateTime.UtcNow;
            
            var updatedLesson = await _repo.UpdateLessonAsync(lessonToUpdate);
            
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
    [HttpDelete("{id}")]
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