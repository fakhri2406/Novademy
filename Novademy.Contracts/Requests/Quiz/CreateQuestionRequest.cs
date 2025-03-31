namespace Novademy.Contracts.Requests.Quiz;

public class CreateQuestionRequest
{
    public string Text { get; set; }
    public Guid QuizId { get; set; }
    public List<CreateAnswerRequest> Answers { get; set; }
}