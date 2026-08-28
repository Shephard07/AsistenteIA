using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IDepuracionContextoService
{
    Task<ContextoDepuracionDto> ObtenerAsync(
        int idConversacion,
        CancellationToken cancellationToken = default);
}