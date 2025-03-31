namespace Novademy.Contracts.Responses.Subscription;

public class SubscriptionResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid PackageId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsActive { get; init; }
}