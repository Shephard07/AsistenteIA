using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

/// <summary>
/// Genera un resumen acumulado cuando una conversación supera
/// la cantidad de mensajes permitidos en el contexto.
/// </summary>
public class ResumenConversacionService
    : IResumenConversacionService
{
    private readonly IAIProvider _aiProvider;
    private readonly IConversacionRepository _conversacionRepository;

    public ResumenConversacionService(
        IAIProvider aiProvider,
        IConversacionRepository conversacionRepository)
    {
        _aiProvider = aiProvider;
        _conversacionRepository = conversacionRepository;
    }

    public async Task ActualizarSiEsNecesarioAsync(
        Conversacion conversacion,
        AsistenteDto asistente,
        ConfiguracionMemoriaDto configuracionMemoria,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversacion);
        ArgumentNullException.ThrowIfNull(asistente);
        ArgumentNullException.ThrowIfNull(configuracionMemoria);

        var cantidadMensajesPorResumir =
            conversacion.TotalMensajes -
            configuracionMemoria.MaximoMensajesContexto -
            conversacion.TotalMensajesResumidos;

        if (cantidadMensajesPorResumir <= 0)
        {
            return;
        }

        var mensajesPorResumir = conversacion.Mensajes
            .OrderBy(mensaje => mensaje.FechaHora)
            .Skip(conversacion.TotalMensajesResumidos)
            .Take(cantidadMensajesPorResumir)
            .Select(mensaje => new MensajeDto
            {
                IdMensaje = mensaje.IdMensaje,
                IdConversacion = mensaje.IdConversacion,
                Rol = mensaje.Rol.ToString(),
                Contenido = mensaje.Contenido,
                FechaHora = mensaje.FechaHora,
                TiempoRespuestaMs = mensaje.TiempoRespuestaMs
            })
            .ToList();

        if (mensajesPorResumir.Count == 0)
        {
            return;
        }

        var mensajesSolicitud = new List<MensajeDto>
        {
            new()
            {
                Rol = "system",
                Contenido = string.Join(
                    Environment.NewLine,
                    [
                        "Resume el contexto de una conversación.",
                        "Conserva decisiones, datos importantes, solicitudes",
                        "del usuario y acciones pendientes.",
                        $"Genera el resumen en {asistente.Idioma}.",
                        "No inventes información.",
                        $"Usa como máximo {configuracionMemoria.LongitudResumen} caracteres."
                    ]),
                FechaHora = DateTime.UtcNow
            }
        };

        if (!string.IsNullOrWhiteSpace(conversacion.ResumenContexto))
        {
            mensajesSolicitud.Add(new MensajeDto
            {
                Rol = "system",
                Contenido = string.Join(
                    Environment.NewLine,
                    [
                        "Resumen acumulado anterior:",
                        conversacion.ResumenContexto
                    ]),
                FechaHora = DateTime.UtcNow
            });
        }

        mensajesSolicitud.AddRange(mensajesPorResumir);

        var solicitudResumen = new ChatRequestDto
        {
            ModeloIA = asistente.ModeloIA,
            Temperatura = 0.1m,
            MaxTokens = Math.Min(
                asistente.MaxTokens,
                configuracionMemoria.LongitudResumen),
            TimeoutSeconds = asistente.TimeoutSeconds,
            Mensajes = mensajesSolicitud
        };

        var respuestaIA = await _aiProvider.SendAsync(
            solicitudResumen,
            cancellationToken);

        conversacion.ActualizarResumenContexto(
            respuestaIA.Contenido,
            conversacion.TotalMensajesResumidos +
            mensajesPorResumir.Count);

        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);
    }
}