using Novademy.Contracts.Enums;

namespace Novademy.Contracts.Requests.User;

public class UpdateUserRequest
{
    public required string Username { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public int Group { get; init; }
    public SectorType Sector { get; init; }
}