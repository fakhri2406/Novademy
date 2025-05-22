using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Novademy.Application.Repositories.Abstract;
using Novademy.Application.Mapping;
using Novademy.Application.Services.Abstract;
using Novademy.Contracts.Requests.Subscription;
using Novademy.Contracts.Responses.Subscription;

namespace Novademy.Application.Services.Concrete;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _repo;
    private readonly IValidator<SubscriptionRequest> _subscribeValidator;

    public SubscriptionService(
        ISubscriptionRepository repo,
        IValidator<SubscriptionRequest> subscribeValidator)
    {
        _repo = repo;
        _subscribeValidator = subscribeValidator;
    }

    public async Task<IEnumerable<SubscriptionResponse>> GetActiveByUserIdAsync(Guid userId)
    {
        var subscriptions = await _repo.GetActiveSubscriptionsByUserIdAsync(userId);
        return subscriptions.Select(s => s.MapToSubscriptionResponse());
    }

    public async Task<SubscriptionResponse> SubscribeAsync(SubscriptionRequest request)
    {
        await _subscribeValidator.ValidateAndThrowAsync(request);
        
        var subscription = request.MapToSubscription();
        var created = await _repo.CreateSubscriptionAsync(subscription);
        
        return created.MapToSubscriptionResponse();
    }
} 