using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Domain.Entities;

/// <summary>
/// Conserva la trazabilidad inmutable de cada versión de Prompt.
/// </summary>
public class HistorialPrompt
{
    private HistorialPrompt()
    {
    }

    public HistorialPrompt(
        int idPrompt,
        int version,
        string contenido,
        string usuarioModificacion,
        string motivoCambio)
    {
        IdPrompt = idPrompt;
        Version = version;
        Contenido = contenido;
        UsuarioModificacion = usuarioModificacion;
        MotivoCambio = motivoCambio;
        FechaModificacion = DateTime.UtcNow;
    }

    public int IdHistorial { get; private set; }

    public int IdPrompt { get; private set; }

    public PromptSistema PromptSistema { get; private set; } = null!;

    public int Version { get; private set; }

    public string Contenido { get; private set; } = string.Empty;

    public DateTime FechaModificacion { get; private set; }

    public string UsuarioModificacion { get; private set; } = string.Empty;

    public string MotivoCambio { get; private set; } = string.Empty;
}