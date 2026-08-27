//ConfiguracionMemoriaDto.cs
namespace Asistente.Application.DTOs;

public class ConfiguracionMemoriaDto
{
    public int IdConfiguracion { get; init; }

    public int MaximoMensajesContexto { get; init; }

    public int MaximoTokensContexto { get; init; }

    public int LongitudResumen { get; init; }

    public int CantidadConversacionesVisibles { get; init; }

    public bool Activo { get; init; }
}