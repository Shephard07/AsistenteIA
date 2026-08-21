using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public class AuditoriaSesionDto
{
    public int IdSesion { get; init; }

    public int IdUsuario { get; init; }

    public string Usuario { get; init; } = string.Empty;

    public DateTime FechaInicio { get; init; }

    public DateTime? FechaFin { get; init; }

    public string DireccionIP { get; init; } = string.Empty;

    public string Navegador { get; init; } = string.Empty;

    public string Estado { get; init; } = string.Empty;
}