using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.API.Mapping;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Tokens;
using Novademy.Application.Validators.Auth;
using Novademy.Contracts.Enums;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Responses.Auth;
using Xunit;

namespace Novademy.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthRepository> _repoMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly AuthController _controller;
    
    public AuthControllerTests()
    {
        _repoMock = new Mock<IAuthRepository>();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _registerValidator = new RegisterRequestValidator();
        _loginValidator = new LoginRequestValidator();
        
        _controller = new AuthController(
            _repoMock.Object,
            _tokenGeneratorMock.Object,
            _registerValidator,
            _loginValidator);
    }
    
    #region Register Tests
    
    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "Pass1234",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "0551234567",
            RoleId = 1,
            Group = 1,
            Sector = SectorType.Azerbaijani,
            ProfilePicture = null
        };
        var user = request.MapToUser();
        _repoMock.Setup(r => r.RegisterUserAsync(It.Is<User>(u => u.Username == "testuser" && u.PhoneNumber == "+994551234567"), null))
                 .ReturnsAsync(user);
        
        // Act
        var result = await _controller.Register(request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.Register), createdResult.ActionName);
        Assert.Equal(user.Id, createdResult.RouteValues["id"]);
        Assert.Equal($"User with ID {user.Id} registered successfully.", createdResult.Value);
    }
    
    [Fact]
    public async Task Register_DuplicateUser_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "duplicate",
            Password = "Pass1234",
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@example.com",
            PhoneNumber = "0551234567",
            RoleId = 1,
            Group = 1,
            Sector = SectorType.Azerbaijani
        };
        _repoMock.Setup(r => r.RegisterUserAsync(It.IsAny<User>(), null))
                 .ThrowsAsync(new ArgumentException("User already exists"));
        
        // Act
        var result = await _controller.Register(request);
        
        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal("User already exists", conflictResult.Value);
    }
    
    [Fact]
    public async Task Register_InvalidPassword_ThrowsValidationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "testuser",
            Password = "pass", // Too short and no uppercase/digit
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "0551234567",
            RoleId = 1,
            Group = 1,
            Sector = SectorType.Azerbaijani
        };
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _controller.Register(request));
    }
    
    #endregion
    
    #region Login Tests
    
    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "Pass1234" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Password = "Pass1234",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+994551234567",
            RoleId = 3
        };
        var accessToken = "access-token";
        var refreshToken = "refresh-token";
        _repoMock.Setup(r => r.LoginUserAsync(request.Username, request.Password)).ReturnsAsync(user);
        _tokenGeneratorMock.Setup(t => t.GenerateAccessToken(user)).Returns(accessToken);
        _tokenGeneratorMock.Setup(t => t.GenerateRefreshToken()).Returns(refreshToken);
        _repoMock.Setup(r => r.CreateRefreshTokenAsync(It.Is<RefreshToken>(rt => rt.UserId == user.Id))).Returns(Task.CompletedTask);
        
        // Act
        var result = await _controller.Login(request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(accessToken, response.AccessToken);
        Assert.Equal(refreshToken, response.RefreshToken);
    }
    
    [Fact]
    public async Task Login_InvalidCredentials_ReturnsNotFound()
    {
        // Arrange
        var request = new LoginRequest { Username = "wrong", Password = "wrong" };
        _repoMock.Setup(r => r.LoginUserAsync(request.Username, request.Password))
                 .ThrowsAsync(new KeyNotFoundException("User not found"));
        
        // Act
        var result = await _controller.Login(request);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found", notFoundResult.Value);
    }
    
    [Fact]
    public async Task Login_EmptyUsername_ThrowsValidationException()
    {
        // Arrange
        var request = new LoginRequest { Username = "", Password = "Pass1234" };
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _controller.Login(request));
    }
    
    #endregion
    
    #region Refresh Tests
    
    [Fact]
    public async Task Refresh_ValidToken_ReturnsOkWithNewTokens()
    {
        // Arrange
        var request = new RefreshTokenRequest { Token = "valid-token" };
        var user = new User { Id = Guid.NewGuid(), Username = "testuser", RoleId = 3 };
        var currentRefreshToken = new RefreshToken
        {
            Token = request.Token,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            User = user
        };
        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";
        _repoMock.Setup(r => r.GetRefreshTokenAsync(request.Token)).ReturnsAsync(currentRefreshToken);
        _tokenGeneratorMock.Setup(t => t.GenerateAccessToken(user)).Returns(newAccessToken);
        _tokenGeneratorMock.Setup(t => t.GenerateRefreshToken()).Returns(newRefreshToken);
        _repoMock.Setup(r => r.CreateRefreshTokenAsync(It.Is<RefreshToken>(rt => rt.UserId == user.Id))).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.RemoveRefreshTokenAsync(request.Token)).Returns(Task.CompletedTask);
        
        // Act
        var result = await _controller.Refresh(request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(newAccessToken, response.AccessToken);
        Assert.Equal(newRefreshToken, response.RefreshToken);
    }
    
    [Fact]
    public async Task Refresh_ExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest { Token = "expired-token" };
        var expiredRefreshToken = new RefreshToken
        {
            Token = request.Token,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        _repoMock.Setup(r => r.GetRefreshTokenAsync(request.Token)).ReturnsAsync(expiredRefreshToken);
        _repoMock.Setup(r => r.RemoveRefreshTokenAsync(request.Token)).Returns(Task.CompletedTask);
        
        // Act
        var result = await _controller.Refresh(request);
        
        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Expired refresh token. Please log in again.", unauthorizedResult.Value);
    }
    
    #endregion
    
    #region Logout Tests
    
    [Fact]
    public async Task Logout_ValidId_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.RemoveAllRefreshTokensAsync(userId)).Returns(Task.CompletedTask);
        
        // Act
        var result = await _controller.Logout(userId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User logged out.", okResult.Value);
    }
    
    [Fact]
    public async Task Logout_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.RemoveAllRefreshTokensAsync(userId))
                 .ThrowsAsync(new KeyNotFoundException("User not found"));
        
        // Act
        var result = await _controller.Logout(userId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("User not found", notFoundResult.Value);
    }
    
    #endregion
}