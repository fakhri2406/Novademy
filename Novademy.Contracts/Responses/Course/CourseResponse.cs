namespace Novademy.Contracts.Responses.Course;

public class CourseResponse
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
}