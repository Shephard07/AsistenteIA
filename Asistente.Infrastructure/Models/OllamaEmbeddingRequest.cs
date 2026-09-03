namespace Asistente.Infrastructure.Models;

public class OllamaEmbeddingRequest
{
    public string Model { get; init; } = string.Empty;

    public string Input { get; init; } = string.Empty;
}