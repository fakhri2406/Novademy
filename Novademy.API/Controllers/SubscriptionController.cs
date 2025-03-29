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
    
    public SubscriptionController(ISubscriptionRepository repo)
    {
        _repo = repo;
    }

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
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        try
        {
            var subscription = request.MapToSubscription();
            
            var createdSubscription = await _repo.CreateSubscriptionAsync(subscription);
            return CreatedAtAction(nameof(GetActiveSubscriptions), new { userId = createdSubscription.UserId },
                $"User {createdSubscription.UserId} subscribed to package {createdSubscription.PackageId}.");
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
            return subscriptions.Any() ? Ok(subscriptions) : NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    #endregion
}