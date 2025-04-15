namespace Novademy.Contracts.Requests.Quiz;

public class CreateQuestionRequest
{
    public required string Text { get; init; }
    public required Guid QuizId { get; init; }
    public required List<CreateAnswerRequest> Answers { get; init; }
}