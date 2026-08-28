using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

/// <summary>
/// Gestiona el historial y los cambios de estado de las conversaciones.
/// </summary>
public class ConversacionGestionService : IConversacionGestionService
{
    private readonly IConversacionRepository _conversacionRepository;

    public ConversacionGestionService(
        IConversacionRepository conversacionRepository)
    {
        _conversacionRepository = conversacionRepository;
    }

    public async Task<IReadOnlyCollection<ConversacionHistorialDto>> ListarAsync(
        int idUsuario,
        string? terminoBusqueda,
        bool incluirArchivadas,
        int cantidadMaxima,
        CancellationToken cancellationToken = default)
    {
        ValidarIdUsuario(idUsuario);

        if (cantidadMaxima is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadMaxima),
                "La cantidad máxima debe estar entre 1 y 100.");
        }

        var conversaciones = await _conversacionRepository
            .ListarPorUsuarioAsync(
                idUsuario,
                terminoBusqueda,
                incluirArchivadas,
                cantidadMaxima,
                cancellationToken);

        return conversaciones
            .Select(ConvertirAHistorialDto)
            .ToArray();
    }

    public async Task<ConversacionDetalleDto> ObtenerDetalleAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var conversacion = await ObtenerDelUsuarioAsync(
            idConversacion,
            idUsuario,
            cancellationToken);

        return ConvertirADetalleDto(conversacion);
    }

    public async Task RenombrarAsync(
        int idConversacion,
        int idUsuario,
        string titulo,
        CancellationToken cancellationToken = default)
    {
        var conversacion = await ObtenerDelUsuarioAsync(
            idConversacion,
            idUsuario,
            cancellationToken);

        conversacion.Renombrar(titulo);

        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    public async Task ArchivarAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var conversacion = await ObtenerDelUsuarioAsync(
            idConversacion,
            idUsuario,
            cancellationToken);

        conversacion.Archivar();

        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    public async Task ReactivarAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var conversacion = await ObtenerDelUsuarioAsync(
            idConversacion,
            idUsuario,
            cancellationToken);

        conversacion.Reactivar();

        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    public async Task EliminarAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var conversacion = await ObtenerDelUsuarioAsync(
            idConversacion,
            idUsuario,
            cancellationToken);

        conversacion.Eliminar();

        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private async Task<Conversacion> ObtenerDelUsuarioAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken)
    {
        ValidarIdUsuario(idUsuario);

        if (idConversacion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idConversacion),
                "El identificador de la conversación debe ser mayor que cero.");
        }

        return await _conversacionRepository
            .ObtenerPorIdYUsuarioAsync(
                idConversacion,
                idUsuario,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "La conversación solicitada no existe.");
    }

    private static ConversacionHistorialDto ConvertirAHistorialDto(
        Conversacion conversacion)
    {
        return new ConversacionHistorialDto
        {
            IdConversacion = conversacion.IdConversacion,
            Titulo = conversacion.Titulo ?? "Nueva conversación",
            Estado = conversacion.Estado.ToString(),
            FechaInicio = conversacion.FechaInicio,
            FechaUltimaActividad = conversacion.FechaUltimaActividad,
            TotalMensajes = conversacion.TotalMensajes,
            ResumenContexto = conversacion.ResumenContexto
        };
    }

    private static ConversacionDetalleDto ConvertirADetalleDto(
        Conversacion conversacion)
    {
        return new ConversacionDetalleDto
        {
            IdConversacion = conversacion.IdConversacion,
            IdAsistente = conversacion.IdAsistente,
            Titulo = conversacion.Titulo ?? "Nueva conversación",
            FechaInicio = conversacion.FechaInicio,
            FechaFin = conversacion.FechaFin,
            FechaUltimaActividad = conversacion.FechaUltimaActividad,
            ResumenContexto = conversacion.ResumenContexto,
            TotalMensajes = conversacion.TotalMensajes,
            Estado = conversacion.Estado.ToString(),
            Mensajes = conversacion.Mensajes
                .OrderBy(mensaje => mensaje.FechaHora)
                .Select(mensaje => new MensajeDto
                {
                    IdMensaje = mensaje.IdMensaje,
                    IdConversacion = mensaje.IdConversacion,
                    Rol = mensaje.Rol.ToString(),
                    Contenido = mensaje.Contenido,
                    FechaHora = mensaje.FechaHora,
                    TiempoRespuestaMs = mensaje.TiempoRespuestaMs
                })
                .ToArray()
        };
    }

    private static void ValidarIdUsuario(int idUsuario)
    {
        if (idUsuario <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idUsuario),
                "El identificador del usuario debe ser mayor que cero.");
        }
    }
}