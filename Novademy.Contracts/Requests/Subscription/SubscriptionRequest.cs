namespace Novademy.Contracts.Requests.Subscription;

public class SubscriptionRequest
{
    public required Guid UserId { get; init; }
    public required Guid PackageId { get; init; }
}