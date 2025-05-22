namespace Novademy.Contracts.Requests.Auth;

public class VerifyEmailRequest
{
    public required Guid UserId { get; init; }
    public required string Code { get; init; }
} 