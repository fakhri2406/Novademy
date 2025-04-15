using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.API.Mapping;
using Novademy.Application.Models;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Validators.Subscription;
using Novademy.Contracts.Requests.Subscription;
using Novademy.Contracts.Responses.Subscription;
using System.Security.Claims;
using Xunit;

namespace Novademy.UnitTests.Controllers;

public class SubscriptionControllerTests
{
    private readonly Mock<ISubscriptionRepository> _repoMock;
    private readonly IValidator<SubscriptionRequest> _subscribeValidator;
    private readonly SubscriptionController _controller;
    
    public SubscriptionControllerTests()
    {
        _repoMock = new Mock<ISubscriptionRepository>();
        _subscribeValidator = new SubscribeRequestValidator();
        
        _controller = new SubscriptionController(
            _repoMock.Object,
            _subscribeValidator);
    }
    
    #region GetActiveSubscriptions Tests
    
    [Fact]
    public async Task GetActiveSubscriptions_WithSubscriptions_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var subscriptions = new List<Subscription>
        {
            new Subscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = Guid.NewGuid(),
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1)
            }
        };
        _repoMock.Setup(r => r.GetActiveSubscriptionsByUserIdAsync(userId)).ReturnsAsync(subscriptions);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetActiveSubscriptions(userId);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responses = Assert.IsAssignableFrom<IEnumerable<SubscriptionResponse>>(okResult.Value);
        Assert.Single(responses);
        Assert.Equal(userId, responses.First().UserId);
    }
    
    [Fact]
    public async Task GetActiveSubscriptions_NoSubscriptions_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetActiveSubscriptionsByUserIdAsync(userId)).ReturnsAsync(new List<Subscription>());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.GetActiveSubscriptions(userId);
        
        // Assert
        Assert.IsType<NoContentResult>(result);
    }
    
    #endregion
    
    #region Subscribe Tests
    
    [Fact]
    public async Task Subscribe_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new SubscriptionRequest
        {
            UserId = Guid.NewGuid(),
            PackageId = Guid.NewGuid()
        };
        var subscription = request.MapToSubscription();
        var createdSubscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            PackageId = request.PackageId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1)
        };
        _repoMock.Setup(r => r.CreateSubscriptionAsync(It.Is<Subscription>(s => s.UserId == request.UserId))).ReturnsAsync(createdSubscription);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act
        var result = await _controller.Subscribe(request);
        
        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(_controller.GetActiveSubscriptions), createdResult.ActionName);
        Assert.Equal(request.UserId, createdResult.RouteValues["userId"]);
    }
    
    [Fact]
    public async Task Subscribe_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        var request = new SubscriptionRequest
        {
            UserId = Guid.Empty, // Invalid: empty guid
            PackageId = Guid.NewGuid()
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") })) }
        };
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _controller.Subscribe(request));
        Assert.Contains("User ID is required.", exception.Message);
    }
    
    #endregion
} 