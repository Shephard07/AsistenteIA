namespace Asistente.Application.DTOs;

public class ConfiguracionProcesamientoDocumentoDto
{
    public int TamanoMaximoChunk { get; init; }

    public int SolapamientoChunk { get; init; }

    public int LongitudMinimaChunk { get; init; }

    public int FrecuenciaSegundos { get; init; }

    public int MaximoDocumentosPorCiclo { get; init; }
}