namespace Asistente.Application.DTOs;

public class ConversacionHistorialDto
{
    public int IdConversacion { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaUltimaActividad { get; set; }

    public int TotalMensajes { get; set; }

    public string? ResumenContexto { get; set; }
}