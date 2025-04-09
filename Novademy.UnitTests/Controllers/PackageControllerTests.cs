using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.API.Mapping;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Validators.Package;
using Novademy.Contracts.Requests.Package;
using Novademy.Contracts.Responses.Package;
using System.Security.Claims;
using Novademy.Contracts.Enums;
using Xunit;

namespace Novademy.UnitTests.Controllers;

public class PackageControllerTests
{
    private readonly Mock<IPackageRepository> _repoMock;
    private readonly Mock<ICourseRepository> _courseRepoMock;
    private readonly IValidator<CreatePackageRequest> _createValidator;
    private readonly IValidator<UpdatePackageRequest> _updateValidator;
    private readonly PackageController _controller;
    
    public PackageControllerTests()
    {
        _repoMock = new Mock<IPackageRepository>();
        _courseRepoMock = new Mock<ICourseRepository>();
        _createValidator = new CreatePackageRequestValidator();
        _updateValidator = new UpdatePackageRequestValidator();
        
        _controller = new PackageController(
            _repoMock.Object,
            _courseRepoMock.Object,
            _createValidator,
            _updateValidator);
    }
    
    #region GetPackages Tests
    
    [Fact]
    public async Task GetPackages_WithPackages_ReturnsOk()
    {
        // Arrange
        var packages = new List<Package>
        {
            new Package { Id = Guid.NewGuid(), Title = "Package 1", Description = "Desc 1", Price = 100m }
        };
        _repoMock.Setup(r => r.GetAllPackagesAsync()).ReturnsAsync(packages);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetPackages();
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responses = Assert.IsAssignableFrom<IEnumerable<PackageResponse>>(okResult.Value);
        Assert.Single(responses);
        Assert.Equal("Package 1", responses.First().Title);
    }
    
    [Fact]
    public async Task GetPackages_NoPackages_ReturnsNoContent()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllPackagesAsync()).ReturnsAsync(new List<Package>());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetPackages();
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    #endregion
    
    #region GetPackage Tests
    
    [Fact]
    public async Task GetPackage_ValidId_ReturnsOk()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = new Package { Id = packageId, Title = "Package 1", Description = "Desc 1", Price = 100m };
        _repoMock.Setup(r => r.GetPackageByIdAsync(packageId)).ReturnsAsync(package);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetPackage(packageId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PackageResponse>(okResult.Value);
        Assert.Equal(packageId, response.Id);
    }
    
    [Fact]
    public async Task GetPackage_PackageNotFound_ReturnsNotFound()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetPackageByIdAsync(packageId)).ThrowsAsync(new KeyNotFoundException("Invalid Package ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetPackage(packageId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Package ID.", notFoundResult.Value);
    }
    
    #endregion
    
    #region CreatePackage Tests
    
    [Fact]
    public async Task CreatePackage_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new CreatePackageRequest
        {
            Title = "New Package",
            Description = "New Description Here",
            Price = 150m,
            Image = null,
            CourseIds = new List<Guid> { courseId }
        };
        var course = new Course { Id = courseId, Title = "Course 1", Description = "Desc 1", Subject = SubjectType.Math };
        var package = request.MapToPackage();
        var createdPackage = new Package { Id = Guid.NewGuid(), Title = "New Package", Description = "New Description Here", Price = 150m, Courses = new List<Course> { course } };
        _courseRepoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ReturnsAsync(course);
        _repoMock.Setup(r => r.CreatePackageAsync(It.Is<Package>(p => p.Title == "New Package"), null)).ReturnsAsync(createdPackage);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.CreatePackage(request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetPackage), createdResult.ActionName);
        Assert.Equal(createdPackage.Id, createdResult.RouteValues["id"]);
        Assert.Equal($"Package with ID {createdPackage.Id} created successfully.", createdResult.Value);
    }
    
    [Fact]
    public async Task CreatePackage_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var request = new CreatePackageRequest
        {
            Title = "", // Invalid: empty
            Description = "New Description Here",
            Price = 150m,
            Image = null,
            CourseIds = new List<Guid> { Guid.NewGuid() }
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _controller.CreatePackage(request));
        Assert.Contains("Title is required.", exception.Message);
    }
    
    [Fact]
    public async Task CreatePackage_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new CreatePackageRequest
        {
            Title = "New Package",
            Description = "New Description Here",
            Price = 150m,
            Image = null,
            CourseIds = new List<Guid> { courseId }
        };
        _courseRepoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ThrowsAsync(new KeyNotFoundException("Invalid Course ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.CreatePackage(request);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Course ID.", notFoundResult.Value);
    }
    
    #endregion
    
    #region UpdatePackage Tests
    
    [Fact]
    public async Task UpdatePackage_ValidRequest_ReturnsOk()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var request = new UpdatePackageRequest
        {
            Title = "Updated Package",
            Description = "Updated Description Here",
            Price = 200m,
            Image = null,
            CourseIds = new List<Guid> { courseId }
        };
        var existingPackage = new Package { Id = packageId, Title = "Old Package", Description = "Old Desc", Price = 100m, Courses = new List<Course>() };
        var course = new Course { Id = courseId, Title = "Course 1", Description = "Desc 1", Subject = SubjectType.Math };
        var updatedPackage = new Package { Id = packageId, Title = "Updated Package", Description = "Updated Description Here", Price = 200m, Courses = new List<Course> { course } };
        _repoMock.Setup(r => r.GetPackageByIdAsync(packageId)).ReturnsAsync(existingPackage);
        _courseRepoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ReturnsAsync(course);
        _repoMock.Setup(r => r.UpdatePackageAsync(It.Is<Package>(p => p.Title == "Updated Package"), null)).ReturnsAsync(updatedPackage);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.UpdatePackage(packageId, request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PackageResponse>(okResult.Value);
        Assert.Equal("Updated Package", response.Title);
        Assert.True(response.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Fact]
    public async Task UpdatePackage_PackageNotFound_ReturnsNotFound()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var request = new UpdatePackageRequest
        {
            Title = "Updated Package",
            Description = "Updated Description Here",
            Price = 200m,
            Image = null,
            CourseIds = new List<Guid> { Guid.NewGuid() }
        };
        _repoMock.Setup(r => r.GetPackageByIdAsync(packageId)).ThrowsAsync(new KeyNotFoundException("Invalid Package ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.UpdatePackage(packageId, request);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Package ID.", notFoundResult.Value);
    }
    
    [Fact]
    public async Task UpdatePackage_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var request = new UpdatePackageRequest
        {
            Title = "Up", // Invalid: too short
            Description = "Updated Description Here",
            Price = 200m,
            Image = null,
            CourseIds = new List<Guid> { Guid.NewGuid() }
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _controller.UpdatePackage(packageId, request));
        Assert.Contains("Title must be between 5 and 100 characters.", exception.Message);
    }
    
    [Fact]
    public async Task UpdatePackage_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var request = new UpdatePackageRequest
        {
            Title = "Updated Package",
            Description = "Updated Description Here",
            Price = 200m,
            Image = null,
            CourseIds = new List<Guid> { courseId }
        };
        var existingPackage = new Package { Id = packageId, Title = "Old Package", Description = "Old Desc", Price = 100m, Courses = new List<Course>() };
        _repoMock.Setup(r => r.GetPackageByIdAsync(packageId)).ReturnsAsync(existingPackage);
        _courseRepoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ThrowsAsync(new KeyNotFoundException("Invalid Course ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.UpdatePackage(packageId, request);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Course ID.", notFoundResult.Value);
    }
    
    #endregion
    
    #region DeletePackage Tests
    
    [Fact]
    public async Task DeletePackage_ValidId_ReturnsNoContent()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        _repoMock.Setup(r => r.DeletePackageAsync(packageId)).Returns(Task.CompletedTask);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.DeletePackage(packageId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task DeletePackage_PackageNotFound_ReturnsNotFound()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        _repoMock.Setup(r => r.DeletePackageAsync(packageId)).ThrowsAsync(new KeyNotFoundException("Invalid Package ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.DeletePackage(packageId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Package ID.", notFoundResult.Value);
    }
    
    #endregion
}