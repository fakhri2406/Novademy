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
        
        _logger.LogInformation("Incoming Request: {method} {path} from {ip}",
            request.Method,
            request.Path,
            context.Connection.RemoteIpAddress?.ToString());
        
        _logger.LogInformation("Request Headers: {headers}", context.Request.Headers);
        
        await _next(context);
        
        _logger.LogInformation("Outgoing Response: {statusCode} for {method} {path}",
            context.Response.StatusCode,
            request.Method,
            request.Path);
    }
}