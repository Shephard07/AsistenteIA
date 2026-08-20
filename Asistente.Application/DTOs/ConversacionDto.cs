using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public sealed class ConversacionDto
{
    public int IdConversacion { get; init; }

    public DateTime FechaInicio { get; init; }

    public DateTime? FechaFin { get; init; }

    public string Estado { get; init; } = string.Empty;

    public IReadOnlyCollection<MensajeDto> Mensajes { get; init; }
        = Array.Empty<MensajeDto>();
}