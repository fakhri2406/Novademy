namespace Novademy.Contracts.Requests.Auth;

public class RefreshTokenRequest
{
    public required string Token { get; init; }
}