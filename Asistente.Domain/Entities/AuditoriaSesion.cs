using Asistente.Domain.Enums;

namespace Asistente.Domain.Entities;

public class AuditoriaSesion
{
    public int IdSesion { get; private set; }

    public int IdUsuario { get; private set; }

    public DateTime FechaInicio { get; private set; }

    public DateTime? FechaFin { get; private set; }

    public string DireccionIP { get; private set; } = string.Empty;

    public string Navegador { get; private set; } = string.Empty;

    public EstadoSesion Estado { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    private AuditoriaSesion()
    {
    }

    public AuditoriaSesion(
        int idUsuario,
        string direccionIP,
        string navegador)
    {
        IdUsuario = idUsuario;
        DireccionIP = direccionIP;
        Navegador = navegador;
        FechaInicio = DateTime.UtcNow;
        Estado = EstadoSesion.Activa;
    }

    public void Cerrar()
    {
        FechaFin = DateTime.UtcNow;
        Estado = EstadoSesion.Cerrada;
    }
}