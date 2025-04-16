using Microsoft.AspNetCore.Http;

namespace Novademy.Contracts.Requests.Lesson;

public class CreateLessonRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required IFormFile Video { get; init; }
    public required int Order { get; init; }
    public required bool IsFree { get; init; }
    public required string Transcript { get; init; }
    public IFormFile? Image { get; init; }
    public required Guid CourseId { get; init; }
}