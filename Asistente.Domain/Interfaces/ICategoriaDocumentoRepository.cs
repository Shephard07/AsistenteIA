using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface ICategoriaDocumentoRepository
{
    Task<IReadOnlyCollection<CategoriaDocumento>> ListarAsync(
        bool soloActivas,
        CancellationToken cancellationToken = default);

    Task<CategoriaDocumento?> ObtenerPorIdAsync(
        int idCategoria,
        CancellationToken cancellationToken = default);

    Task<CategoriaDocumento?> ObtenerPorNombreAsync(
        string nombre,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        CategoriaDocumento categoria,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}