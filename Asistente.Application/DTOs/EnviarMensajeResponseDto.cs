using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public sealed class EnviarMensajeResponseDto
{
    public int IdConversacion { get; init; }

    public string Respuesta { get; init; } = string.Empty;

    public int TiempoRespuestaMs { get; init; }
}