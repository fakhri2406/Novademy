using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class UserQuizAttempt
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public int Score { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public Guid UserId { get; set; }
    public User? User { get; set; }
    [Required]
    public Guid QuizId { get; set; }
    public Quiz? Quiz { get; set; }
}