using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.User;
using Novademy.Contracts.Responses.User;
using Novademy.Contracts.Enums;
using Xunit;

namespace Novademy.UnitTests.ControllerTests;

public class UserControllerTests
{
    [Fact]
    public async Task GetUsers_WithExistingUsers_ReturnsOk()
    {
        // Arrange
        var users = new List<UserResponse>
        {
            new UserResponse
            {
                Id = Guid.NewGuid(),
                Username = "user1",
                FirstName = "First",
                LastName = "Last",
                Email = "email@example.com",
                PhoneNumber = "1234567890",
                Role = "User",
                Group = 1,
                Sector = SectorType.Azerbaijani
            }
        };
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync(users);
        var controller = new UserController(mockUserService.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsAssignableFrom<IEnumerable<UserResponse>>(okResult.Value);
        Assert.Equal(users, returnedUsers);
        mockUserService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUsers_WithNoUsers_ReturnsNoContent()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<UserResponse>());
        var controller = new UserController(mockUserService.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetUsers();

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockUserService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserResponse
        {
            Id = userId,
            Username = "user",
            FirstName = "First",
            LastName = "Last",
            Email = "email@example.com",
            PhoneNumber = "1234567890",
            Role = "User",
            Group = 2,
            Sector = SectorType.English
        };
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.GetByIdAsync(userId)).ReturnsAsync(user);
        var controller = new UserController(mockUserService.Object);

        // Act
        var result = await controller.GetUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(user, okResult.Value);
        mockUserService.Verify(s => s.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateRequest = new UpdateUserRequest
        {
            Username = "newuser",
            FirstName = "New",
            LastName = "User",
            Email = "newemail@example.com",
            PhoneNumber = "0987654321",
            Group = 3,
            Sector = SectorType.Russian
        };
        var updatedUser = new UserResponse
        {
            Id = userId,
            Username = updateRequest.Username,
            FirstName = updateRequest.FirstName,
            LastName = updateRequest.LastName,
            Email = updateRequest.Email,
            PhoneNumber = updateRequest.PhoneNumber,
            Role = "User",
            Group = updateRequest.Group,
            Sector = updateRequest.Sector
        };
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.UpdateAsync(userId, updateRequest)).ReturnsAsync(updatedUser);
        var controller = new UserController(mockUserService.Object);

        // Act
        var result = await controller.UpdateUser(userId, updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updatedUser, okResult.Value);
        mockUserService.Verify(s => s.UpdateAsync(userId, updateRequest), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_WhenCalled_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUserService = new Mock<IUserService>();
        mockUserService.Setup(s => s.DeleteAsync(userId)).Returns(Task.CompletedTask);
        var controller = new UserController(mockUserService.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.DeleteUser(userId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockUserService.Verify(s => s.DeleteAsync(userId), Times.Once);
    }
} 