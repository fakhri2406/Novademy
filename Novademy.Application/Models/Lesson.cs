using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class Lesson
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public string VideoUrl { get; set; }
    [Required]
    public int Order { get; set; }
    public string? Transcript { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Required]
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
}