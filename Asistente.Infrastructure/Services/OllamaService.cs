using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Domain.ValueObjects;
using Asistente.Infrastructure.Models;
using Asistente.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asistente.Infrastructure.Services;

/// Implementa la comunicación HTTP con el servicio local Ollama.

public class OllamaService : IAsistenteIA
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

    public async Task<RespuestaIA> GenerarRespuestaAsync(
        IReadOnlyCollection<Mensaje> mensajes,
        CancellationToken cancellationToken = default)
    {
        var request = new OllamaChatRequest
        {
            Model = _options.Model,
            Stream = false,
            Think = false,
            KeepAlive = _options.KeepAlive,
            Messages = mensajes.Select(mensaje => new OllamaChatMessage
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
                request,
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

            return new RespuestaIA(
                respuesta.Message.Content.Trim(),
                (int)cronometro.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
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

    private static string ConvertirRol(RolMensaje rol)
    {
        return rol switch
        {
            RolMensaje.Usuario => "user",
            RolMensaje.Asistente => "assistant",
            _ => throw new ArgumentOutOfRangeException(nameof(rol))
        };
    }
}