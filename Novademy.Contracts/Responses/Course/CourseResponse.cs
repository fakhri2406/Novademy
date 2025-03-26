using Novademy.Contracts.Enums;

namespace Novademy.Contracts.Responses.Course;

public class CourseResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required SubjectType Subject { get; init; }
    public string? ImageUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}