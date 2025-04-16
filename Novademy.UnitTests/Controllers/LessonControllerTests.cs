using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.API.Mapping;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Validators.Lesson;
using Novademy.Contracts.Requests.Lesson;
using Novademy.Contracts.Responses.Lesson;
using System.Security.Claims;
using Xunit;

namespace Novademy.UnitTests.Controllers;

public class LessonControllerTests
{
    private readonly Mock<ILessonRepository> _repoMock;
    private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock;
    private readonly IValidator<CreateLessonRequest> _createValidator;
    private readonly IValidator<UpdateLessonRequest> _updateValidator;
    private readonly LessonController _controller;
    
    public LessonControllerTests()
    {
        _repoMock = new Mock<ILessonRepository>();
        _subscriptionRepoMock = new Mock<ISubscriptionRepository>();
        _createValidator = new CreateLessonRequestValidator();
        _updateValidator = new UpdateLessonRequestValidator();
        
        _controller = new LessonController(
            _repoMock.Object,
            _subscriptionRepoMock.Object,
            _createValidator,
            _updateValidator);
    }
    
    #region GetLessons Tests
    
    [Fact]
    public async Task GetLessons_WithLessons_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var lessons = new List<Lesson>
        {
            new Lesson { Id = Guid.NewGuid(), Title = "Lesson 1", Description = "Desc 1", CourseId = courseId, VideoUrl = "url", Order = 1 }
        };
        _repoMock.Setup(r => r.GetLessonsByCourseIdAsync(courseId)).ReturnsAsync(lessons);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetLessons(courseId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responses = Assert.IsAssignableFrom<IEnumerable<LessonResponse>>(okResult.Value);
        Assert.Single(responses);
        Assert.Equal("Lesson 1", responses.First().Title);
    }
    
    [Fact]
    public async Task GetLessons_NoLessons_ReturnsNoContent()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetLessonsByCourseIdAsync(courseId)).ReturnsAsync(new List<Lesson>());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetLessons(courseId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task GetLessons_InvalidCourseId_ReturnsNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetLessonsByCourseIdAsync(courseId)).ThrowsAsync(new KeyNotFoundException("Invalid Course ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetLessons(courseId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Course ID.", notFoundResult.Value);
    }
    
    #endregion
    
    #region GetLesson Tests
    
    [Fact]
    public async Task GetLesson_Admin_ReturnsOk()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lesson = new Lesson { Id = lessonId, Title = "Lesson 1", Description = "Desc 1", VideoUrl = "url", Order = 1, CourseId = Guid.NewGuid() };
        _repoMock.Setup(r => r.GetLessonByIdAsync(lessonId)).ReturnsAsync(lesson);
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
        var result = await _controller.GetLesson(lessonId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LessonResponse>(okResult.Value);
        Assert.Equal(lessonId, response.Id);
    }
    
    [Fact]
    public async Task GetLesson_StudentWithSubscription_ReturnsOk()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lesson = new Lesson { Id = lessonId, Title = "Lesson 1", Description = "Desc 1", VideoUrl = "url", Order = 1, CourseId = Guid.NewGuid(), IsFree = false };
        _repoMock.Setup(r => r.GetLessonByIdAsync(lessonId)).ReturnsAsync(lesson);
        _subscriptionRepoMock.Setup(s => s.HasActiveSubscriptionForLessonAsync(userId, lessonId)).ReturnsAsync(true);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim("id", userId.ToString())
                }))
            }
        };
        
        // Act
        var result = await _controller.GetLesson(lessonId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LessonResponse>(okResult.Value);
        Assert.Equal(lessonId, response.Id);
    }
    
    [Fact]
    public async Task GetLesson_StudentNoSubscription_ReturnsForbidden()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lesson = new Lesson { Id = lessonId, Title = "Lesson 1", Description = "Desc 1", VideoUrl = "url", Order = 1, CourseId = Guid.NewGuid(), IsFree = false };
        _repoMock.Setup(r => r.GetLessonByIdAsync(lessonId)).ReturnsAsync(lesson);
        _subscriptionRepoMock.Setup(s => s.HasActiveSubscriptionForLessonAsync(userId, lessonId)).ReturnsAsync(false);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim("id", userId.ToString())
                }))
            }
        };
        
        // Act
        var result = await _controller.GetLesson(lessonId);
        
        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.Equal("You do not have access to this lesson.", forbidResult.AuthenticationSchemes.FirstOrDefault());
    }
    
    [Fact]
    public async Task GetLesson_LessonNotFound_ReturnsNotFound()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetLessonByIdAsync(lessonId)).ThrowsAsync(new KeyNotFoundException("Invalid Lesson ID."));
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
        var result = await _controller.GetLesson(lessonId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Lesson ID.", notFoundResult.Value);
    }
    
    #endregion
    
    #region CreateLesson Tests
    
    [Fact]
    public async Task CreateLesson_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var videoMock = new Mock<IFormFile>();
        var request = new CreateLessonRequest
        {
            Title = "New Lesson",
            Description = "New Desc",
            Video = videoMock.Object,
            Order = 1,
            IsFree = false,
            Transcript = "Transcript",
            Image = null,
            CourseId = courseId
        };
        var lesson = request.MapToLesson();
        var createdLesson = new Lesson { Id = Guid.NewGuid(), Title = "New Lesson", Description = "New Desc", VideoUrl = "video_url", Order = 1, Transcript = "Transcript", CourseId = courseId };
        _repoMock.Setup(r => r.CreateLessonAsync(It.Is<Lesson>(l => l.Title == "New Lesson"), videoMock.Object, null)).ReturnsAsync(createdLesson);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }))
            }
        };
        
        // Act
        var result = await _controller.CreateLesson(request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetLesson), createdResult.ActionName);
        Assert.NotNull(createdResult.RouteValues);
        Assert.Equal(createdLesson.Id, createdResult.RouteValues["id"]);
        Assert.Equal($"Lesson with ID {createdLesson.Id} created successfully for Course {createdLesson.CourseId}.", createdResult.Value);
    }
    
    [Fact]
    public async Task CreateLesson_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var videoMock = new Mock<IFormFile>();
        var request = new CreateLessonRequest
        {
            Title = "", // Invalid: empty
            Description = "New Desc",
            Video = videoMock.Object,
            Order = 1,
            IsFree = false,
            Transcript = "Transcript",
            Image = null,
            CourseId = courseId
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }))
            }
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _controller.CreateLesson(request));
        Assert.Contains("Title is required.", exception.Message);
    }
    
    #endregion
    
    #region UpdateLesson Tests
    
    [Fact]
    public async Task UpdateLesson_ValidRequest_ReturnsOk()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var videoMock = new Mock<IFormFile>();
        var request = new UpdateLessonRequest
        {
            Title = "Updated Lesson",
            Description = "Updated Desc",
            Video = videoMock.Object,
            Order = 2,
            Transcript = "Updated Transcript",
            Image = null
        };
        var existingLesson = new Lesson { Id = lessonId, Title = "Old Lesson", Description = "Old Desc", VideoUrl = "old_url", Order = 1, Transcript = "Old Transcript", CourseId = Guid.NewGuid() };
        var updatedLesson = new Lesson { Id = lessonId, Title = "Updated Lesson", Description = "Updated Desc", VideoUrl = "new_url", Order = 2, Transcript = "Updated Transcript", CourseId = existingLesson.CourseId };
        _repoMock.Setup(r => r.GetLessonByIdAsync(lessonId)).ReturnsAsync(existingLesson);
        _repoMock.Setup(r => r.UpdateLessonAsync(It.Is<Lesson>(l => l.Title == "Updated Lesson"), videoMock.Object, null)).ReturnsAsync(updatedLesson);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }))
            }
        };
        
        // Act
        var result = await _controller.UpdateLesson(lessonId, request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LessonResponse>(okResult.Value);
        Assert.Equal("Updated Lesson", response.Title);
        Assert.True(response.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
    }
    
    [Fact]
    public async Task UpdateLesson_LessonNotFound_ReturnsNotFound()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var videoMock = new Mock<IFormFile>();
        var request = new UpdateLessonRequest
        {
            Title = "Updated Lesson",
            Description = "Updated Desc",
            Video = videoMock.Object,
            Order = 2,
            Transcript = "Updated Transcript",
            Image = null
        };
        _repoMock.Setup(r => r.GetLessonByIdAsync(lessonId)).ThrowsAsync(new KeyNotFoundException("Invalid Lesson ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }))
            }
        };
        
        // Act
        var result = await _controller.UpdateLesson(lessonId, request);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Lesson ID.", notFoundResult.Value);
    }
    
    [Fact]
    public async Task UpdateLesson_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var videoMock = new Mock<IFormFile>();
        var request = new UpdateLessonRequest
        {
            Title = "Updated Lesson",
            Description = "", // Invalid: empty
            Video = videoMock.Object,
            Order = 2,
            Transcript = "Updated Transcript",
            Image = null
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }))
            }
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _controller.UpdateLesson(lessonId, request));
        Assert.Contains("Description is required.", exception.Message);
    }
    
    #endregion
    
    #region DeleteLesson Tests
    
    [Fact]
    public async Task DeleteLesson_ValidId_ReturnsNoContent()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteLessonAsync(lessonId)).Returns(Task.CompletedTask);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }))
            }
        };
        
        // Act
        var result = await _controller.DeleteLesson(lessonId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task DeleteLesson_LessonNotFound_ReturnsNotFound()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteLessonAsync(lessonId)).ThrowsAsync(new KeyNotFoundException("Invalid Lesson ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }))
            }
        };
        
        // Act
        var result = await _controller.DeleteLesson(lessonId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Lesson ID.", notFoundResult.Value);
    }
    
    #endregion
}