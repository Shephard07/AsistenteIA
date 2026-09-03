namespace Asistente.Infrastructure.Models;

public class OllamaEmbeddingResponse
{
    public List<float[]> Embeddings { get; init; }
        = new List<float[]>();
}