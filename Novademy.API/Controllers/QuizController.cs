using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Requests.Quiz;
using Novademy.API.Mapping;

namespace Novademy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizRepository _quizRepo;
    private readonly IQuestionRepository _questionRepo;
    private readonly IAnswerRepository _answerRepo;
    private readonly ILessonRepository _lessonRepo;
    
    public QuizController(
        IQuizRepository quizRepo,
        IQuestionRepository questionRepo,
        IAnswerRepository answerRepo,
        ILessonRepository lessonRepo)
    {
        _quizRepo = quizRepo;
        _questionRepo = questionRepo;
        _answerRepo = answerRepo;
        _lessonRepo = lessonRepo;
    }
    
    [HttpGet("lesson/{lessonId}")]
    public async Task<IActionResult> GetQuizzesByLessonId(Guid lessonId)
    {
        var quizzes = await _quizRepo.GetQuizzesByLessonIdAsync(lessonId);
        return quizzes.Any() ? Ok(quizzes) : NoContent();
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetQuiz(Guid id)
    {
        var quiz = await _quizRepo.GetQuizByIdAsync(id);
        return Ok(quiz);
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequest request)
    {
        var lesson = await _lessonRepo.GetLessonByIdAsync(request.LessonId);
        var quiz = request.MapToQuiz();
        var createdQuiz = await _quizRepo.CreateQuizAsync(quiz);
        return CreatedAtAction(nameof(GetQuiz), new { id = createdQuiz.Id }, createdQuiz);
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateQuiz(Guid id, [FromBody] UpdateQuizRequest request)
    {
        var quiz = await _quizRepo.GetQuizByIdAsync(id);
        quiz.Title = request.Title;
        await _quizRepo.UpdateQuizAsync(quiz);
        return Ok(quiz);
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteQuiz(Guid id)
    {
        await _quizRepo.DeleteQuizAsync(id);
        return NoContent();
    }
    
    [HttpPost("question")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        var quiz = await _quizRepo.GetQuizByIdAsync(request.QuizId);
        if (!request.Answers.Any(a => a.IsCorrect))
        {
            return BadRequest("A question must have at least one correct answer.");
        }

        var question = request.MapToQuestion();
        var createdQuestion = await _questionRepo.CreateQuestionAsync(question);

        foreach (var answerRequest in request.Answers)
        {
            var answer = answerRequest.MapToAnswer(createdQuestion.Id);
            await _answerRepo.CreateAnswerAsync(answer);
        }

        return CreatedAtAction(nameof(GetQuestion), new { id = createdQuestion.Id }, createdQuestion);
    }

    [HttpGet("question/{id}")]
    public async Task<IActionResult> GetQuestion(Guid id)
    {
        var question = await _questionRepo.GetQuestionByIdAsync(id);
        return Ok(question);
    }
}