using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IAdministracionRagService
{
    Task<EstadoRagDto> ObtenerEstadoAsync(
        CancellationToken cancellationToken = default);

    Task<ConfiguracionRagDto> ActualizarConfiguracionAsync(
    ActualizarConfiguracionRagRequestDto request,
    int idUsuarioActor,
    ContextoClienteDto contextoCliente,
    CancellationToken cancellationToken = default);

    Task SolicitarReindexacionAsync(
    int idDocumento,
    int idUsuarioActor,
    ContextoClienteDto contextoCliente,
    CancellationToken cancellationToken = default);
}