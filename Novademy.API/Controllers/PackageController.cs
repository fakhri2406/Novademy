using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Package;

namespace Novademy.API.Controllers;

[ApiController]
public class PackageController : ControllerBase
{
    private readonly IPackageService _packageService;
    
    public PackageController(IPackageService packageService)
    {
        _packageService = packageService;
    }
    
    #region GET

    /// <summary>
    /// Get all packages
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Package.GetPackages)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPackages()
    {
        var responses = await _packageService.GetAllAsync();
        return responses.Any() ? Ok(responses) : NoContent();
    }
    
    /// <summary>
    /// Get one package by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Package.GetPackage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPackage([FromRoute] Guid id)
    {
        var response = await _packageService.GetByIdAsync(id);
        return Ok(response);
    }

    #endregion
    
    #region POST
    
    /// <summary>
    /// Create a package with courses
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Package.CreatePackage)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePackage([FromForm] CreatePackageRequest request)
    {
        var response = await _packageService.CreateAsync(request);
        return CreatedAtAction(nameof(GetPackage), new { id = response.Id },
            $"Package with ID {response.Id} created successfully.");
    }
    
    #endregion
    
    #region PUT
    
    /// <summary>
    /// Update an existing package
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    [Route(ApiEndPoints.Package.UpdatePackage)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePackage([FromRoute] Guid id, [FromForm] UpdatePackageRequest request)
    {
        var response = await _packageService.UpdateAsync(id, request);
        return Ok(response);
    }
    
    #endregion
    
    #region DELETE
    
    /// <summary>
    /// Delete a package by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route(ApiEndPoints.Package.DeletePackage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePackage([FromRoute] Guid id)
    {
        await _packageService.DeleteAsync(id);
        return NoContent();
    }
    
    #endregion
}