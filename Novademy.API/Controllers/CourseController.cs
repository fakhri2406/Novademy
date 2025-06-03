using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Application.Services.Abstract;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Requests.Course;

namespace Novademy.API.Controllers;

[ApiController]
[Authorize]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ISubscriptionRepository _subscriptionRepo;
    
    public CourseController(ICourseService courseService, ISubscriptionRepository subscriptionRepo)
    {
        _courseService = courseService;
        _subscriptionRepo = subscriptionRepo;
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
        var responses = await _courseService.GetAllAsync();
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
        
        var response = await _courseService.GetByIdAsync(id);
        
        return Ok(response);
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
        var response = await _courseService.CreateAsync(request);
        return CreatedAtAction(nameof(GetCourse), new { id = response.Id },
            $"Course with ID {response.Id} created successfully.");
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
        var response = await _courseService.UpdateAsync(id, request);
        return Ok(response);
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
        await _courseService.DeleteAsync(id);
        return NoContent();
    }
    
    #endregion
}