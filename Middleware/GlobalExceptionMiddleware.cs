using System.Net;
using System.Text.Json;
using ADPA.Models.DTOs;

namespace ADPA.Middleware;

/// <summary>
/// Global exception handling middleware for structured error responses
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🚨 Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        string message = GetErrorMessage(exception);

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            case KeyNotFoundException:
            case FileNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                message = "Resource not found";
                break;
            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                message = "Unauthorized access";
                break;
            case InvalidOperationException when exception.Message.Contains("duplicate"):
                response.StatusCode = (int)HttpStatusCode.Conflict;
                message = "Resource already exists";
                break;
            case TimeoutException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                message = "Request timeout";
                break;
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                message = "An internal server error occurred";
                break;
        }

        var errorResponse = new
        {
            success = false,
            message = message,
            data = (object?)null,
            timestamp = DateTime.UtcNow
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await response.WriteAsync(jsonResponse);
    }

    private static string GetErrorMessage(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => "Required parameter is missing",
            ArgumentException => "Invalid parameter provided",
            InvalidOperationException => "Invalid operation attempted",
            _ => "An unexpected error occurred"
        };
    }
}

/// <summary>
/// Extension method for registering the global exception middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}