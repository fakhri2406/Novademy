namespace Novademy.Contracts.Requests.Course;

public class CreateCourseRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
}