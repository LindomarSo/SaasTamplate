using FluentValidation;
using SaasStarter.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace SaasStarter.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errorCode, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.BadRequest,
                "Erro de validação.",
                (string?)null,
                (object)ve.Errors.Select(e => e.ErrorMessage).ToList()
            ),
            EmailNotConfirmedException => (
                HttpStatusCode.Unauthorized,
                exception.Message,
                (string?)"EMAIL_NOT_CONFIRMED",
                (object?)null
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Forbidden,
                exception.Message,
                (string?)null,
                (object?)null
            ),
            InvalidOperationException => (
                HttpStatusCode.Conflict,
                exception.Message,
                (string?)null,
                (object?)null
            ),
            PlanLimitExceededException => (
                HttpStatusCode.Forbidden,
                exception.Message,
                (string?)null,
                (object?)null
            ),
            NotSupportedException => (
                HttpStatusCode.BadRequest,
                exception.Message,
                (string?)null,
                (object?)null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Ocorreu um erro interno. Tente novamente mais tarde.",
                (string?)null,
                (object?)null
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new { status = (int)statusCode, message, errorCode, errors };
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
