using Asistente.Domain.Entities;
using Asistente.Domain.Enums;

namespace Asistente.Domain.Interfaces;

public interface IDocumentoRepository
{
    Task<IReadOnlyCollection<Documento>> ListarAsync(
        string? terminoBusqueda,
        int? idCategoria,
        EstadoDocumento? estado,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        CancellationToken cancellationToken = default);

    Task<Documento?> ObtenerPorIdAsync(
        int idDocumento,
        CancellationToken cancellationToken = default);

    Task<Documento?> ObtenerPorCodigoAsync(
        string codigo,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Documento documento,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}