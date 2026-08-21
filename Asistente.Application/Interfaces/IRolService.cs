using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IRolService
{
    Task<IReadOnlyCollection<RolDto>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task<RolDto> CrearAsync(
        CrearRolRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task<RolDto> ActualizarAsync(
        int idRol,
        ActualizarRolRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task CambiarEstadoAsync(
        int idRol,
        bool activar,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);
}