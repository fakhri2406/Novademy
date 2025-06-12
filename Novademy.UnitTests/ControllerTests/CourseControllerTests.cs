using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Enums;
using Novademy.Contracts.Requests.Course;
using Novademy.Contracts.Responses.Course;
using Xunit;

namespace Novademy.UnitTests.ControllerTests;

public class CourseControllerTests
{
    [Fact]
    public async Task GetCourses_WithExistingCourses_ReturnsOk()
    {
        // Arrange
        var sampleCourse = new CourseResponse
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Description = "Desc",
            Subject = SubjectType.Math,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var courses = new List<CourseResponse> { sampleCourse };
        var mockService = new Mock<ICourseService>();
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(courses);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new CourseController(mockService.Object, mockRepo.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetCourses();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCourses = Assert.IsAssignableFrom<IEnumerable<CourseResponse>>(okResult.Value);
        Assert.Equal(courses, returnedCourses);
        mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCourses_WithNoCourses_ReturnsNoContent()
    {
        // Arrange
        var mockService = new Mock<ICourseService>();
        mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<CourseResponse>());
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new CourseController(mockService.Object, mockRepo.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetCourses();

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockService.Verify(s => s.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCourse_WhenUserIsAdmin_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sampleCourse = new CourseResponse
        {
            Id = courseId,
            Title = "Title",
            Description = "Desc",
            Subject = SubjectType.Biology,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var mockService = new Mock<ICourseService>();
        mockService.Setup(s => s.GetByIdAsync(courseId)).ReturnsAsync(sampleCourse);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new CourseController(mockService.Object, mockRepo.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetCourse(courseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(sampleCourse, okResult.Value);
        mockService.Verify(s => s.GetByIdAsync(courseId), Times.Once);
        mockRepo.Verify(r => r.HasActiveForCourseAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetCourse_WhenUserIsTeacher_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sampleCourse = new CourseResponse
        {
            Id = courseId,
            Title = "Title",
            Description = "Desc",
            Subject = SubjectType.History,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var mockService = new Mock<ICourseService>();
        mockService.Setup(s => s.GetByIdAsync(courseId)).ReturnsAsync(sampleCourse);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new CourseController(mockService.Object, mockRepo.Object);
        // set user as Teacher
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim("id", userId.ToString()),
                new Claim(ClaimTypes.Role, "Teacher")
            }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetCourse(courseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(sampleCourse, okResult.Value);
        mockRepo.Verify(r => r.HasActiveForCourseAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
        mockService.Verify(s => s.GetByIdAsync(courseId), Times.Once);
    }

    [Fact]
    public async Task GetCourse_WhenUserHasSubscription_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sampleCourse = new CourseResponse
        {
            Id = courseId,
            Title = "Title",
            Description = "Desc",
            Subject = SubjectType.Physics,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var mockService = new Mock<ICourseService>();
        mockService.Setup(s => s.GetByIdAsync(courseId)).ReturnsAsync(sampleCourse);
        var mockRepo = new Mock<ISubscriptionRepository>();
        mockRepo.Setup(r => r.HasActiveForCourseAsync(userId, courseId)).ReturnsAsync(true);
        var controller = new CourseController(mockService.Object, mockRepo.Object);
        // set user
        var identity = new ClaimsIdentity(new[] { new Claim("id", userId.ToString()) }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetCourse(courseId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(sampleCourse, okResult.Value);
        mockRepo.Verify(r => r.HasActiveForCourseAsync(userId, courseId), Times.Once);
        mockService.Verify(s => s.GetByIdAsync(courseId), Times.Once);
    }

    [Fact]
    public async Task GetCourse_WhenUserHasNoSubscription_ReturnsForbid()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sampleCourse = new CourseResponse
        {
            Id = courseId,
            Title = "Title",
            Description = "Desc",
            Subject = SubjectType.Geography,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var mockService = new Mock<ICourseService>();
        mockService.Setup(s => s.GetByIdAsync(courseId)).ReturnsAsync(sampleCourse);
        var mockRepo = new Mock<ISubscriptionRepository>();
        mockRepo.Setup(r => r.HasActiveForCourseAsync(userId, courseId)).ReturnsAsync(false);
        var controller = new CourseController(mockService.Object, mockRepo.Object);
        // set user
        var identity = new ClaimsIdentity(new[] { new Claim("id", userId.ToString()) }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.GetCourse(courseId);

        // Assert
        var forbidResult = Assert.IsType<ForbidResult>(result);
        Assert.Contains("You do not have access to this course.", forbidResult.AuthenticationSchemes);
        mockRepo.Verify(r => r.HasActiveForCourseAsync(userId, courseId), Times.Once);
        mockService.Verify(s => s.GetByIdAsync(courseId), Times.Never);
    }

    [Fact]
    public async Task CreateCourse_WhenCalled_ReturnsCreatedAtAction()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var createRequest = new CreateCourseRequest
        {
            Title = "Title",
            Description = "Desc",
            Subject = SubjectType.Math,
            Image = null
        };
        var createdResponse = new CourseResponse
        {
            Id = courseId,
            Title = "Title",
            Description = "Desc",
            Subject = SubjectType.Math,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var mockService = new Mock<ICourseService>();
        mockService.Setup(s => s.CreateAsync(createRequest)).ReturnsAsync(createdResponse);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new CourseController(mockService.Object, mockRepo.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.CreateCourse(createRequest);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(CourseController.GetCourse), createdAtActionResult.ActionName);
        Assert.Equal(createdResponse.Id, createdAtActionResult.RouteValues["id"]);
        Assert.Equal($"Course with ID {createdResponse.Id} created successfully.", createdAtActionResult.Value);
        mockService.Verify(s => s.CreateAsync(createRequest), Times.Once);
    }

    [Fact]
    public async Task UpdateCourse_WhenCalled_ReturnsOk()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var updateRequest = new UpdateCourseRequest
        {
            Title = "New Title",
            Description = "New Desc",
            Subject = SubjectType.Biology,
            Image = null
        };
        var updatedResponse = new CourseResponse
        {
            Id = courseId,
            Title = "New Title",
            Description = "New Desc",
            Subject = SubjectType.Biology,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var mockService = new Mock<ICourseService>();
        mockService.Setup(s => s.UpdateAsync(courseId, updateRequest)).ReturnsAsync(updatedResponse);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new CourseController(mockService.Object, mockRepo.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.UpdateCourse(courseId, updateRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(updatedResponse, okResult.Value);
        mockService.Verify(s => s.UpdateAsync(courseId, updateRequest), Times.Once);
    }

    [Fact]
    public async Task DeleteCourse_WhenCalled_ReturnsNoContent()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var mockService = new Mock<ICourseService>();
        mockService.Setup(s => s.DeleteAsync(courseId)).Returns(Task.CompletedTask);
        var mockRepo = new Mock<ISubscriptionRepository>();
        var controller = new CourseController(mockService.Object, mockRepo.Object);
        // set user as Admin
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        // Act
        var result = await controller.DeleteCourse(courseId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockService.Verify(s => s.DeleteAsync(courseId), Times.Once);
    }
} 