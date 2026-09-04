namespace Asistente.Application.DTOs;

public sealed class DocumentoChunkDetalleDto
{
    public int IdChunk { get; init; }

    public int IdDocumento { get; init; }

    public string CodigoDocumento { get; init; } = string.Empty;

    public string NombreDocumento { get; init; } = string.Empty;

    public int IdVersionDocumento { get; init; }

    public int NumeroVersion { get; init; }

    public int IdCategoria { get; init; }

    public string Categoria { get; init; } = string.Empty;

    public int NumeroChunk { get; init; }

    public int Orden { get; init; }

    public int PaginaInicial { get; init; }

    public int PaginaFinal { get; init; }

    public int TotalCaracteres { get; init; }

    public string Texto { get; init; } = string.Empty;
}