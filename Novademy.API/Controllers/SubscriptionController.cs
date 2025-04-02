using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.Mapping;
using Novademy.Application.Repositories.Abstract;
using Novademy.Contracts.Requests.Subscription;

namespace Novademy.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionRepository _repo;
    private readonly IValidator<SubscriptionRequest> _subscribeValidator;
    
    public SubscriptionController(
        ISubscriptionRepository repo,
        IValidator<SubscriptionRequest> subscribeValidator)
    {
        _repo = repo;
        _subscribeValidator = subscribeValidator;
    }
    
    #region GET
    
    /// <summary>
    /// Get active subscriptions for a user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("active/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActiveSubscriptions([FromRoute] Guid userId)
    {
        try
        {
            var subscriptions = await _repo.GetActiveSubscriptionsByUserIdAsync(userId);
            var responses = subscriptions.Select(s => s.MapToSubscriptionResponse());
            return responses.Any() ? Ok(responses) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
    
    #region POST

    /// <summary>
    /// Subscribe to a package (annual)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Subscribe([FromBody] SubscriptionRequest request)
    {
        var validationResult = await _subscribeValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        
        var subscription = request.MapToSubscription();
        try
        {
            var createdSubscription = await _repo.CreateSubscriptionAsync(subscription);
            
            var response = createdSubscription.MapToSubscriptionResponse();
            return CreatedAtAction(nameof(GetActiveSubscriptions), new { userId = response.UserId },
                $"User {response.UserId} subscribed to package {response.PackageId}.");
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