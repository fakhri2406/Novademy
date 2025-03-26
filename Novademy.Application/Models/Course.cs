using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class Course
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    public ICollection<Package> Packages { get; set; } = new List<Package>();
}