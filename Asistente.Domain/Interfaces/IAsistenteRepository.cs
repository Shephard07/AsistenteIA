using AsistenteEntity = Asistente.Domain.Entities.Asistente;

namespace Asistente.Domain.Interfaces;

public interface IAsistenteRepository
{
    Task AgregarAsync(
        AsistenteEntity asistente,
        CancellationToken cancellationToken = default);

    Task<AsistenteEntity?> ObtenerPorIdAsync(
        int idAsistente,
        CancellationToken cancellationToken = default);

    Task<AsistenteEntity?> ObtenerActivoAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AsistenteEntity>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}