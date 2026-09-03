namespace Asistente.Infrastructure.Options;

public class IndexacionDocumentalOptions
{
    public const string SectionName = "IndexacionDocumental";

    public int FrecuenciaSegundos { get; set; }

    public int MaximoDocumentosPorCiclo { get; set; }
}