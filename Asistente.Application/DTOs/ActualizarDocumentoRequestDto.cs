// ActualizarDocumentoRequestDto.cs
namespace Asistente.Application.DTOs;

public class ActualizarDocumentoRequestDto
{
    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public int IdCategoria { get; init; }
}