namespace Asistente.Application.DTOs;

public class ProcesamientoDocumentoDto
{
    public int IdVersionDocumento { get; init; }

    public string Estado { get; init; } = string.Empty;

    public DateTime? FechaInicio { get; init; }

    public DateTime? FechaFin { get; init; }

    public int TotalPaginas { get; init; }

    public int TotalCaracteres { get; init; }

    public int TotalChunks { get; init; }

    public string Observaciones { get; init; } = string.Empty;
}