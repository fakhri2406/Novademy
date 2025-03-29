namespace Novademy.Contracts.Requests.Lesson;

public class CreateLessonRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string VideoUrl { get; init; }
    public required int Order { get; init; }
    public string? Transcript { get; init; }
    public string? ImageUrl { get; init; }
    public required Guid CourseId { get; init; }
}