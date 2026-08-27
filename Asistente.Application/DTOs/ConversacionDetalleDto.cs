namespace Asistente.Application.DTOs;

public class ConversacionDetalleDto
{
    public int IdConversacion { get; init; }

    public int? IdAsistente { get; init; }

    public string Titulo { get; init; } = string.Empty;

    public DateTime FechaInicio { get; init; }

    public DateTime? FechaFin { get; init; }

    public DateTime FechaUltimaActividad { get; init; }

    public string? ResumenContexto { get; init; }

    public int TotalMensajes { get; init; }

    public string Estado { get; init; } = string.Empty;

    public IReadOnlyCollection<MensajeDto> Mensajes { get; init; }
        = Array.Empty<MensajeDto>();
}