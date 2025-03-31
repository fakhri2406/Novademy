using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class Answer
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Text { get; set; }
    [Required]
    public bool IsCorrect { get; set; }
    [Required]
    public Guid QuestionId { get; set; }
    public Question? Question { get; set; }
}