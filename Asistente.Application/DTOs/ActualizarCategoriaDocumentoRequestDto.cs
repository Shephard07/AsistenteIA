// ActualizarCategoriaDocumentoRequestDto.cs
namespace Asistente.Application.DTOs;

public class ActualizarCategoriaDocumentoRequestDto
{
    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;
}