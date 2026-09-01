using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IDocumentoProcesadoRepository
{
    Task<IReadOnlyCollection<DocumentoVersion>>
        ObtenerVersionesPendientesAsync(
            int cantidadMaxima,
            CancellationToken cancellationToken = default);

    Task<DocumentoProcesado?> ObtenerPorVersionAsync(
        int idVersionDocumento,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        DocumentoProcesado procesamiento,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}