using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IConfiguracionMemoriaService
{
    Task<ConfiguracionMemoriaDto> ObtenerActivaAsync(
        CancellationToken cancellationToken = default);

    Task<ConfiguracionMemoriaDto> ActualizarAsync(
        ActualizarConfiguracionMemoriaRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);
}