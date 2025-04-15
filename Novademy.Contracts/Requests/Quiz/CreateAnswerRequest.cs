namespace Novademy.Contracts.Requests.Quiz;

public class CreateAnswerRequest
{
    public required string Text { get; init; }
    public required bool IsCorrect { get; init; }
}