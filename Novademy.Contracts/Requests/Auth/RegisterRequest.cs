using Novademy.Contracts.Enums;

namespace Novademy.Contracts.Requests.Auth;

public class RegisterRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required int PhoneNumber { get; init; }
    public required int RoleId { get; set; }
    public int Group { get; set; }
    public SectorType Sector { get; set; }
    public string? ProfilePictureUrl { get; set; }
}