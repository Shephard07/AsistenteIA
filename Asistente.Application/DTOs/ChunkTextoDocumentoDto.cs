namespace Asistente.Application.DTOs;

public class ChunkTextoDocumentoDto
{
    public int PaginaInicial { get; init; }

    public int PaginaFinal { get; init; }

    public string Texto { get; init; } = string.Empty;
}