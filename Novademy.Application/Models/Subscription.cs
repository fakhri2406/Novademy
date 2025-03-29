using System.ComponentModel.DataAnnotations;

namespace Novademy.Application.Models;

public class Subscription
{ 
    [Key]
    public Guid Id { get; set; }
    [Required]
    public Guid UserId { get; set; }
    public User? User { get; set; }
    [Required]
    public Guid PackageId { get; set; }
    public Package? Package { get; set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}