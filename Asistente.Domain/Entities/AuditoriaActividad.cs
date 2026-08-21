using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Domain.Entities;

public class AuditoriaActividad
{
    public int IdActividad { get; private set; }

    public int? IdUsuario { get; private set; }

    public DateTime FechaHora { get; private set; }

    public string Modulo { get; private set; } = string.Empty;

    public string Accion { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public string DireccionIP { get; private set; } = string.Empty;

    public Usuario? Usuario { get; private set; }

    private AuditoriaActividad()
    {
    }

    public AuditoriaActividad(
        int? idUsuario,
        string modulo,
        string accion,
        string descripcion,
        string direccionIP)
    {
        IdUsuario = idUsuario;
        Modulo = modulo;
        Accion = accion;
        Descripcion = descripcion;
        DireccionIP = direccionIP;
        FechaHora = DateTime.UtcNow;
    }
}