namespace Novademy.Application.ExternalServices.OpenAI;

public interface IOpenAIService
{
    Task<string> AskQuestionAsync(string transcript, string question);
}