using System.ComponentModel.DataAnnotations;
using Novademy.Contracts.Enums;

namespace Novademy.Application.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    [MinLength(8)]
    public string Password { get; set; }
    public string? Salt { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    [Range(1, 4)]
    public int Group { get; set; }
    public SectorType Sector { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationExpiry { get; set; }
    public bool IsEmailVerified { get; set; }
    [Required]
    public int RoleId { get; set; }
    public Role? Role { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}