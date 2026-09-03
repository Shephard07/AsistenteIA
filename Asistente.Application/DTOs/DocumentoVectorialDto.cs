using System;

namespace Asistente.Application.DTOs;

public class DocumentoVectorialDto
{
    public Guid IdentificadorDocumentoIndexado { get; init; }

    public int IdDocumento { get; init; }

    public int IdVersionDocumento { get; init; }

    public int IdDocumentoProcesado { get; init; }

    public int IdCategoria { get; init; }

    public int NumeroChunk { get; init; }

    public int PaginaInicial { get; init; }

    public int PaginaFinal { get; init; }

    public string Texto { get; init; } = string.Empty;

    public float[] Embedding { get; init; } = Array.Empty<float>();

    public string IdVector =>
        $"{IdentificadorDocumentoIndexado:N}-{NumeroChunk}";
}