using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IDocumentoChunkRepository
{
    Task<IReadOnlyCollection<DocumentoChunk>>
        ListarPorDocumentoYVersionAsync(
            int idDocumento,
            int idVersionDocumento,
            CancellationToken cancellationToken = default);
}