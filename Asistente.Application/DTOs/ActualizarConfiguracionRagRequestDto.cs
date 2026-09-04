namespace Asistente.Application.DTOs;

public sealed class ActualizarConfiguracionRagRequestDto
{
    public string ModeloEmbeddings { get; init; } = string.Empty;

    public int CantidadResultados { get; init; }

    public decimal PuntajeMinimo { get; init; }

    public int LongitudMaximaContexto { get; init; }
}