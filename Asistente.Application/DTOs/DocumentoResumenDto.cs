// DocumentoResumenDto.cs
namespace Asistente.Application.DTOs;

public class DocumentoResumenDto
{
    public int IdDocumento { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Categoria { get; init; } = string.Empty;

    public int VersionActual { get; init; }

    public string Estado { get; init; } = string.Empty;

    public string EstadoProcesamiento { get; init; } = string.Empty;

    public DateTime FechaRegistro { get; init; }

    public string UsuarioRegistro { get; init; } = string.Empty;
}