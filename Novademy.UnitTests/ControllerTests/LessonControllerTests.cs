using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Lesson;
using Novademy.Contracts.Responses.Lesson;
using Xunit;

namespace Novademy.UnitTests.ControllerTests;

public class LessonControllerTests
{
    [Fact]
    public async Task GetLessons_WithExistingLessons_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var sampleLesson = new LessonResponse
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Description = "Desc",
            VideoUrl = "videoUrl",
            Order = 1,
            IsFree = true,
            Transcript = "transcript",
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseId = courseId
        };
        var lessons = new List<LessonResponse> { sampleLesson };
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.GetByCourseIdAsync(courseId)).ReturnsAsync(lessons);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new LessonController(mockService.Object, mockRepo.Object);

        // Act
        var result = await controller.GetLessons(courseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedLessons = Assert.IsAssignableFrom<IEnumerable<LessonResponse>>(okResult.Value);
        Assert.Equal(lessons, returnedLessons);
        mockService.Verify(s => s.GetByCourseIdAsync(courseId), Times.Once);
    }

    [Fact]
    public async Task GetLessons_WithNoLessons_ReturnsNoContent()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.GetByCourseIdAsync(courseId)).ReturnsAsync(new List<LessonResponse>());
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new LessonController(mockService.Object, mockRepo.Object);

        // Act
        var result = await controller.GetLessons(courseId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockService.Verify(s => s.GetByCourseIdAsync(courseId), Times.Once);
    }

    [Fact]
    public async Task GetLesson_WhenLessonIsFree_ReturnsOk()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sampleLesson = new LessonResponse
        {
            Id = lessonId,
            Title = "Title",
            Description = "Desc",
            VideoUrl = "url",
            Order = 1,
            IsFree = true,
            Transcript = "trans",
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseId = Guid.NewGuid()
        };
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.GetByIdAsync(lessonId)).ReturnsAsync(sampleLesson);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new LessonController(mockService.Object, mockRepo.Object);
        // set user
        var identity = new ClaimsIdentity(new[] { new Claim("id", userId.ToString()) }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetLesson(lessonId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(sampleLesson, okResult.Value);
        mockService.Verify(s => s.GetByIdAsync(lessonId), Times.Once);
        mockRepo.Verify(r => r.HasActiveForLessonAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetLesson_WhenUserIsAdminAndLessonIsNotFree_ReturnsOk()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sampleLesson = new LessonResponse
        {
            Id = lessonId,
            Title = "Title",
            Description = "Desc",
            VideoUrl = "url",
            Order = 1,
            IsFree = false,
            Transcript = "trans",
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseId = Guid.NewGuid()
        };
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.GetByIdAsync(lessonId)).ReturnsAsync(sampleLesson);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new LessonController(mockService.Object, mockRepo.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            },
            "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetLesson(lessonId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(sampleLesson, okResult.Value);
        mockService.Verify(s => s.GetByIdAsync(lessonId), Times.Once);
        mockRepo.Verify(r => r.HasActiveForLessonAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetLesson_WhenUserHasSubscriptionAndLessonIsNotFree_ReturnsOk()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sampleLesson = new LessonResponse
        {
            Id = lessonId,
            Title = "Title",
            Description = "Desc",
            VideoUrl = "url",
            Order = 1,
            IsFree = false,
            Transcript = "trans",
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseId = Guid.NewGuid()
        };
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.GetByIdAsync(lessonId)).ReturnsAsync(sampleLesson);
        var mockRepo = new Mock<ISubscriptionRepository>();
        mockRepo.Setup(r => r.HasActiveForLessonAsync(userId, lessonId)).ReturnsAsync(true);
        var controller = new LessonController(mockService.Object, mockRepo.Object);
        // set user
        var identity = new ClaimsIdentity(new[] { new Claim("id", userId.ToString()) }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetLesson(lessonId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(sampleLesson, okResult.Value);
        mockService.Verify(s => s.GetByIdAsync(lessonId), Times.Once);
        mockRepo.Verify(r => r.HasActiveForLessonAsync(userId, lessonId), Times.Once);
    }

    [Fact]
    public async Task GetLesson_WhenUserHasNoSubscriptionAndLessonIsNotFree_ReturnsForbid()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sampleLesson = new LessonResponse
        {
            Id = lessonId,
            Title = "Title",
            Description = "Desc",
            VideoUrl = "url",
            Order = 1,
            IsFree = false,
            Transcript = "trans",
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseId = Guid.NewGuid()
        };
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.GetByIdAsync(lessonId)).ReturnsAsync(sampleLesson);
        var mockRepo = new Mock<ISubscriptionRepository>();
        mockRepo.Setup(r => r.HasActiveForLessonAsync(userId, lessonId)).ReturnsAsync(false);
        var controller = new LessonController(mockService.Object, mockRepo.Object);
        // set user
        var identity = new ClaimsIdentity(new[] { new Claim("id", userId.ToString()) }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetLesson(lessonId);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.Contains("You do not have access to this lesson.", forbidResult.AuthenticationSchemes);
        mockService.Verify(s => s.GetByIdAsync(lessonId), Times.Once);
        mockRepo.Verify(r => r.HasActiveForLessonAsync(userId, lessonId), Times.Once);
    }

    [Fact]
    public async Task CreateLesson_WhenCalledByTeacher_ReturnsCreatedAtAction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var videoMock = new Mock<IFormFile>().Object;
        var createRequest = new CreateLessonRequest
        {
            Title = "Title",
            Description = "Desc",
            Video = videoMock,
            Order = 1,
            IsFree = false,
            Transcript = "trans",
            Image = null,
            CourseId = courseId
        };
        var createdResponse = new LessonResponse
        {
            Id = Guid.NewGuid(),
            Title = createRequest.Title,
            Description = createRequest.Description,
            VideoUrl = "url",
            Order = createRequest.Order,
            IsFree = createRequest.IsFree,
            Transcript = createRequest.Transcript,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseId = courseId
        };
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.CreateAsync(createRequest)).ReturnsAsync(createdResponse);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new LessonController(mockService.Object, mockRepo.Object);
        // set user as Teacher
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Role, "Teacher")
            },
            "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.CreateLesson(createRequest);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(LessonController.GetLesson), createdAtActionResult.ActionName);
        Assert.Equal(createdResponse.Id, createdAtActionResult.RouteValues["id"]);
        Assert.Equal($"Lesson with ID {createdResponse.Id} created successfully for Course {createdResponse.CourseId}.", createdAtActionResult.Value);
        mockService.Verify(s => s.CreateAsync(createRequest), Times.Once);
    }

    [Fact]
    public async Task UpdateLesson_WhenCalled_ReturnsOk()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var videoMock = new Mock<IFormFile>().Object;
        var updateRequest = new UpdateLessonRequest
        {
            Title = "Title",
            Description = "Desc",
            Video = videoMock,
            Order = 1,
            Transcript = "trans",
            Image = null
        };
        var updatedResponse = new LessonResponse
        {
            Id = lessonId,
            Title = updateRequest.Title,
            Description = updateRequest.Description,
            VideoUrl = "url",
            Order = updateRequest.Order,
            IsFree = false,
            Transcript = updateRequest.Transcript,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseId = Guid.NewGuid()
        };
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.UpdateAsync(lessonId, updateRequest)).ReturnsAsync(updatedResponse);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new LessonController(mockService.Object, mockRepo.Object);
        // set user as Teacher
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Role, "Teacher")
            },
            "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.UpdateLesson(lessonId, updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updatedResponse, okResult.Value);
        mockService.Verify(s => s.UpdateAsync(lessonId, updateRequest), Times.Once);
    }

    [Fact]
    public async Task DeleteLesson_WhenCalled_ReturnsNoContent()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var mockService = new Mock<ILessonService>();
        mockService.Setup(s => s.DeleteAsync(lessonId)).Returns(Task.CompletedTask);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new LessonController(mockService.Object, mockRepo.Object);
        // set user as Teacher
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Role, "Teacher")
            },
            "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.DeleteLesson(lessonId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockService.Verify(s => s.DeleteAsync(lessonId), Times.Once);
    }
} 