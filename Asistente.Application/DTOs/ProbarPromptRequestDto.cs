namespace Asistente.Application.DTOs;

public class ProbarPromptRequestDto
{
    public int IdAsistente { get; init; }

    public int IdPrompt { get; init; }

    public string Mensaje { get; init; } = string.Empty;
}