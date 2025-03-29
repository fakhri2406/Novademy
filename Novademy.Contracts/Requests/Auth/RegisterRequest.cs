using System.ComponentModel.DataAnnotations;
using Novademy.Contracts.Enums;

namespace Novademy.Contracts.Requests.Auth;

public class RegisterRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    [RegularExpression(@"^0?\d{9}$")]
    public required string PhoneNumber { get; init; }
    public required int RoleId { get; init; }
    public int Group { get; init; }
    public SectorType Sector { get; init; }
    public string? ProfilePictureUrl { get; init; }
}