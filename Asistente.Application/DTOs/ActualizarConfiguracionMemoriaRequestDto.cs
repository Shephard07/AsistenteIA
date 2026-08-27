namespace Asistente.Application.DTOs;

public class ActualizarConfiguracionMemoriaRequestDto
{
    public int MaximoMensajesContexto { get; init; }

    public int MaximoTokensContexto { get; init; }

    public int LongitudResumen { get; init; }

    public int CantidadConversacionesVisibles { get; init; }
}