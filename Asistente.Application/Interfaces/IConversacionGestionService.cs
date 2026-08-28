using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface IConversacionGestionService
{
    Task<IReadOnlyCollection<ConversacionHistorialDto>> ListarAsync(
        int idUsuario,
        string? terminoBusqueda,
        bool incluirArchivadas,
        int cantidadMaxima,
        CancellationToken cancellationToken = default);

    Task<ConversacionDetalleDto> ObtenerDetalleAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task RenombrarAsync(
        int idConversacion,
        int idUsuario,
        string titulo,
        CancellationToken cancellationToken = default);

    Task ArchivarAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task ReactivarAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default);
}