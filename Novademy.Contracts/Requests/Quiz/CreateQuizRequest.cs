namespace Novademy.Contracts.Requests.Quiz;

public class CreateQuizRequest
{
    public required string Title { get; init; }
    public required Guid LessonId { get; init; }
}