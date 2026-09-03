namespace Asistente.Application.DTOs;

public sealed class ConfiguracionRagDto
{
    public string Proveedor { get; init; } = string.Empty;

    public string ModeloEmbeddings { get; init; } = string.Empty;

    public string BaseVectorial { get; init; } = string.Empty;

    public int CantidadResultados { get; init; }

    public decimal PuntajeMinimo { get; init; }

    public int LongitudMaximaContexto { get; init; }
}