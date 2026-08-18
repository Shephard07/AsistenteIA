using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Shared.Models;

namespace Asistente.Application.Services;

/// Coordina el envío, la respuesta de IA y el registro de mensajes.
public class EnviarMensajeService : IEnviarMensajeService
{
    private readonly IConversacionRepository _conversacionRepository;
    private readonly IAsistenteIA _asistenteIA;

    public EnviarMensajeService(
        IConversacionRepository conversacionRepository,
        IAsistenteIA asistenteIA)
    {
        _conversacionRepository = conversacionRepository;
        _asistenteIA = asistenteIA;
    }

    public async Task<EnviarMensajeResponse> EjecutarAsync(
        EnviarMensajeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Mensaje))
        {
            throw new ArgumentException(
                "El mensaje no puede estar vacío.",
                nameof(request.Mensaje));
        }

        Conversacion conversacion;

        if (request.IdConversacion.HasValue)
        {
            conversacion = await _conversacionRepository.ObtenerPorIdAsync(
                request.IdConversacion.Value,
                cancellationToken)
                ?? throw new KeyNotFoundException(
                    "La conversación solicitada no existe.");
        }
        else
        {
            conversacion = new Conversacion();

            await _conversacionRepository.AgregarAsync(
                conversacion,
                cancellationToken);
        }

        var mensajeUsuario = new Mensaje(
            RolMensaje.Usuario,
            request.Mensaje);

        conversacion.AgregarMensaje(mensajeUsuario);

        // Se guarda primero el mensaje del usuario.
        // Así queda registrado incluso si Ollama presenta un error.
        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);

        var respuestaIA = await _asistenteIA.GenerarRespuestaAsync(
            conversacion.Mensajes.ToList(),
            cancellationToken);

        var mensajeAsistente = new Mensaje(
            RolMensaje.Asistente,
            respuestaIA.Contenido,
            respuestaIA.TiempoRespuestaMs);

        conversacion.AgregarMensaje(mensajeAsistente);

        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);

        return new EnviarMensajeResponse
        {
            IdConversacion = conversacion.IdConversacion,
            Respuesta = respuestaIA.Contenido,
            TiempoRespuestaMs = respuestaIA.TiempoRespuestaMs
        };
    }
}