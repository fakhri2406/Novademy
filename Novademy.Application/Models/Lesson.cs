using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class Lesson
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; }
    public string Description { get; set; }
    [Required]
    public string VideoUrl { get; set; }
    [Required]
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
}