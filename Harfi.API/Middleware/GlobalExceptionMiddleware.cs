using System.Net;
using System.Text.Json;

namespace Harfi.API.Middleware;

/// <summary>
/// Catches ALL unhandled exceptions and returns a clean JSON error response.
/// Registered in Program.cs BEFORE all other middleware.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex,
                "Unhandled exception on {Method} {Path} — {Message}",
                context.Request.Method,
                context.Request.Path,
                ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context, Exception exception)
    {
        // Map exception types → HTTP status codes + Arabic messages
        var (statusCode, message) = exception switch
        {
            InvalidOperationException e => (HttpStatusCode.BadRequest, e.Message),
            UnauthorizedAccessException e => (HttpStatusCode.Forbidden, e.Message),
            KeyNotFoundException e => (HttpStatusCode.NotFound, e.Message),
            ArgumentNullException e => (HttpStatusCode.BadRequest, e.Message),
            ArgumentException e => (HttpStatusCode.BadRequest, e.Message),
            NotImplementedException => (HttpStatusCode.NotImplemented, "هذه الخاصية غير متاحة بعد."),
            _ => (HttpStatusCode.InternalServerError, "خطأ في الخادم، حاول مرة أخرى.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var body = new
        {
            success = false,
            message = message,
            data = (object?)null,
            errors = new[] { message },
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

