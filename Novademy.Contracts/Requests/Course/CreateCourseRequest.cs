using System.ComponentModel.DataAnnotations;
using Novademy.Contracts.Enums;

namespace Novademy.Contracts.Requests.Course;

public class CreateCourseRequest
{
    [Required(ErrorMessage = "Title is required.")]
    public required string Title { get; init; }
    
    [Required(ErrorMessage = "Description is required.")]
    public required string Description { get; init; }
    
    [Required(ErrorMessage = "Subject is required.")]
    public required SubjectType Subject { get; init; }
    
    [Url(ErrorMessage = "Invalid URL format.")]
    public string? ImageUrl { get; init; }
}