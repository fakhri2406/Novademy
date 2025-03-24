namespace Novademy.Contracts.Requests.Course;

public class UpdateCourseRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
}