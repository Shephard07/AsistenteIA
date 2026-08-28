using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IAlmacenamientoDocumentoService
{
    Task<ArchivoDocumentoAlmacenadoDto> GuardarAsync(
        ArchivoDocumentoCargaDto archivo,
        CancellationToken cancellationToken = default);

    Task<Stream> AbrirLecturaAsync(
        string rutaArchivo,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(
        string rutaArchivo,
        CancellationToken cancellationToken = default);
}