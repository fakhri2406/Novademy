namespace Novademy.Contracts.Requests.Subscription;

public class SubscribeRequest
{
    public required Guid UserId { get; init; }
    public required Guid PackageId { get; init; }
}