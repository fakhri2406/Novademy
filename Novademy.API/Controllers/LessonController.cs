using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Requests.Lesson;
using Novademy.Application.Services.Abstract;

namespace Novademy.API.Controllers;

[ApiController]
[Authorize]
public class LessonController : ControllerBase
{
    private readonly ILessonService _lessonService;
    private readonly ISubscriptionRepository _subscriptionRepo;
    
    public LessonController(ILessonService lessonService, ISubscriptionRepository subscriptionRepo)
    {
        _lessonService = lessonService;
        _subscriptionRepo = subscriptionRepo;
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLessons([FromRoute] Guid courseId)
    {
        try
        {
            var responses = await _lessonService.GetByCourseIdAsync(courseId);
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLesson([FromRoute] Guid id)
    {
        try
        {
            var response = await _lessonService.GetByIdAsync(id);
            
            var isAdmin = User.IsInRole("Admin");
            var isTeacher = User.IsInRole("Teacher");
            
            if ((!isAdmin && !isTeacher) && !response.IsFree)
            {
                var userId = Guid.Parse(User.FindFirst("id")?.Value ?? string.Empty);
                var hasAccess = await _subscriptionRepo.HasActiveSubscriptionForLessonAsync(userId, id);
                if (!hasAccess)
                {
                    return Forbid("You do not have access to this lesson.");
                }
            }
            
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateLesson([FromForm] CreateLessonRequest request)
    {
        try
        {
            var response = await _lessonService.CreateAsync(request);
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateLesson([FromRoute] Guid id, [FromForm] UpdateLessonRequest request)
    {
        try
        {
            var response = await _lessonService.UpdateAsync(id, request);
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteLesson([FromRoute] Guid id)
    {
        try
        {
            await _lessonService.DeleteAsync(id);
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