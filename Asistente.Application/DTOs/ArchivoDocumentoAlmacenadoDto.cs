// ArchivoDocumentoAlmacenadoDto.cs
namespace Asistente.Application.DTOs;

public class ArchivoDocumentoAlmacenadoDto
{
    public string NombreArchivo { get; init; } = string.Empty;

    public string RutaArchivo { get; init; } = string.Empty;

    public long TamanoArchivo { get; init; }

    public string HashArchivo { get; init; } = string.Empty;
}