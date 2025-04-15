using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.API.Mapping;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Validators.Quiz;
using Novademy.Contracts.Requests.Quiz;
using System.Security.Claims;
using Xunit;

namespace Novademy.UnitTests.Controllers;

public class QuizControllerTests
{
    private readonly Mock<IQuizRepository> _quizRepoMock;
    private readonly Mock<IQuestionRepository> _questionRepoMock;
    private readonly Mock<IAnswerRepository> _answerRepoMock;
    private readonly IValidator<CreateQuizRequest> _createQuizValidator;
    private readonly IValidator<UpdateQuizRequest> _updateQuizValidator;
    private readonly IValidator<CreateQuestionRequest> _createQuestionValidator;
    private readonly IValidator<CreateAnswerRequest> _createAnswerValidator;
    private readonly QuizController _controller;
    
    public QuizControllerTests()
    {
        _quizRepoMock = new Mock<IQuizRepository>();
        _questionRepoMock = new Mock<IQuestionRepository>();
        _answerRepoMock = new Mock<IAnswerRepository>();
        _createQuizValidator = new CreateQuizRequestValidator();
        _updateQuizValidator = new UpdateQuizRequestValidator();
        _createQuestionValidator = new CreateQuestionRequestValidator();
        _createAnswerValidator = new CreateAnswerRequestValidator();
        
        _controller = new QuizController(
            _quizRepoMock.Object,
            _questionRepoMock.Object,
            _answerRepoMock.Object,
            _createQuizValidator,
            _updateQuizValidator,
            _createQuestionValidator,
            _createAnswerValidator);
    }
    
    #region GetQuizzes Tests
    
    [Fact]
    public async Task GetQuizzes_WithQuizzes_ReturnsOk()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var quizzes = new List<Quiz>
        {
            new Quiz { Id = Guid.NewGuid(), Title = "Quiz 1", LessonId = lessonId }
        };
        _quizRepoMock.Setup(r => r.GetQuizzesByLessonIdAsync(lessonId)).ReturnsAsync(quizzes);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetQuizzes(lessonId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responses = Assert.IsAssignableFrom<IEnumerable<Quiz>>(okResult.Value);
        Assert.Single(responses);
        Assert.Equal("Quiz 1", responses.First().Title);
    }
    
    [Fact]
    public async Task GetQuizzes_NoQuizzes_ReturnsNoContent()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        _quizRepoMock.Setup(r => r.GetQuizzesByLessonIdAsync(lessonId)).ReturnsAsync(new List<Quiz>());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetQuizzes(lessonId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    #endregion
    
    #region GetQuiz Tests
    
    [Fact]
    public async Task GetQuiz_ValidId_ReturnsOk()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var quiz = new Quiz { Id = quizId, Title = "Quiz 1", LessonId = Guid.NewGuid() };
        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId)).ReturnsAsync(quiz);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetQuiz(quizId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<Quiz>(okResult.Value);
        Assert.Equal(quizId, response.Id);
    }
    
    [Fact]
    public async Task GetQuiz_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId)).ThrowsAsync(new KeyNotFoundException("Invalid Quiz ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetQuiz(quizId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Quiz ID.", notFoundResult.Value);
    }
    
    #endregion
    
    #region CreateQuiz Tests
    
    [Fact]
    public async Task CreateQuiz_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var request = new CreateQuizRequest
        {
            Title = "New Quiz",
            LessonId = lessonId
        };
        var quiz = request.MapToQuiz();
        var createdQuiz = new Quiz { Id = Guid.NewGuid(), Title = "New Quiz", LessonId = lessonId };
        _quizRepoMock.Setup(r => r.CreateQuizAsync(It.Is<Quiz>(q => q.Title == "New Quiz"))).ReturnsAsync(createdQuiz);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.CreateQuiz(request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetQuiz), createdResult.ActionName);
        Assert.Equal(createdQuiz.Id, createdResult.RouteValues["id"]);
    }
    
    [Fact]
    public async Task CreateQuiz_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateQuizRequest
        {
            Title = "", // Invalid: empty
            LessonId = Guid.NewGuid()
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _controller.CreateQuiz(request));
        Assert.Contains("Title is required.", exception.Message);
    }
    
    #endregion
    
    #region UpdateQuiz Tests
    
    [Fact]
    public async Task UpdateQuiz_ValidRequest_ReturnsOk()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var request = new UpdateQuizRequest
        {
            Title = "Updated Quiz"
        };
        var existingQuiz = new Quiz { Id = quizId, Title = "Old Quiz", LessonId = Guid.NewGuid() };
        var updatedQuiz = new Quiz { Id = quizId, Title = "Updated Quiz", LessonId = existingQuiz.LessonId };
        _quizRepoMock.Setup(r => r.GetQuizByIdAsync(quizId)).ReturnsAsync(existingQuiz);
        _quizRepoMock.Setup(r => r.UpdateQuizAsync(It.Is<Quiz>(q => q.Title == "Updated Quiz"))).ReturnsAsync(updatedQuiz);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.UpdateQuiz(quizId, request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<Quiz>(okResult.Value);
        Assert.Equal("Updated Quiz", response.Title);
    }
    
    [Fact]
    public async Task UpdateQuiz_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        var request = new UpdateQuizRequest
        {
            Title = "" // Invalid: empty
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _controller.UpdateQuiz(quizId, request));
        Assert.Contains("Title is required.", exception.Message);
    }
    
    #endregion
    
    #region DeleteQuiz Tests
    
    [Fact]
    public async Task DeleteQuiz_ValidId_ReturnsNoContent()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        _quizRepoMock.Setup(r => r.DeleteQuizAsync(quizId)).Returns(Task.CompletedTask);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.DeleteQuiz(quizId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    [Fact]
    public async Task DeleteQuiz_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var quizId = Guid.NewGuid();
        _quizRepoMock.Setup(r => r.DeleteQuizAsync(quizId)).ThrowsAsync(new KeyNotFoundException("Invalid Quiz ID."));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.DeleteQuiz(quizId);
        
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Invalid Quiz ID.", notFoundResult.Value);
    }
    
    #endregion
}