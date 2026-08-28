// DocumentoDetalleDto.cs
namespace Asistente.Application.DTOs;

public class DocumentoDetalleDto
{
    public int IdDocumento { get; init; }

    public string Codigo { get; init; } = string.Empty;

    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public int IdCategoria { get; init; }

    public string Categoria { get; init; } = string.Empty;

    public int VersionActual { get; init; }

    public string Estado { get; init; } = string.Empty;

    public string EstadoProcesamiento { get; init; } = string.Empty;

    public DateTime FechaRegistro { get; init; }

    public string UsuarioRegistro { get; init; } = string.Empty;

    public IReadOnlyCollection<DocumentoVersionDto> Versiones { get; init; }
        = Array.Empty<DocumentoVersionDto>();
}