namespace Asistente.Application.DTOs;

public class HistorialPromptDto
{
    public int IdHistorial { get; init; }

    public int IdPrompt { get; init; }

    public int Version { get; init; }

    public string Contenido { get; init; } = string.Empty;

    public DateTime FechaModificacion { get; init; }

    public string UsuarioModificacion { get; init; } = string.Empty;

    public string MotivoCambio { get; init; } = string.Empty;
}