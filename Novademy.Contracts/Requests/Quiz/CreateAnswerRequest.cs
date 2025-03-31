namespace Novademy.Contracts.Requests.Quiz;

public class CreateAnswerRequest
{
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
}