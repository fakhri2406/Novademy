namespace Novademy.API.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var ip = context.Connection.RemoteIpAddress?.ToString();
        
        _logger.LogInformation("Incoming Request {@RequestInfo}",
            new
            {
                Method = request.Method,
                Path = request.Path,
                IP = ip,
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            });
        
        await _next(context);
        
        _logger.LogInformation("Outgoing Response {@ResponseInfo}",
            new
            {
                StatusCode = context.Response.StatusCode,
                Method = request.Method,
                Path = request.Path
            });
    }
}