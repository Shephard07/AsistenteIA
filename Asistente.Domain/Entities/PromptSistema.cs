using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Domain.Entities;

/// <summary>
/// Representa una versión inmutable de un Prompt del Sistema.
/// </summary>
public class PromptSistema
{
    private PromptSistema()
    {
    }

    public PromptSistema(
        int idAsistente,
        string nombre,
        string contenido,
        int version,
        bool activo,
        string usuarioCreacion)
    {
        IdAsistente = idAsistente;
        Nombre = nombre;
        Contenido = contenido;
        Version = version;
        Activo = activo;
        UsuarioCreacion = usuarioCreacion;
        FechaCreacion = DateTime.UtcNow;
    }

    public int IdPrompt { get; private set; }

    public int IdAsistente { get; private set; }

    public Asistente Asistente { get; private set; } = null!;

    public string Nombre { get; private set; } = string.Empty;

    public string Contenido { get; private set; } = string.Empty;

    public int Version { get; private set; }

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public string UsuarioCreacion { get; private set; } = string.Empty;

    public ICollection<HistorialPrompt> Historiales { get; private set; }
        = new List<HistorialPrompt>();

    public void CambiarEstado(bool activo)
    {
        Activo = activo;
    }
}