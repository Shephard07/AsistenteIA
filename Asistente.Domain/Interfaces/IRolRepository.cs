using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IRolRepository
{
    Task<Rol?> ObtenerPorIdAsync(
        int idRol,
        CancellationToken cancellationToken = default);

    Task<Rol?> ObtenerPorNombreAsync(
        string nombre,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Rol>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNombreAsync(
        string nombre,
        int? idRolExcluir = null,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Rol rol,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}