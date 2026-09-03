namespace Asistente.Application.DTOs;

public class ContextoRagDto
{
    public string Contenido { get; init; } = string.Empty;

    public IReadOnlyCollection<FragmentoContextoRagDto>
        Fragmentos
    { get; init; }
        = Array.Empty<FragmentoContextoRagDto>();

    public bool TieneResultados => Fragmentos.Count > 0;
}