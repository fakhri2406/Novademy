using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.Application.Services.Abstract;
using Novademy.Application.ExternalServices.OpenAI;
using Novademy.Contracts.Requests.OpenAI;
using Novademy.Contracts.Responses.OpenAI;
using Novademy.Contracts.Responses.Lesson;
using Xunit;

namespace Novademy.UnitTests.ControllerTests;

public class OpenAIControllerTests
{
    [Fact]
    public async Task AskQuestion_ReturnsOkWithAnswer()
    {
        // Arrange
        var lessonId = Guid.NewGuid();
        var question = "What is the main concept?";
        var transcript = "This is the lesson transcript.";
        var answer = "This is the AI answer.";

        var lessonResponse = new LessonResponse
        {
            Id = lessonId,
            Title = "Title",
            Description = "Description",
            VideoUrl = "video.mp4",
            Order = 1,
            IsFree = true,
            Transcript = transcript,
            ImageUrl = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseId = Guid.NewGuid()
        };

        var mockLessonService = new Mock<ILessonService>();
        mockLessonService.Setup(s => s.GetByIdAsync(lessonId)).ReturnsAsync(lessonResponse);

        var mockOpenAIService = new Mock<IOpenAIService>();
        mockOpenAIService.Setup(s => s.AskQuestionAsync(transcript, question)).ReturnsAsync(answer);

        var controller = new OpenAIController(mockLessonService.Object, mockOpenAIService.Object);
        var request = new AskLessonQuestionRequest { LessonId = lessonId, Question = question };

        // Act
        var result = await controller.AskQuestion(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AskLessonQuestionResponse>(okResult.Value);
        Assert.Equal(answer, response.Answer);

        mockLessonService.Verify(s => s.GetByIdAsync(lessonId), Times.Once);
        mockOpenAIService.Verify(s => s.AskQuestionAsync(transcript, question), Times.Once);
    }
} 