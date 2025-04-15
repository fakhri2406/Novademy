namespace Novademy.Application.Services.Abstract;

public interface IAIChatService
{
    Task<string> GetChatResponseAsync(string userQuestion, string context);
}