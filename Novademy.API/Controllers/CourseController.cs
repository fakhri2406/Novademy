using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.API.Mapping;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Requests.Course;

namespace Novademy.API.Controllers;

[ApiController]
[Authorize]
public class CourseController : ControllerBase
{
    private readonly ICourseRepository _repo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IValidator<CreateCourseRequest> _createValidator;
    private readonly IValidator<UpdateCourseRequest> _updateValidator;
    
    public CourseController(
        ICourseRepository repo,
        ISubscriptionRepository subscriptionRepo,
        IValidator<CreateCourseRequest> createValidator,
        IValidator<UpdateCourseRequest> updateValidator)
    {
        _repo = repo;
        _subscriptionRepo = subscriptionRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }
    
    #region GET
    
    /// <summary>
    /// Get all courses
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Course.GetCourses)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await _repo.GetAllCoursesAsync();
        var responses = courses.Select(c => c.MapToCourseResponse());
        return responses.Any() ? Ok(responses) : NoContent();
    }
    
    /// <summary>
    /// Get one course by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Course.GetCourse)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCourse([FromRoute] Guid id)
    {
        try
        {
            var course = await _repo.GetCourseByIdAsync(id);
            
            var isAdmin = User.IsInRole("Admin");
            var isTeacher = User.IsInRole("Teacher");
            
            if (!isAdmin && !isTeacher)
            {
                var userId = Guid.Parse(User.FindFirst("id")?.Value ?? string.Empty);
                var hasAccess = await _subscriptionRepo.HasActiveSubscriptionForCourseAsync(userId, id);
                if (!hasAccess)
                {
                    return Forbid("You do not have access to this course.");
                }
            }
            
            var response = course!.MapToCourseResponse();
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
    /// Create a course
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Course.CreateCourse)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCourse([FromForm] CreateCourseRequest request)
    {
        await _createValidator.ValidateAndThrowAsync(request);
        
        var course = request.MapToCourse();
        try
        {
            var createdCourse = await _repo.CreateCourseAsync(course, request.Image!);
            
            var response = createdCourse.MapToCourseResponse();
            return CreatedAtAction(nameof(GetCourse), new { id = response.Id },
                $"Course with ID {response.Id} created successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region PUT
    
    /// <summary>
    /// Update an existing course
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    [Route(ApiEndPoints.Course.UpdateCourse)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCourse([FromRoute] Guid id, [FromForm] UpdateCourseRequest request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);
        
        try
        {
            var courseToUpdate = await _repo.GetCourseByIdAsync(id);
            
            courseToUpdate!.Title = request.Title;
            courseToUpdate.Description = request.Description;
            courseToUpdate.Subject = request.Subject;
            courseToUpdate.UpdatedAt = DateTime.UtcNow;
            
            var updatedCourse = await _repo.UpdateCourseAsync(courseToUpdate, request.Image ?? null);
            
            var response = updatedCourse!.MapToCourseResponse();
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
    /// Delete a course by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route(ApiEndPoints.Course.DeleteCourse)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCourse([FromRoute] Guid id)
    {
        try
        {
            await _repo.DeleteCourseAsync(id);
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