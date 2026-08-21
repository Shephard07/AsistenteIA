using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task<Usuario?> ObtenerPorNombreUsuarioAsync(
        string nombreUsuario,
        CancellationToken cancellationToken = default);

    Task<Usuario?> ObtenerPorNombreUsuarioConRolesAsync(
        string nombreUsuario,
        CancellationToken cancellationToken = default);

    Task<Usuario?> ObtenerConRolesPorIdAsync(
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Usuario>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreUsuarioAsync(
        string nombreUsuario,
        int? idUsuarioExcluir = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteCorreoAsync(
        string correo,
        int? idUsuarioExcluir = null,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Usuario usuario,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}