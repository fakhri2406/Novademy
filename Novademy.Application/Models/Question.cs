using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class Question
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Text { get; set; }
    [Required]
    public Guid QuizId { get; set; }
    public Quiz? Quiz { get; set; }
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
}