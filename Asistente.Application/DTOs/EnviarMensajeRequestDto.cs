using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public sealed class EnviarMensajeRequestDto
{
    public int? IdConversacion { get; init; }

    public string Mensaje { get; init; } = string.Empty;
}