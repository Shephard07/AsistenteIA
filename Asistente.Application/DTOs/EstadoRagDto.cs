namespace Asistente.Application.DTOs;

public sealed class EstadoRagDto
{
    public ConfiguracionRagDto Configuracion { get; init; } = new();

    public bool BaseVectorialDisponible { get; init; }

    public int TotalDocumentos { get; init; }

    public int TotalPendientes { get; init; }

    public int TotalEnProceso { get; init; }

    public int TotalIndexados { get; init; }

    public int TotalConError { get; init; }

    public int TotalChunks { get; init; }

    public int TotalEmbeddings { get; init; }

    public decimal TiempoPromedioIndexacionSegundos { get; init; }

    public IReadOnlyCollection<DocumentoIndexadoEstadoDto> Documentos { get; init; }
        = Array.Empty<DocumentoIndexadoEstadoDto>();
}