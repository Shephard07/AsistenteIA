namespace Asistente.Application.DTOs;

public class PromptSistemaDto
{
    public int IdPrompt { get; init; }

    public int IdAsistente { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Contenido { get; init; } = string.Empty;

    public int Version { get; init; }

    public bool Activo { get; init; }

    public DateTime FechaCreacion { get; init; }

    public string UsuarioCreacion { get; init; } = string.Empty;
}