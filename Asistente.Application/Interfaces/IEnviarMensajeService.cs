using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IEnviarMensajeService
{
    Task<EnviarMensajeResponseDto> EjecutarAsync(
        EnviarMensajeRequestDto request,
        CancellationToken cancellationToken = default);
}