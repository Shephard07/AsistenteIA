namespace Asistente.Infrastructure.Options;

public class ProcesamientoDocumentalOptions
{
    public const string SectionName = "ProcesamientoDocumental";

    public int TamanoMaximoChunk { get; init; }

    public int SolapamientoChunk { get; init; }

    public int LongitudMinimaChunk { get; init; }

    public int FrecuenciaSegundos { get; init; }

    public int MaximoDocumentosPorCiclo { get; init; }
}