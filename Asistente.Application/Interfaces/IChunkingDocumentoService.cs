using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IChunkingDocumentoService
{
    IReadOnlyCollection<ChunkTextoDocumentoDto> GenerarChunks(
        IReadOnlyCollection<PaginaTextoDocumentoDto> paginas,
        ConfiguracionProcesamientoDocumentoDto configuracion);
}