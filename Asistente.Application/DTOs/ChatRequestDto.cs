using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public sealed class ChatRequestDto
{
    public IReadOnlyCollection<MensajeDto> Mensajes { get; init; }
        = Array.Empty<MensajeDto>();
}