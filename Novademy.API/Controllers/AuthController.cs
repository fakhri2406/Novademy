using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Tokens;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Responses.Auth;
using Novademy.API.Mapping;

namespace Novademy.API.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _repo;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    
    public AuthController(
        IAuthRepository repo,
        ITokenGenerator tokenGenerator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _repo = repo;
        _tokenGenerator = tokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
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
        await _registerValidator.ValidateAndThrowAsync(request);
        
        var user = request.MapToUser();
        try
        {
            var registeredUser = await _repo.RegisterUserAsync(user, request.ProfilePicture ?? null);
            return CreatedAtAction(nameof(Register), new { id = registeredUser.Id },
                $"User with ID {registeredUser.Id} registered successfully.");
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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
        await _loginValidator.ValidateAndThrowAsync(request);
        
        try
        {
            var user = await _repo.LoginUserAsync(request.Username, request.Password);
            
            var accessToken = _tokenGenerator.GenerateAccessToken(user);
            var refreshToken = new RefreshToken
            {
                Token = _tokenGenerator.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                UserId = user.Id
            };
            
            await _repo.CreateRefreshTokenAsync(refreshToken);
    
            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
            
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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var currentRefreshToken = await _repo.GetRefreshTokenAsync(request.Token);
            
            if (currentRefreshToken.ExpiresAt < DateTime.UtcNow)
            {
                await _repo.RemoveRefreshTokenAsync(currentRefreshToken.Token);
                return Unauthorized("Expired refresh token. Please log in again.");
            }
            
            var newAccessToken = _tokenGenerator.GenerateAccessToken(currentRefreshToken.User!);
            var newRefreshToken = new RefreshToken
            {
                Token = _tokenGenerator.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                UserId = currentRefreshToken.UserId
            };
            
            await _repo.CreateRefreshTokenAsync(newRefreshToken);
            await _repo.RemoveRefreshTokenAsync(currentRefreshToken.Token);
            
            var response = new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
            
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
        try
        {
            await _repo.RemoveAllRefreshTokensAsync(id);
            return Ok("User logged out.");
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