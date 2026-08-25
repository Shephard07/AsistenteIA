using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IAsistenteService
{
    Task<IReadOnlyCollection<AsistenteDto>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task<AsistenteDto> ObtenerPorIdAsync(
        int idAsistente,
        CancellationToken cancellationToken = default);

    Task<AsistenteDto> ObtenerActivoAsync(
        CancellationToken cancellationToken = default);

    Task<AsistenteDto> CrearAsync(
        CrearAsistenteRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task<AsistenteDto> ActualizarAsync(
        int idAsistente,
        ActualizarAsistenteRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task CambiarEstadoAsync(
        int idAsistente,
        bool activo,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);
}