using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IDocumentoIndexadoRepository
{
    Task<IReadOnlyCollection<DocumentoProcesado>>
        ObtenerProcesamientosPendientesAsync(
            int cantidadMaxima,
            CancellationToken cancellationToken = default);

    Task<DocumentoIndexado?> ObtenerPorProcesamientoAsync(
        int idDocumentoProcesado,
        CancellationToken cancellationToken = default);

    Task<DocumentoProcesado?> ObtenerProcesamientoActivoPorDocumentoAsync(
    int idDocumento,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<DocumentoProcesado>>
    ListarProcesamientosAsync(
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        DocumentoIndexado documentoIndexado,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}