using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Tokens;
using Novademy.Application.ExternalServices.Email;
using Novademy.Application.Mapping;
using Novademy.Application.Models;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Responses.Auth;

namespace Novademy.Application.Services.Concrete;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _repo;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _environment;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<VerifyEmailRequest> _verifyEmailValidator;
    
    public AuthService(
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
    
    public async Task<Guid> RegisterAsync(RegisterRequest request)
    {
        await _registerValidator.ValidateAndThrowAsync(request);
        
        var user = request.MapToUser();
        var registeredUser = await _repo.RegisterUserAsync(user, request.ProfilePicture);
        
        var templatePath = Path.Combine(_environment.WebRootPath, "EmailTemplate.html");
        var htmlBody = await File.ReadAllTextAsync(templatePath);
        htmlBody = htmlBody.Replace("{0}", registeredUser.EmailVerificationCode)
                           .Replace("{1}", DateTime.Now.Year.ToString());
        
        await _emailService.SendEmailAsync(
            registeredUser.Email,
            "Email Verification Code",
            htmlBody,
            true);
        
        return registeredUser.Id;
    }
    
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        await _loginValidator.ValidateAndThrowAsync(request);
        
        var user = await _repo.LoginUserAsync(request.Username, request.Password);
        
        if (!user.IsEmailVerified)
            throw new Exception("Email not verified.");
        
        var accessToken = _tokenGenerator.GenerateAccessToken(user);
        var refreshToken = new RefreshToken
        {
            Token = _tokenGenerator.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            UserId = user.Id
        };
        await _repo.CreateRefreshTokenAsync(refreshToken);
        
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }
    
    public async Task VerifyEmailAsync(VerifyEmailRequest request)
    {
        await _verifyEmailValidator.ValidateAndThrowAsync(request);
        
        var user = await _repo.GetUserByIdAsync(request.UserId);

        if (user.IsEmailVerified)
        {
            throw new Exception("Email is already verified.");
        }
        
        if (user.EmailVerificationExpiry < DateTime.UtcNow)
        {
            throw new Exception("Verification code expired.");
        }
        
        if (user.EmailVerificationCode != request.Code)
        {
            throw new Exception("Invalid verification code.");
        }
        
        user.IsEmailVerified = true;
        user.EmailVerificationCode = null;
        user.EmailVerificationExpiry = null;
        await _repo.UpdateUserAsync(user);
    }
    
    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        var currentRefreshToken = await _repo.GetRefreshTokenAsync(request.Token);
        if (currentRefreshToken.ExpiresAt < DateTime.UtcNow)
        {
            await _repo.RemoveRefreshTokenAsync(currentRefreshToken.Token);
            throw new UnauthorizedAccessException("Expired refresh token. Please log in again.");
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
        
        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token
        };
    }
    
    public async Task LogoutAsync(Guid userId)
    {
        await _repo.RemoveAllRefreshTokensAsync(userId);
    }
} 