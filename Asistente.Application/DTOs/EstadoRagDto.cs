using Asistente.Domain.Enums;

namespace Asistente.Application.DTOs;

public sealed class EstadoRagDto
{
    public ConfiguracionRagDto Configuracion { get; init; } = new();

    public int TotalDocumentos { get; init; }

    public int TotalPendientes { get; init; }

    public int TotalEnProceso { get; init; }

    public int TotalIndexados { get; init; }

    public int TotalConError { get; init; }

    public IReadOnlyCollection<DocumentoIndexadoEstadoDto> Documentos { get; init; }
        = Array.Empty<DocumentoIndexadoEstadoDto>();
}