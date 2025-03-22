namespace Novademy.Contracts.Requests;

public class RefreshTokenRequest
{
    public required string Token { get; init; }
}