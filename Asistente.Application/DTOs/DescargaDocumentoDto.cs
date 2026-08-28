// DescargaDocumentoDto.cs
namespace Asistente.Application.DTOs;

public class DescargaDocumentoDto
{
    public string NombreArchivo { get; init; } = string.Empty;

    public string TipoContenido { get; init; } = "application/pdf";

    public Stream Contenido { get; init; } = Stream.Null;
}