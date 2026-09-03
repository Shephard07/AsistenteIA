using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IAdministracionRagService
{
    Task<EstadoRagDto> ObtenerEstadoAsync(
        CancellationToken cancellationToken = default);
}