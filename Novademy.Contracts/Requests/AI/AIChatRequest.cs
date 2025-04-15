namespace Novademy.Contracts.Requests.AI;

public class AIChatRequest
{
    public required string Question { get; init; }
    public required Guid LessonId { get; init; }
} 