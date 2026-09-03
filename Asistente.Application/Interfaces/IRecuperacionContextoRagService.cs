using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IRecuperacionContextoRagService
{
    Task<ContextoRagDto> RecuperarAsync(
        string consulta,
        CancellationToken cancellationToken = default);
}