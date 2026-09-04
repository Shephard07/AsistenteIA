using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IEmbeddingConfiguracionRepository
{
    Task<EmbeddingConfiguracion> ObtenerActivaAsync(
        CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(
    CancellationToken cancellationToken = default);
}