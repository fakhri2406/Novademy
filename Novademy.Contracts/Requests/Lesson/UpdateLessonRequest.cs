using System.ComponentModel.DataAnnotations;

namespace Novademy.Contracts.Requests.Lesson;

public class UpdateLessonRequest
{
    [Required(ErrorMessage = "Title is required.")]
    public required string Title { get; init; }
    
    [Required(ErrorMessage = "Description is required.")]
    public required string Description { get; init; }
    
    [Required(ErrorMessage = "Video URL is required.")]
    [Url(ErrorMessage = "Invalid URL format.")]
    public required string VideoUrl { get; init; }
    
    [Required(ErrorMessage = "Order is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Order must be a positive number.")]
    public required int Order { get; init; }
    
    [StringLength(1000, ErrorMessage = "Transcript cannot exceed 1000 characters.")]
    public string? Transcript { get; init; }
    
    [Url(ErrorMessage = "Invalid URL format.")]
    public string? ImageUrl { get; init; }
}