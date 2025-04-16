using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.API.Mapping;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Enums;
using Novademy.Contracts.Requests.Course;
using Novademy.Contracts.Responses.Course;
using System.Security.Claims;
using Novademy.Application.Validators.Course;
using Xunit;

namespace Novademy.UnitTests.Controllers;

public class CourseControllerTests
{
    private readonly Mock<ICourseRepository> _repoMock;
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock;
    private readonly IValidator<CreateCourseRequest> _createValidator;
    private readonly IValidator<UpdateCourseRequest> _updateValidator;
    private readonly CourseController _controller;
    
    public CourseControllerTests()
    {
        _repoMock = new Mock<ICourseRepository>();
        _subscriptionRepoMock = new Mock<ISubscriptionRepository>();
        _createValidator = new CreateCourseRequestValidator();
        _updateValidator = new UpdateCourseRequestValidator();
        
        _controller = new CourseController(
            _repoMock.Object,
            _subscriptionRepoMock.Object,
            _createValidator,
            _updateValidator);
    }
    
    #region GET Tests
    
    [Fact]
    public async Task GetCourses_WithCourses_ReturnsOk()
    {
        // Arrange
        var courses = new List<Course>
        {
            new Course { Id = Guid.NewGuid(), Title = "Course 1", Description = "Desc 1", Subject = SubjectType.Math }
        };
        _repoMock.Setup(r => r.GetAllCoursesAsync()).ReturnsAsync(courses);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };

        // Act
        var result = await _controller.GetCourses();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responses = Assert.IsAssignableFrom<IEnumerable<CourseResponse>>(okResult.Value);
        Assert.Single(responses);
        Assert.Equal("Course 1", responses.First().Title);
    }
    
    [Fact]
    public async Task GetCourses_NoCourses_ReturnsNoContent()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllCoursesAsync()).ReturnsAsync(new List<Course>());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };

        // Act
        var result = await _controller.GetCourses();

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    #endregion
    
    #region GetCourse Tests
    
    [Fact]
    public async Task GetCourse_Admin_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var course = new Course { Id = courseId, Title = "Course 1", Description = "Desc 1", Subject = SubjectType.Math };
        _repoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ReturnsAsync(course);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("id", userId.ToString())
                }))
            }
        };
    
        // Act
        var result = await _controller.GetCourse(courseId);
    
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CourseResponse>(okResult.Value);
        Assert.Equal(courseId, response.Id);
    }
    
    [Fact]
    public async Task GetCourse_StudentWithSubscription_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var course = new Course { Id = courseId, Title = "Course 1", Description = "Desc 1", Subject = SubjectType.Math };
        _repoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ReturnsAsync(course);
        _subscriptionRepoMock.Setup(s => s.HasActiveSubscriptionForCourseAsync(userId, courseId)).ReturnsAsync(true);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("id", userId.ToString()) })) }
        };
        
        // Act
        var result = await _controller.GetCourse(courseId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CourseResponse>(okResult.Value);
        Assert.Equal(courseId, response.Id);
    }
    
    [Fact]
    public async Task GetCourse_StudentNoSubscription_ReturnsForbidden()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var course = new Course { Id = courseId, Title = "Course 1", Description = "Desc 1", Subject = SubjectType.Math };
        _repoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ReturnsAsync(course);
        _subscriptionRepoMock.Setup(s => s.HasActiveSubscriptionForCourseAsync(userId, courseId)).ReturnsAsync(false);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("id", userId.ToString()) })) }
        };
        
        // Act
        var result = await _controller.GetCourse(courseId);
        
        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.Equal("You do not have access to this course.", forbidResult.AuthenticationSchemes.FirstOrDefault());
    }
    
    [Fact]
    public async Task GetCourse_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ThrowsAsync(new KeyNotFoundException("Invalid Course ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetCourse(courseId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Course ID.", notFoundResult.Value);
    }
    
    #endregion
    
    #region CreateCourse Tests
    
    [Fact]
    public async Task CreateCourse_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new CreateCourseRequest
        {
            Title = "New Course",
            Description = "New Desc",
            Subject = SubjectType.Math,
            Image = null
        };
        var course = request.MapToCourse();
        var createdCourse = new Course { Id = Guid.NewGuid(), Title = "New Course", Description = "New Desc", Subject = SubjectType.Math };
        _repoMock.Setup(r => r.CreateCourseAsync(It.Is<Course>(c => c.Title == "New Course"), null)).ReturnsAsync(createdCourse);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };

        // Act
        var result = await _controller.CreateCourse(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetCourse), createdResult.ActionName);
        Assert.NotNull(createdResult.RouteValues);
        Assert.Equal(createdCourse.Id, createdResult.RouteValues["id"]);
        Assert.Equal($"Course with ID {createdCourse.Id} created successfully.", createdResult.Value);
    }
    
    #endregion
    
    #region UpdateCourse Tests
    
    [Fact]
    public async Task UpdateCourse_ValidRequest_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new UpdateCourseRequest
        {
            Title = "Updated Course",
            Description = "Updated Desc",
            Subject = SubjectType.Math,
            Image = null
        };
        var existingCourse = new Course { Id = courseId, Title = "Old Course", Description = "Old Desc", Subject = SubjectType.Math };
        var updatedCourse = new Course { Id = courseId, Title = "Updated Course", Description = "Updated Desc", Subject = SubjectType.Math }; 
        _repoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ReturnsAsync(existingCourse); 
        _repoMock.Setup(r => r.UpdateCourseAsync(It.Is<Course>(c => c.Title == "Updated Course"), null)).ReturnsAsync(updatedCourse);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.UpdateCourse(courseId, request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<CourseResponse>(okResult.Value);
        Assert.Equal("Updated Course", response.Title);
        Assert.True(response.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Fact]
    public async Task UpdateCourse_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var request = new UpdateCourseRequest
        {
            Title = "Updated Course",
            Description = "Updated Desc",
            Subject = SubjectType.Math,
            Image = null
        };
        _repoMock.Setup(r => r.GetCourseByIdAsync(courseId)).ThrowsAsync(new KeyNotFoundException("Invalid Course ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.UpdateCourse(courseId, request);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Course ID.", notFoundResult.Value);
    }
    
    #endregion
    
    #region DeleteCourse Tests
    
    [Fact]
    public async Task DeleteCourse_ValidId_ReturnsNoContent()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteCourseAsync(courseId)).Returns(Task.CompletedTask);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.DeleteCourse(courseId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task DeleteCourse_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteCourseAsync(courseId)).ThrowsAsync(new KeyNotFoundException("Invalid Course ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.DeleteCourse(courseId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Course ID.", notFoundResult.Value);
    }
    
    #endregion
}