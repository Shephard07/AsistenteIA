namespace Asistente.Application.Interfaces;

public interface IEmbeddingProvider
{
    Task<float[]> GenerarAsync(
        string texto,
        string modelo,
        CancellationToken cancellationToken = default);
}