//IAuditoriaService.cs
using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IAuditoriaService
{
    Task<IReadOnlyCollection<AuditoriaSesionDto>> ListarSesionesAsync(
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AuditoriaActividadDto>> ListarActividadesAsync(
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);
}