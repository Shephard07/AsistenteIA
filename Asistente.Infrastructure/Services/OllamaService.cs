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
        var ollamaRequest = new OllamaChatRequest
        {
            Model = _options.Model,
            Stream = false,
            Think = false,
            KeepAlive = _options.KeepAlive,
            Messages = request.Mensajes.Select(mensaje => new OllamaChatMessage
            {
                Role = ConvertirRol(mensaje.Rol),
                Content = mensaje.Contenido
            }).ToList()
        };

        var cronometro = Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/chat",
                ollamaRequest,
                cancellationToken);

            cronometro.Stop();

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException(
                    $"El modelo '{_options.Model}' no está instalado en Ollama.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var detalleError = await response.Content.ReadAsStringAsync(
                    cancellationToken);

                _logger.LogError(
                    "Ollama devolvió el estado {StatusCode}. Detalle: {DetalleError}",
                    response.StatusCode,
                    detalleError);

                throw new HttpRequestException(
                    "No fue posible obtener una respuesta de Ollama.");
            }

            var respuesta = await response.Content
                .ReadFromJsonAsync<OllamaChatResponse>(
                    cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(respuesta?.Message?.Content))
            {
                throw new InvalidOperationException(
                    "Ollama no devolvió una respuesta válida.");
            }

            return new ChatResponseDto
            {
                Contenido = respuesta.Message.Content.Trim(),
                TiempoRespuestaMs = (int)cronometro.ElapsedMilliseconds
            };
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                "Ollama excedió el tiempo máximo de espera de {TimeoutSeconds} segundos.",
                _options.TimeoutSeconds);

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
            "usuario" or "user" => "user",
            "asistente" or "assistant" => "assistant",
            _ => throw new ArgumentOutOfRangeException(nameof(rol))
        };
    }
}