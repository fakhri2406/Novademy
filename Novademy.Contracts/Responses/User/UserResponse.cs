using Novademy.Contracts.Enums;

namespace Novademy.Contracts.Responses.User;

public class UserResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string PhoneNumber { get; init; }
    public string Role { get; init; }
    public int Group { get; init; }
    public SectorType Sector { get; init; }
}