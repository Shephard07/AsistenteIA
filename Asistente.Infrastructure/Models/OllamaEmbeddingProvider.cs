using System.Net;
using System.Net.Http.Json;
using Asistente.Application.Interfaces;
using Asistente.Infrastructure.Models;
using Microsoft.Extensions.Logging;

namespace Asistente.Infrastructure.Services;

public class OllamaEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaEmbeddingProvider> _logger;

    public OllamaEmbeddingProvider(
        HttpClient httpClient,
        ILogger<OllamaEmbeddingProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<float[]> GenerarAsync(
        string texto,
        string modelo,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new ArgumentException(
                "El texto para generar el embedding es obligatorio.",
                nameof(texto));
        }

        if (string.IsNullOrWhiteSpace(modelo))
        {
            throw new ArgumentException(
                "El modelo de embeddings es obligatorio.",
                nameof(modelo));
        }

        var request = new OllamaEmbeddingRequest
        {
            Model = modelo.Trim(),
            Input = texto.Trim()
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "api/embed",
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException(
                $"El modelo de embeddings '{request.Model}' " +
                "no está instalado en Ollama.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var detalleError = await response.Content
                .ReadAsStringAsync(cancellationToken);

            _logger.LogError(
                "Ollama devolvió el estado {StatusCode} al generar " +
                "un embedding. Detalle: {DetalleError}",
                response.StatusCode,
                detalleError);

            throw new HttpRequestException(
                "No fue posible generar el embedding en Ollama.");
        }

        var resultado = await response.Content
            .ReadFromJsonAsync<OllamaEmbeddingResponse>(
                cancellationToken: cancellationToken);

        var embedding = resultado?.Embeddings.FirstOrDefault();

        if (embedding is null || embedding.Length == 0)
        {
            throw new InvalidOperationException(
                "Ollama no devolvió un embedding válido.");
        }

        if (embedding.Any(valor =>
                float.IsNaN(valor) || float.IsInfinity(valor)))
        {
            throw new InvalidOperationException(
                "Ollama devolvió un embedding con valores no válidos.");
        }

        return embedding;
    }
}