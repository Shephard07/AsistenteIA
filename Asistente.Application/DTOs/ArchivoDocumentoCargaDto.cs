// ArchivoDocumentoCargaDto.cs
namespace Asistente.Application.DTOs;

public class ArchivoDocumentoCargaDto
{
    public string NombreArchivo { get; init; } = string.Empty;

    public string TipoContenido { get; init; } = string.Empty;

    public long TamanoArchivo { get; init; }

    public Stream Contenido { get; init; } = Stream.Null;
}