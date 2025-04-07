using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Requests.Quiz;
using Novademy.API.Mapping;

namespace Novademy.API.Controllers;

[ApiController]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizRepository _quizRepo;
    private readonly IQuestionRepository _questionRepo;
    private readonly IAnswerRepository _answerRepo;
    private readonly IValidator<CreateQuizRequest> _createQuizValidator;
    private readonly IValidator<UpdateQuizRequest> _updateQuizValidator;
    private readonly IValidator<CreateQuestionRequest> _createQuestionValidator;
    private readonly IValidator<CreateAnswerRequest> _createAnswerValidator;
    
    public QuizController(
        IQuizRepository quizRepo,
        IQuestionRepository questionRepo,
        IAnswerRepository answerRepo,
        IValidator<CreateQuizRequest> createQuizValidator,
        IValidator<UpdateQuizRequest> updateQuizValidator,
        IValidator<CreateQuestionRequest> createQuestionValidator,
        IValidator<CreateAnswerRequest> createAnswerValidator)
    {
        _quizRepo = quizRepo;
        _questionRepo = questionRepo;
        _answerRepo = answerRepo;
        _createQuizValidator = createQuizValidator;
        _updateQuizValidator = updateQuizValidator;
        _createQuestionValidator = createQuestionValidator;
        _createAnswerValidator = createAnswerValidator;
    }
    
    #region GET (Quiz)
    
    /// <summary>
    /// Get all quizzes of a specific lesson
    /// </summary>
    /// <param name="lessonId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Quiz.GetQuizzes)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuizzes(Guid lessonId)
    {
        var quizzes = await _quizRepo.GetQuizzesByLessonIdAsync(lessonId);
        return quizzes.Any() ? Ok(quizzes) : NoContent();
    }
    
    /// <summary>
    /// Get one quiz by ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Quiz.GetQuiz)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuiz(Guid id)
    {
        try
        {
            var quiz = await _quizRepo.GetQuizByIdAsync(id);
            return Ok(quiz);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region POST (Quiz)
    
    /// <summary>
    /// Create a quiz
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Quiz.CreateQuiz)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequest request)
    {
        await _createQuizValidator.ValidateAndThrowAsync(request);
        
        var quiz = request.MapToQuiz();
        try
        {
            var createdQuiz = await _quizRepo.CreateQuizAsync(quiz);
            return CreatedAtAction(nameof(GetQuiz), new { id = createdQuiz.Id }, createdQuiz);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region PUT (Quiz)
    
    /// <summary>
    /// Update an existing quiz
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut]
    [Route(ApiEndPoints.Quiz.UpdateQuiz)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateQuiz(Guid id, [FromBody] UpdateQuizRequest request)
    {
        await _updateQuizValidator.ValidateAndThrowAsync(request);
        
        try
        {
            var quiz = await _quizRepo.GetQuizByIdAsync(id);
            
            quiz!.Title = request.Title;
            await _quizRepo.UpdateQuizAsync(quiz);
            
            return Ok(quiz);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region DELETE (Quiz)
    
    /// <summary>
    /// Delete a quiz
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    [Route(ApiEndPoints.Quiz.DeleteQuiz)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeleteQuiz(Guid id)
    {
        try
        {
            await _quizRepo.DeleteQuizAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region GET (Question)
    
    /// <summary>
    /// Get all questions of a specific quiz
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Quiz.GetQuestion)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetQuestion(Guid id)
    {
        try
        {
            var question = await _questionRepo.GetQuestionByIdAsync(id);
            return Ok(question);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region POST (Question)
    
    /// <summary>
    /// Create a question with answers
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.Quiz.CreateQuestion)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        await _createQuestionValidator.ValidateAndThrowAsync(request);
        
        if (!request.Answers.Any(a => a.IsCorrect))
        {
            return BadRequest("A question must have at least one correct answer.");
        }
        
        var question = request.MapToQuestion();
        
        try
        {
            var createdQuestion = await _questionRepo.CreateQuestionAsync(question);
            
            foreach (var answerRequest in request.Answers)
            {
                await _createAnswerValidator.ValidateAndThrowAsync(answerRequest);
                
                var answer = answerRequest.MapToAnswer(createdQuestion.Id);
                await _answerRepo.CreateAnswerAsync(answer);
            }
            
            return CreatedAtAction(nameof(GetQuestion), new { id = createdQuestion.Id }, createdQuestion);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
}