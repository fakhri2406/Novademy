using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Novademy.API.Controllers;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Auth;
using Novademy.Contracts.Responses.Auth;
using Novademy.Contracts.Responses.User;
using Xunit;

namespace Novademy.UnitTests.ControllerTests;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsCreatedAtAction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new RegisterRequest
        {
            Username = "user",
            Password = "pass",
            FirstName = "First",
            LastName = "Last",
            Email = "email@example.com",
            PhoneNumber = "1234567890",
            Group = 1,
            Sector = default,
            ProfilePicture = null
        };
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(userId);
        var mockUserService = new Mock<IUserService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var controller = new AuthController(mockAuthService.Object, mockUserService.Object, mockLogger.Object);

        // Act
        var result = await controller.Register(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(AuthController.Register), createdResult.ActionName);
        Assert.Equal(userId, createdResult.RouteValues["id"]);
        Assert.Equal($"User with ID {userId} registered successfully.", createdResult.Value);
        mockAuthService.Verify(s => s.RegisterAsync(request), Times.Once);
    }

    [Fact]
    public async Task Login_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new LoginRequest { Username = "user", Password = "pass" };
        var authResponse = new AuthResponse { AccessToken = "access", RefreshToken = "refresh" };
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(s => s.LoginAsync(request)).ReturnsAsync(authResponse);
        var mockUserService = new Mock<IUserService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var controller = new AuthController(mockAuthService.Object, mockUserService.Object, mockLogger.Object);

        // Act
        var result = await controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(authResponse, okResult.Value);
        mockAuthService.Verify(s => s.LoginAsync(request), Times.Once);
    }

    [Fact]
    public async Task VerifyEmail_ReturnsOk()
    {
        // Arrange
        var request = new VerifyEmailRequest { UserId = Guid.NewGuid(), Code = "code" };
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(s => s.VerifyEmailAsync(request)).Returns(Task.CompletedTask);
        var mockUserService = new Mock<IUserService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var controller = new AuthController(mockAuthService.Object, mockUserService.Object, mockLogger.Object);

        // Act
        var result = await controller.VerifyEmail(request);

        // Assert
        Assert.IsType<OkResult>(result);
        mockAuthService.Verify(s => s.VerifyEmailAsync(request), Times.Once);
    }

    [Fact]
    public async Task Refresh_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new RefreshTokenRequest { Token = "token" };
        var authResponse = new AuthResponse { AccessToken = "newAccess", RefreshToken = "newRefresh" };
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(s => s.RefreshAsync(request)).ReturnsAsync(authResponse);
        var mockUserService = new Mock<IUserService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var controller = new AuthController(mockAuthService.Object, mockUserService.Object, mockLogger.Object);

        // Act
        var result = await controller.Refresh(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(authResponse, okResult.Value);
        mockAuthService.Verify(s => s.RefreshAsync(request), Times.Once);
    }

    [Fact]
    public async Task Logout_ReturnsOkWithMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockAuthService = new Mock<IAuthService>();
        mockAuthService.Setup(s => s.LogoutAsync(userId)).Returns(Task.CompletedTask);
        var mockUserService = new Mock<IUserService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var controller = new AuthController(mockAuthService.Object, mockUserService.Object, mockLogger.Object);

        // Act
        var result = await controller.Logout(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User logged out.", okResult.Value);
        mockAuthService.Verify(s => s.LogoutAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutIdClaim_ReturnsUnauthorized()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthService>();
        var mockUserService = new Mock<IUserService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var controller = new AuthController(mockAuthService.Object, mockUserService.Object, mockLogger.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var result = await controller.GetCurrentUser();

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("User ID claim not found in token", unauthorizedResult.Value);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidIdClaim_ReturnsOkWithUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userResponse = new UserResponse { Id = userId, Username = "user", FirstName = "First", LastName = "Last", Email = "email", PhoneNumber = "phone", Role = "role", Group = 1, Sector = default };
        var mockAuthService = new Mock<IAuthService>();
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(userResponse);
        var mockLogger = new Mock<ILogger<AuthController>>();
        var controller = new AuthController(mockAuthService.Object, mockUserService.Object, mockLogger.Object);
        var identity = new ClaimsIdentity(new[] { new Claim("id", userId.ToString()) }, "TestAuth");
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };

        // Act
        var result = await controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(userResponse, okResult.Value);
        mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
    }
} 