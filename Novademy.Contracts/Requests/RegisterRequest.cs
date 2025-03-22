namespace Novademy.Contracts.Requests;

public class RegisterRequest
{
    public required string Username { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required int PhoneNumber { get; init; }
    public required string Password { get; init; }
    public int RoleId { get; set; }
}