using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public sealed class MensajeDto
{
    public int IdMensaje { get; init; }

    public int IdConversacion { get; init; }

    public string Rol { get; init; } = string.Empty;

    public string Contenido { get; init; } = string.Empty;

    public DateTime FechaHora { get; init; }

    public int? TiempoRespuestaMs { get; init; }
}