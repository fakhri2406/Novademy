using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class Role
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
}