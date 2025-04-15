using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.AI;
using Novademy.Contracts.Responses.AI;

namespace Novademy.API.Controllers;

[ApiController]
[Authorize]
public class AIChatController : ControllerBase
{
    private readonly IAIChatService _aiChatService;
    private readonly ILessonRepository _lessonRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public AIChatController(
        IAIChatService aiChatService,
        ILessonRepository lessonRepository,
        ISubscriptionRepository subscriptionRepository)
    {
        _aiChatService = aiChatService;
        _lessonRepository = lessonRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    [HttpPost]
    [Route(ApiEndPoints.AIChat.Chat)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Chat([FromBody] AIChatRequest request)
    {
        try
        {
            var lesson = await _lessonRepository.GetLessonByIdAsync(request.LessonId);
            if (lesson is null)
            {
                return NotFound("Lesson not found.");
            }

            var isAdmin = User.IsInRole("Admin");
            var isTeacher = User.IsInRole("Teacher");
            
            if ((!isAdmin && !isTeacher) && !lesson.IsFree)
            {
                var userId = Guid.Parse(User.FindFirst("id")?.Value ?? string.Empty);
                var hasAccess = await _subscriptionRepository.HasActiveSubscriptionForLessonAsync(userId, request.LessonId);
                if (!hasAccess)
                {
                    return Forbid("You do not have access to this lesson.");
                }
            }
            
            var response = await _aiChatService.GetChatResponseAsync(request.Question, lesson.Transcript!);
            
            return Ok(new AIChatResponse { Answer = response });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
} 