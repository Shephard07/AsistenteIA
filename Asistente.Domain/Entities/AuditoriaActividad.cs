using System;

namespace Asistente.Domain.Entities;

public class AuditoriaActividad
{
    public int IdActividad { get; private set; }

    public int? IdUsuario { get; private set; }

    public int? IdDocumento { get; private set; }

    public DateTime FechaHora { get; private set; }

    public string Modulo { get; private set; } = string.Empty;

    public string Accion { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public string DireccionIP { get; private set; } = string.Empty;

    public Usuario? Usuario { get; private set; }

    public Documento? Documento { get; private set; }

    private AuditoriaActividad()
    {
    }

    public AuditoriaActividad(
        int? idUsuario,
        string modulo,
        string accion,
        string descripcion,
        string direccionIP,
        int? idDocumento = null)
    {
        IdUsuario = idUsuario;
        IdDocumento = idDocumento;
        Modulo = modulo;
        Accion = accion;
        Descripcion = descripcion;
        DireccionIP = direccionIP;
        FechaHora = DateTime.UtcNow;
    }
}