using Asistente.Domain.Enums;

namespace Asistente.Application.DTOs;

public sealed class DocumentoIndexadoEstadoDto
{
    public int IdDocumento { get; init; }

    public string CodigoDocumento { get; init; } = string.Empty;

    public string NombreDocumento { get; init; } = string.Empty;

    public int IdVersionDocumento { get; init; }

    public int NumeroVersion { get; init; }

    public int IdDocumentoProcesado { get; init; }

    public int? IdDocumentoIndexado { get; init; }

    public EstadoIndexacionDocumento Estado { get; init; }

    public int TotalChunks { get; init; }

    public int TotalEmbeddings { get; init; }

    public DateTime? FechaInicio { get; init; }

    public DateTime? FechaIndexacion { get; init; }

    public string Observaciones { get; init; } = string.Empty;
}