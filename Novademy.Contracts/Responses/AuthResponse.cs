namespace Novademy.Contracts.Responses;

public class AuthResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}