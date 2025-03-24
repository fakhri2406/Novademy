namespace Novademy.Contracts.Responses.Lesson;

public class LessonResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string VideoUrl { get; init; }
    public Guid CourseId { get; init; }
}