using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IVectorStore
{
    Task IndexarAsync(
        DocumentoVectorialDto documento,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ResultadoBusquedaVectorialDto>>
        BuscarAsync(
            BusquedaVectorialRequestDto request,
            CancellationToken cancellationToken = default);

    Task EliminarPorDocumentoAsync(
        Guid identificadorDocumentoIndexado,
        CancellationToken cancellationToken = default);
}