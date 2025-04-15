using System.Text;
using Microsoft.Extensions.Configuration;
using OpenAI_API;
using OpenAI_API.Chat;

namespace Novademy.Application.Services.Concrete;

public class AIChatService : IAIChatService
{
    private readonly OpenAIAPI _openAiApi;
    private const string SystemPrompt = "You are an educational AI assistant. Your role is to help students understand the lesson content. Use the provided lesson transcript as context to answer questions accurately and helpfully. If the question is not related to the lesson content, politely inform the user that you can only answer questions about the lesson.";

    public AIChatService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI API key is not configured");
        _openAiApi = new OpenAIAPI(apiKey);
    }

    public async Task<string> GetChatResponseAsync(string userQuestion, string context)
    {
        var chat = _openAiApi.Chat.CreateConversation();
        
        chat.AppendSystemMessage(SystemPrompt);
        chat.AppendUserMessage($"Here is the lesson transcript: {context}");
        chat.AppendUserMessage(userQuestion);
        
        var response = await chat.GetResponseFromChatbotAsync();
        return response;
    }
} 