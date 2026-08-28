// CrearDocumentoRequestDto.cs
namespace Asistente.Application.DTOs;

public class CrearDocumentoRequestDto
{
    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public int IdCategoria { get; init; }
}