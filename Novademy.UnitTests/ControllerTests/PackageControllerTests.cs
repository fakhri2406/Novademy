using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Package;
using Novademy.Contracts.Responses.Package;
using Xunit;

namespace Novademy.UnitTests.ControllerTests;

public class PackageControllerTests
{
    [Fact]
    public async Task GetPackages_WithExistingPackages_ReturnsOk()
    {
        // Arrange
        var samplePackage = new PackageResponse
        {
            Id = Guid.NewGuid(),
            Title = "Test Package",
            Description = "Description",
            Price = 99.99m,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseIds = new List<Guid> { Guid.NewGuid() }
        };
        var packages = new List<PackageResponse> { samplePackage };
        var mockService = new Mock<IPackageService>();
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(packages);
        var controller = new PackageController(mockService.Object);

        // Act
        var result = await controller.GetPackages();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPackages = Assert.IsAssignableFrom<IEnumerable<PackageResponse>>(okResult.Value);
        Assert.Equal(packages, returnedPackages);
        mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPackages_WithNoPackages_ReturnsNoContent()
    {
        // Arrange
        var mockService = new Mock<IPackageService>();
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<PackageResponse>());
        var controller = new PackageController(mockService.Object);

        // Act
        var result = await controller.GetPackages();

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPackage_ReturnsOk()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var samplePackage = new PackageResponse
        {
            Id = packageId,
            Title = "Test Package",
            Description = "Description",
            Price = 49.99m,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };
        var mockService = new Mock<IPackageService>();
        mockService.Setup(s => s.GetByIdAsync(packageId)).ReturnsAsync(samplePackage);
        var controller = new PackageController(mockService.Object);

        // Act
        var result = await controller.GetPackage(packageId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(samplePackage, okResult.Value);
        mockService.Verify(s => s.GetByIdAsync(packageId), Times.Once);
    }

    [Fact]
    public async Task CreatePackage_WhenCalledByAdmin_ReturnsCreatedAtAction()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var createRequest = new CreatePackageRequest
        {
            Title = "New Package",
            Description = "Desc",
            Price = 19.99m,
            Image = null,
            CourseIds = new List<Guid> { Guid.NewGuid() }
        };
        var createdResponse = new PackageResponse
        {
            Id = packageId,
            Title = createRequest.Title,
            Description = createRequest.Description,
            Price = createRequest.Price,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseIds = createRequest.CourseIds
        };
        var mockService = new Mock<IPackageService>();
        mockService.Setup(s => s.CreateAsync(createRequest)).ReturnsAsync(createdResponse);
        var controller = new PackageController(mockService.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };

        // Act
        var result = await controller.CreatePackage(createRequest);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(PackageController.GetPackage), createdAtActionResult.ActionName);
        Assert.Equal(createdResponse.Id, createdAtActionResult.RouteValues["id"]);
        Assert.Equal($"Package with ID {createdResponse.Id} created successfully.", createdAtActionResult.Value);
        mockService.Verify(s => s.CreateAsync(createRequest), Times.Once);
    }

    [Fact]
    public async Task UpdatePackage_WhenCalledByAdmin_ReturnsOk()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var updateRequest = new UpdatePackageRequest
        {
            Title = "Updated Package",
            Description = "Updated Desc",
            Price = 29.99m,
            Image = null,
            CourseIds = new List<Guid> { Guid.NewGuid() }
        };
        var updatedResponse = new PackageResponse
        {
            Id = packageId,
            Title = updateRequest.Title,
            Description = updateRequest.Description,
            Price = updateRequest.Price,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseIds = updateRequest.CourseIds
        };
        var mockService = new Mock<IPackageService>();
        mockService.Setup(s => s.UpdateAsync(packageId, updateRequest)).ReturnsAsync(updatedResponse);
        var controller = new PackageController(mockService.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };

        // Act
        var result = await controller.UpdatePackage(packageId, updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updatedResponse, okResult.Value);
        mockService.Verify(s => s.UpdateAsync(packageId, updateRequest), Times.Once);
    }

    [Fact]
    public async Task DeletePackage_WhenCalledByAdmin_ReturnsNoContent()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var mockService = new Mock<IPackageService>();
        mockService.Setup(s => s.DeleteAsync(packageId)).Returns(Task.CompletedTask);
        var controller = new PackageController(mockService.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };

        // Act
        var result = await controller.DeletePackage(packageId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockService.Verify(s => s.DeleteAsync(packageId), Times.Once);
    }
} 