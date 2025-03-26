using System.ComponentModel.DataAnnotations;
using Novademy.Contracts.Enums;

namespace Novademy.Application.Models;

public class Course
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public SubjectType Subject { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Package> Packages { get; set; } = new List<Package>();
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}