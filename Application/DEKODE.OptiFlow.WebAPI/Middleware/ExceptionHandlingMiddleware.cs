using System.Net;
using System.Text.Json;
using DEKODE.OptiFlow.Domain.Entities.Exceptions;
using DEKODE.OptiFlow.DTO.Common;

namespace DEKODE.OptiFlow.WebAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate _next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        this._next = _next;
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
            _logger.LogError(ex, "Unhandled exception intercepted: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            PlanogramValidationException valEx => (
                HttpStatusCode.BadRequest,
                valEx.Message,
                new List<string> { $"Store: {valEx.StoreName ?? "N/A"}", $"Brand: {valEx.BrandName ?? "N/A"}" }
            ),
            PlanogramImportException impEx => (
                HttpStatusCode.BadRequest,
                impEx.Message,
                new List<string> { $"Row: {impEx.RowIndex}" }
            ),
            EntityNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                new List<string>()
            ),
            ConcurrencyConflictException concEx => (
                HttpStatusCode.Conflict,
                concEx.Message,
                new List<string>()
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                argEx.Message,
                new List<string>()
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected internal error occurred.",
                new List<string> { exception.Message }
            )
        };

        context.Response.StatusCode = (int)statusCode;
        var response = ApiResponse<object>.Fail(message, errors);
        var json = JsonSerializer.Serialize(response);

        return context.Response.WriteAsync(json);
    }
}
