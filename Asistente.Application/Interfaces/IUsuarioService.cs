using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IUsuarioService
{
    Task<IReadOnlyCollection<UsuarioDto>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task<UsuarioDto> ObtenerPorIdAsync(
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task<UsuarioDto> CrearAsync(
        CrearUsuarioRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task<UsuarioDto> ActualizarAsync(
        int idUsuario,
        ActualizarUsuarioRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task<UsuarioDto> AsignarRolesAsync(
        int idUsuario,
        AsignarRolesUsuarioRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task CambiarPasswordAsync(
        int idUsuario,
        CambiarPasswordRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task CambiarEstadoAsync(
        int idUsuario,
        bool activar,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);
}