using System.ComponentModel.DataAnnotations;

namespace Novademy.Contracts.Requests.Subscription;

public class SubscribeRequest
{
    [Required(ErrorMessage = "User ID is required.")]
    public required Guid UserId { get; init; }
    
    [Required(ErrorMessage = "Package ID is required.")]
    public required Guid PackageId { get; init; }
}