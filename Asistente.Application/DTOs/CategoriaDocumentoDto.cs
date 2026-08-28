// CategoriaDocumentoDto.cs
namespace Asistente.Application.DTOs;

public class CategoriaDocumentoDto
{
    public int IdCategoria { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public bool Activo { get; init; }

    public DateTime FechaCreacion { get; init; }
}