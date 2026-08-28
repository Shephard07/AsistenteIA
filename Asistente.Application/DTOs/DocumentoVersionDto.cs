// DocumentoVersionDto.cs
namespace Asistente.Application.DTOs;

public class DocumentoVersionDto
{
    public int IdVersion { get; init; }

    public int NumeroVersion { get; init; }

    public string NombreArchivo { get; init; } = string.Empty;

    public long TamanoArchivo { get; init; }

    public string HashArchivo { get; init; } = string.Empty;

    public DateTime FechaCarga { get; init; }

    public string UsuarioCarga { get; init; } = string.Empty;

    public bool Activo { get; init; }
}