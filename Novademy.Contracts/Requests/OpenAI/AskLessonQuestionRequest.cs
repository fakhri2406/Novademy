namespace Novademy.Contracts.Requests.OpenAI;

public class AskLessonQuestionRequest
{
    public required Guid LessonId { get; init; }
    public required string Question { get; init; }
}