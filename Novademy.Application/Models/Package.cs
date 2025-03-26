using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class Package
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public decimal Price { get; set; }
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}