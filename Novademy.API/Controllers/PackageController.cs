using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Requests.Package;
using Novademy.API.Mapping;

namespace Novademy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class PackageController : ControllerBase
{
    private readonly IPackageRepository _repo;
    private readonly ICourseRepository _courseRepo;
    
    public PackageController(IPackageRepository repo, ICourseRepository courseRepo)
    {
        _repo = repo;
        _courseRepo = courseRepo;
    }

    #region GET

    /// <summary>
    /// Get all packages
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackages()
    {
        var packages = await _repo.GetAllPackagesAsync();
        var responses = packages.Select(p => p.MapToPackageResponse());
        return responses.Any() ? Ok(responses) : NoContent();
    }
    
    /// <summary>
    /// Get one package by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackage([FromRoute] Guid id)
    {
        try
        {
            var package = await _repo.GetPackageByIdAsync(id);
            var response = package!.MapToPackageResponse();
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
    /// Create a package with courses
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePackage([FromForm] CreatePackageRequest request)
    {
        var package = request.MapToPackage();
        try
        {
            var courses = new List<Course>();
            foreach (var courseId in request.CourseIds)
            {
                var course = await _courseRepo.GetCourseByIdAsync(courseId);
                if (course is not null)
                {
                    courses.Add(course);
                }
            }
            package.Courses = courses!;
            
            var createdPackage = await _repo.CreatePackageAsync(package);
            
            var response = createdPackage.MapToPackageResponse();
            return CreatedAtAction(nameof(GetPackage), new { id = response.Id },
                $"Package with ID {response.Id} created successfully.");
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
    
    #region PUT
    
    /// <summary>
    /// Update an existing package
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
    public async Task<IActionResult> UpdatePackage([FromRoute] Guid id, [FromBody] UpdatePackageRequest request)
    {
        try
        {
            var packageToUpdate = await _repo.GetPackageByIdAsync(id);

            packageToUpdate!.Title = request.Title;
            packageToUpdate.Description = request.Description;

            packageToUpdate.Courses.Clear();
            foreach (var courseId in request.CourseIds)
            {
                var course = await _courseRepo.GetCourseByIdAsync(courseId);
                if (course is not null)
                {
                    packageToUpdate.Courses.Add(course);
                }
            }

            var updatedPackage = await _repo.UpdatePackageAsync(packageToUpdate);

            var response = updatedPackage!.MapToPackageResponse();
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
    /// Delete a package by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePackage([FromRoute] Guid id)
    {
        try
        {
            await _repo.DeletePackageAsync(id);
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