using Microsoft.AspNetCore.Http;
using Novademy.Contracts.Enums;

namespace Novademy.Contracts.Requests.Course;

public class CreateCourseRequest
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required SubjectType Subject { get; init; }
    public IFormFile? Image { get; init; }
}