using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class Quiz
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public Guid LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public ICollection<Question> Questions { get; set; } = new List<Question>();
}