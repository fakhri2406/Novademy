using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Novademy.API.EndPoints;
using System.Linq;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Subscription;

namespace Novademy.API.Controllers;

[ApiController]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    
    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }
    
    #region GET
    
    /// <summary>
    /// Get active subscriptions for a user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet]
    [Route(ApiEndPoints.Subscription.GetActiveSubscriptions)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveSubscriptions([FromRoute] Guid userId)
    {
        try
        {
            var responses = await _subscriptionService.GetActiveByUserIdAsync(userId);
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
    [Route(ApiEndPoints.Subscription.Subscribe)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Subscribe([FromBody] SubscriptionRequest request)
    {
        try
        {
            var response = await _subscriptionService.SubscribeAsync(request);
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