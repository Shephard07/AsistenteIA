using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IAIProvider
{
    Task<ChatResponseDto> SendAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default);
}