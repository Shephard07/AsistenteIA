//IConversacionRepository.cs
using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IConversacionRepository
{
    Task<Conversacion?> ObtenerPorIdAsync(
        int idConversacion,
        CancellationToken cancellationToken = default);

    Task<Conversacion?> ObtenerPorIdYUsuarioAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Conversacion>> ListarPorUsuarioAsync(
        int idUsuario,
        string? terminoBusqueda,
        bool incluirArchivadas,
        int cantidadMaxima,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Conversacion conversacion,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}