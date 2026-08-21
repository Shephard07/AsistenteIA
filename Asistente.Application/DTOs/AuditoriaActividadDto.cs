using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public class AuditoriaActividadDto
{
    public int IdActividad { get; init; }

    public int? IdUsuario { get; init; }

    public string Usuario { get; init; } = string.Empty;

    public DateTime FechaHora { get; init; }

    public string Modulo { get; init; } = string.Empty;

    public string Accion { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public string DireccionIP { get; init; } = string.Empty;
}