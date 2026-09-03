using System;

namespace Asistente.Application.DTOs;

public class ResultadoBusquedaVectorialDto
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

    public decimal Puntaje { get; init; }
}