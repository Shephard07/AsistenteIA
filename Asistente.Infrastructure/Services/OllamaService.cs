using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Infrastructure.Models;
using Asistente.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asistente.Infrastructure.Services;

/// <summary>
/// Implementación del proveedor local de IA mediante Ollama.
/// </summary>
public class OllamaService : IAIProvider
{
    private const int IntentosMaximos = 2;

    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaService> _logger;

    public OllamaService(
        HttpClient httpClient,
        IOptions<OllamaOptions> options,
        ILogger<OllamaService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ChatResponseDto> SendAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var modelo = string.IsNullOrWhiteSpace(request.ModeloIA)
            ? _options.Model
            : request.ModeloIA;

        var timeoutSeconds = request.TimeoutSeconds > 0
            ? request.TimeoutSeconds
            : _options.TimeoutSeconds;

        var ollamaRequest = new OllamaChatRequest
        {
            Model = modelo,
            Stream = false,
            Think = false,
            KeepAlive = _options.KeepAlive,
            Options = new OllamaGenerationOptions
            {
                Temperature = request.Temperatura,
                NumPredict = request.MaxTokens
            },
            Messages = request.Mensajes
                .Select(mensaje => new OllamaChatMessage
                {
                    Role = ConvertirRol(mensaje.Rol),
                    Content = mensaje.Contenido
                })
                .ToList()
        };

        using var timeoutCancellationTokenSource =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(timeoutSeconds));

        using var linkedCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutCancellationTokenSource.Token);

        var token = linkedCancellationTokenSource.Token;
        var cronometro = Stopwatch.StartNew();

        try
        {
            for (var intento = 1; intento <= IntentosMaximos; intento++)
            {
                using var response = await _httpClient.PostAsJsonAsync(
                    "api/chat",
                    ollamaRequest,
                    token);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException(
                        $"El modelo '{modelo}' no está instalado en Ollama.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var detalleError = await response.Content
                        .ReadAsStringAsync(token);

                    _logger.LogError(
                        "Ollama devolvió el estado {StatusCode}. Detalle: {DetalleError}",
                        response.StatusCode,
                        detalleError);

                    throw new HttpRequestException(
                        "No fue posible obtener una respuesta de Ollama.");
                }

                var respuesta = await response.Content
                    .ReadFromJsonAsync<OllamaChatResponse>(
                        cancellationToken: token);

                if (!string.IsNullOrWhiteSpace(respuesta?.Message?.Content))
                {
                    cronometro.Stop();

                    return new ChatResponseDto
                    {
                        Contenido = respuesta.Message.Content.Trim(),
                        TiempoRespuestaMs =
                            (int)cronometro.ElapsedMilliseconds
                    };
                }

                _logger.LogWarning(
                    "Ollama devolvió una respuesta vacía. Intento {Intento} de {IntentosMaximos}.",
                    intento,
                    IntentosMaximos);

                if (intento < IntentosMaximos)
                {
                    await Task.Delay(500, token);
                }
            }

            throw new InvalidOperationException(
                "Ollama no devolvió una respuesta válida.");
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                "Ollama excedió el tiempo máximo de espera de {TimeoutSeconds} segundos.",
                timeoutSeconds);

            throw new TimeoutException(
                "Ollama tardó demasiado en responder.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(
                exception,
                "No fue posible establecer comunicación con Ollama.");

            throw;
        }
    }

    private static string ConvertirRol(string rol)
    {
        return rol.ToLowerInvariant() switch
        {
            "sistema" or "system" => "system",
            "usuario" or "user" => "user",
            "asistente" or "assistant" => "assistant",
            _ => throw new ArgumentOutOfRangeException(nameof(rol))
        };
    }
}