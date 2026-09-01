using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IExtractorTextoDocumento
{
    bool Soporta(string nombreArchivo);

    Task<IReadOnlyCollection<PaginaTextoDocumentoDto>>
        ExtraerAsync(
            Stream contenido,
            CancellationToken cancellationToken = default);
}