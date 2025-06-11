using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Application.ExternalServices.OpenAI;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.OpenAI;
using Novademy.Contracts.Responses.OpenAI;

namespace Novademy.API.Controllers;

[ApiController]
[Authorize]
public class OpenAIController : ControllerBase
{
    private readonly ILessonService _lessonService;
    private readonly IOpenAIService _openAIService;
    
    public OpenAIController(ILessonService lessonService, IOpenAIService openAIService)
    {
        _lessonService = lessonService;
        _openAIService = openAIService;
    }
    
    #region Ask Lesson Question

    /// <summary>
    /// Ask a question about a lesson using OpenAI API
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route(ApiEndPoints.OpenAI.Ask)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AskQuestion([FromForm] AskLessonQuestionRequest request)
    {
        var response = await _lessonService.GetByIdAsync(request.LessonId);
        var transcript = response.Transcript;
        
        var answer = await _openAIService.AskQuestionAsync(transcript!, request.Question);
        return Ok(new AskLessonQuestionResponse { Answer = answer });
    }
    
    #endregion
}