using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Novademy.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Not Found");
            await HandleExceptionAsync(context, ex.Message, StatusCodes.Status404NotFound);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Access Denied");
            await HandleExceptionAsync(context, ex.Message, StatusCodes.Status401Unauthorized);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid Argument");
            await HandleExceptionAsync(context, ex.Message, StatusCodes.Status409Conflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Error");
            await HandleExceptionAsync(context, "An unexpected error occurred.", StatusCodes.Status400BadRequest);
        }
    }
    
    private static Task HandleExceptionAsync(HttpContext context, string message, int statusCode)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;
        
        var problemDetails = new ProblemDetails
        {
            Title = "Error",
            Detail = message,
            Status = statusCode,
            Instance = context.Request.Path
        };
        
        var result = JsonSerializer.Serialize(problemDetails);
        return context.Response.WriteAsync(result);
    }
}