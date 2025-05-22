using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Tokens;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Responses.Auth;
using Novademy.API.Mapping;
using Novademy.Application.ExternalServices.Email;

namespace Novademy.API.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _repo;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _environment;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<VerifyEmailRequest> _verifyEmailValidator;
    
    public AuthController(
        IAuthRepository repo,
        ITokenGenerator tokenGenerator,
        IEmailService emailService,
        IWebHostEnvironment environment,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<VerifyEmailRequest> verifyEmailValidator)
    {
        _repo = repo;
        _tokenGenerator = tokenGenerator;
        _emailService = emailService;
        _environment = environment;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _verifyEmailValidator = verifyEmailValidator;
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
            
            string templatePath = Path.Combine(_environment.WebRootPath, "EmailTemplate.html");
            string htmlBody = await System.IO.File.ReadAllTextAsync(templatePath);
            
            htmlBody = htmlBody.Replace("{0}", registeredUser.EmailVerificationCode)
                               .Replace("{1}", DateTime.Now.Year.ToString());
            
            await _emailService.SendEmailAsync(
                registeredUser.Email,
                "Email Verification Code",
                htmlBody,
                true);
            
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
            
            if (!user.IsEmailVerified)
            {
                return BadRequest("Email not verified.");
            }
            
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
        await _verifyEmailValidator.ValidateAndThrowAsync(request);
        
        try
        {
            var user = await _repo.GetUserByIdAsync(request.UserId);
            
            if (user.IsEmailVerified)
            {
                return BadRequest("Email already verified.");
            }
            
            if (user.EmailVerificationExpiry < DateTime.UtcNow)
            {
                return BadRequest("Verification code expired.");
            }
            
            if (user.EmailVerificationCode != request.Code)
            {
                return BadRequest("Invalid verification code.");
            }
            
            user.IsEmailVerified = true;
            user.EmailVerificationCode = null;
            user.EmailVerificationExpiry = null;
            await _repo.UpdateUserAsync(user);
            
            return Ok();
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