namespace Asistente.Application.DTOs;

public class CrearVersionPromptRequestDto
{
    public string Nombre { get; init; } = string.Empty;

    public string Contenido { get; init; } = string.Empty;

    public string MotivoCambio { get; init; } = string.Empty;
}