using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Contracts.Requests.Auth;
using Novademy.Application.Services.Abstract;

namespace Novademy.API.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }
    
    #region Register
    
    /// <summary>
    /// Create a user
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Auth.Register)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromForm] RegisterRequest request)
    {
        var userId = await _authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), new { id = userId },
            $"User with ID {userId} registered successfully.");
    }
    
    #endregion
    
    #region Login
    
    /// <summary>
    /// Log the user in and generate a pair of tokens
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Auth.Login)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromForm] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }
    
    #endregion
    
    #region Verify Email
    
    /// <summary>
    /// Verify a user's email with code
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Auth.VerifyEmail)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        await _authService.VerifyEmailAsync(request);
        return Ok();
    }
    
    #endregion
    
    #region Refresh
    
    /// <summary>
    /// Generate a new pair of tokens
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Auth.Refresh)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshAsync(request);
        return Ok(response);
    }
    
    #endregion
    
    #region Logout
    
    /// <summary>
    /// Clear all refresh tokens
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Auth.Logout)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromRoute] Guid id)
    {
        await _authService.LogoutAsync(id);
        return Ok("User logged out.");
    }
    
    #endregion
    
    #region Get Current User
    
    /// <summary>
    /// Get the current user's information
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Auth.GetCurrentUser)]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUser()
    {
        _logger.LogInformation("GetCurrentUser endpoint called");
        _logger.LogInformation("User claims: {@Claims}", User.Claims.Select(c => new { c.Type, c.Value }));
        
        var userIdClaim = User.FindFirst("id");
        if (userIdClaim == null)
        {
            _logger.LogWarning("User ID claim not found in token");
            return Unauthorized("User ID claim not found in token");
        }
        
        var userId = Guid.Parse(userIdClaim.Value);
        _logger.LogInformation("User ID from token: {UserId}", userId);
            
        var user = await _authService.GetUserByIdAsync(userId);
        return Ok(user);
    }
    
    /// <summary>
    /// Test endpoint to verify routing and authentication
    /// </summary>
    [HttpGet("test-auth")]
    [Authorize]
    public IActionResult TestAuth()
    {
        _logger.LogInformation("TestAuth endpoint called");
        _logger.LogInformation("User claims: {@Claims}", User.Claims.Select(c => new { c.Type, c.Value }));
        return Ok(new { message = "Authentication successful", claims = User.Claims.Select(c => new { c.Type, c.Value }) });
    }
    
    #endregion
}