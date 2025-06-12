using Microsoft.AspNetCore.Mvc;
using Moq;
using Novademy.API.Controllers;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Subscription;
using Novademy.Contracts.Responses.Subscription;
using Xunit;

namespace Novademy.UnitTests.ControllerTests;

public class SubscriptionControllerTests
{
    [Fact]
    public async Task GetActiveSubscriptions_WithExistingSubscriptions_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sampleSubscription = new SubscriptionResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PackageId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true
        };
        var subscriptions = new List<SubscriptionResponse> { sampleSubscription };
        var mockService = new Mock<ISubscriptionService>();
        mockService.Setup(s => s.GetActiveByUserIdAsync(userId)).ReturnsAsync(subscriptions);
        var controller = new SubscriptionController(mockService.Object);

        // Act
        var result = await controller.GetActiveSubscriptions(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IEnumerable<SubscriptionResponse>>(okResult.Value);
        Assert.Equal(subscriptions, returned);
        mockService.Verify(s => s.GetActiveByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetActiveSubscriptions_WithNoSubscriptions_ReturnsNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockService = new Mock<ISubscriptionService>();
        mockService.Setup(s => s.GetActiveByUserIdAsync(userId)).ReturnsAsync(new List<SubscriptionResponse>());
        var controller = new SubscriptionController(mockService.Object);

        // Act
        var result = await controller.GetActiveSubscriptions(userId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        mockService.Verify(s => s.GetActiveByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Subscribe_WhenCalled_ReturnsCreatedAtAction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var packageId = Guid.NewGuid();
        var request = new SubscriptionRequest { UserId = userId, PackageId = packageId };
        var response = new SubscriptionResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PackageId = packageId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            IsActive = true
        };
        var mockService = new Mock<ISubscriptionService>();
        mockService.Setup(s => s.SubscribeAsync(request)).ReturnsAsync(response);
        var controller = new SubscriptionController(mockService.Object);

        // Act
        var result = await controller.Subscribe(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(SubscriptionController.GetActiveSubscriptions), createdResult.ActionName);
        Assert.Equal(response.UserId, createdResult.RouteValues["userId"]);
        Assert.Equal($"User {response.UserId} subscribed to package {response.PackageId}.", createdResult.Value);
        mockService.Verify(s => s.SubscribeAsync(request), Times.Once);
    }
} 