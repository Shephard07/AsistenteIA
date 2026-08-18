using Asistente.Shared.Models;

namespace Asistente.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Ocurrió un error no controlado en la solicitud.");

            var (statusCode, mensaje) = exception switch
            {
                ArgumentException => (
                    StatusCodes.Status400BadRequest,
                    exception.Message),

                KeyNotFoundException => (
                    StatusCodes.Status404NotFound,
                    exception.Message),

                TimeoutException => (
                    StatusCodes.Status504GatewayTimeout,
                    "La IA tardó demasiado en responder. Inténtalo nuevamente."),

                HttpRequestException => (
                    StatusCodes.Status503ServiceUnavailable,
                    "El servicio de IA no está disponible. Verifica que Ollama esté iniciado."),

                InvalidOperationException => (
                    StatusCodes.Status503ServiceUnavailable,
                    exception.Message),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Ocurrió un error inesperado. Inténtalo nuevamente.")
            };

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Mensaje = mensaje
            });
        }
    }
}