using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}