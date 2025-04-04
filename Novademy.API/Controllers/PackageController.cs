using FluentValidation;
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
    private readonly IValidator<CreatePackageRequest> _createValidator;
    private readonly IValidator<UpdatePackageRequest> _updateValidator;
    
    public PackageController(
        IPackageRepository repo,
        ICourseRepository courseRepo,
        IValidator<CreatePackageRequest> createValidator,
        IValidator<UpdatePackageRequest> updateValidator)
    {
        _repo = repo;
        _courseRepo = courseRepo;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        var package = request.MapToPackage();
        try
        {
            package.Courses = new List<Course>();
            foreach (var courseId in request.CourseIds)
            {
                var course = await _courseRepo.GetCourseByIdAsync(courseId);
                if (course is not null)
                {
                    package.Courses.Add(course);
                }
            }
            
            var createdPackage = await _repo.CreatePackageAsync(package, request.Image!);
            
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
    public async Task<IActionResult> UpdatePackage([FromRoute] Guid id, [FromForm] UpdatePackageRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        try
        {
            var packageToUpdate = await _repo.GetPackageByIdAsync(id);
            
            packageToUpdate!.Title = request.Title;
            packageToUpdate.Description = request.Description;
            packageToUpdate.Price = request.Price;
            packageToUpdate.UpdatedAt = DateTime.UtcNow;
            
            packageToUpdate.Courses.Clear();
            foreach (var courseId in request.CourseIds)
            {
                var course = await _courseRepo.GetCourseByIdAsync(courseId);
                if (course is not null)
                {
                    packageToUpdate.Courses.Add(course);
                }
            }
            
            var updatedPackage = await _repo.UpdatePackageAsync(packageToUpdate, request.Image ?? null);
            
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