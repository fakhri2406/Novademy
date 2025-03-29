using System.ComponentModel.DataAnnotations;

namespace Novademy.Contracts.Requests.Package;

public class UpdatePackageRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 100 characters.")]
    public required string Title { get; init; }
    
    [Required(ErrorMessage = "Description is required.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters.")]
    public required string Description { get; init; }
    
    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
    public required decimal Price { get; init; }
    
    [Url(ErrorMessage = "Invalid URL format.")]
    public string? ImageUrl { get; init; }
    
    [Required(ErrorMessage = "At least one Course ID is required.")]
    public required IEnumerable<Guid> CourseIds { get; init; }
}