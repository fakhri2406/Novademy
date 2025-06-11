using Novademy.Contracts.Requests.Subscription;
using Novademy.Contracts.Responses.Subscription;

namespace Novademy.Application.Services.Abstract;

public interface ISubscriptionService
{
    Task<IEnumerable<SubscriptionResponse>> GetActiveByUserIdAsync(Guid userId);
    Task<SubscriptionResponse> SubscribeAsync(SubscriptionRequest request);
} 