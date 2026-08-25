namespace Asistente.Application.DTOs;

public class ProbarPromptResponseDto
{
    public string PromptGenerado { get; init; } = string.Empty;

    public string Respuesta { get; init; } = string.Empty;

    public int TiempoRespuestaMs { get; init; }
}