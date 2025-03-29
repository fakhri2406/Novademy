using System.ComponentModel.DataAnnotations;
using Novademy.Contracts.Enums;

namespace Novademy.Contracts.Requests.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 20 characters.")]
    public required string Username { get; init; }
    
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
    public required string Password { get; init; }
    
    [Required(ErrorMessage = "First name is required.")]
    public required string FirstName { get; init; }
    
    [Required(ErrorMessage = "Last name is required.")]
    public required string LastName { get; init; }
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public required string Email { get; init; }
    
    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^0?\d{9}$",
        ErrorMessage = "Phone number must be 9 or 10 digits, optionally starting with 0.")]
    public required string PhoneNumber { get; init; }
    
    [Required(ErrorMessage = "Role ID is required.")]
    [Range(1, 3, ErrorMessage = "RoleId must be between 1 and 3 (1=Admin, 2=Teacher, 3=Student).")]
    public required int RoleId { get; init; }
    
    [Range(1, 4, ErrorMessage = "Group must be between 1 and 4.")]
    public int Group { get; init; }
    
    [Range(1, 3, ErrorMessage = "Group must be between 1 and 4 (1=AZ, 2=RU, 3=ENG).")]
    public SectorType Sector { get; init; }
    
    [Url(ErrorMessage = "Invalid URL format.")]
    public string? ProfilePictureUrl { get; init; }
}