// CrearCategoriaDocumentoRequestDto.cs
namespace Asistente.Application.DTOs;

public class CrearCategoriaDocumentoRequestDto
{
    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;
}