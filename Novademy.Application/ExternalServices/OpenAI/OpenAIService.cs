using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Novademy.Application.ExternalServices.OpenAI;

public class OpenAIService : IOpenAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpenAIOptions _options;
    
    public OpenAIService(IHttpClientFactory httpClientFactory, IOptions<OpenAIOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }
    
    public async Task<string> AskQuestionAsync(string transcript, string question)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        
        var prompt =
            $"Answer the following question of the student based only on the " +
            $"provided lesson transcript. Do not use external knowledge." +
            $"\n\nTranscript:\n{transcript}";
        
        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = prompt },
                new { role = "user", content = question }
            },
            max_tokens = 150
        };
        
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();
        
        var responseBody = await response.Content.ReadAsStringAsync();
        
        var doc = JsonDocument.Parse(responseBody);
        
        if (doc.RootElement.TryGetProperty("error", out var error))
        {
            var errorMessage = error.GetProperty("message").GetString();
            throw new Exception($"OpenAI API error: {errorMessage}");
        }
        
        if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
        {
            throw new Exception("OpenAI API returned an invalid or empty response.");
        }
        
        var message = choices[0].GetProperty("message");
        if (!message.TryGetProperty("content", out var contentElement))
        {
            throw new Exception("OpenAI API response does not contain a valid message content.");
        }
        
        var contentStr = contentElement.GetString();
        return contentStr?.Trim() ?? throw new Exception("Content is null.");
    }
}