using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Enums;
using FluentValidation;

namespace Asistente.Application.Services;

/// <summary>
/// Coordina el envío de mensajes, el proveedor de IA y el registro de la conversación.
/// </summary>
public class EnviarMensajeService : IEnviarMensajeService
{
    private readonly IConversacionService _conversacionService;
    private readonly IMensajeService _mensajeService;
    private readonly IAIProvider _aiProvider;
    private readonly IValidator<EnviarMensajeRequestDto> _validator;

    public EnviarMensajeService(
        IConversacionService conversacionService,
        IMensajeService mensajeService,
        IAIProvider aiProvider,
        IValidator<EnviarMensajeRequestDto> validator)
    {
        _conversacionService = conversacionService;
        _mensajeService = mensajeService;
        _aiProvider = aiProvider;
        _validator = validator;
    }

    public async Task<EnviarMensajeResponseDto> EjecutarAsync(
        EnviarMensajeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _validator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var conversacion = await _conversacionService.ObtenerOCrearAsync(
            request.IdConversacion,
            cancellationToken);

        await _mensajeService.RegistrarAsync(
            conversacion,
            RolMensaje.Usuario,
            request.Mensaje,
            null,
            cancellationToken);

        var chatRequest = new ChatRequestDto
        {
            Mensajes = conversacion.Mensajes.Select(mensaje => new MensajeDto
            {
                IdMensaje = mensaje.IdMensaje,
                IdConversacion = mensaje.IdConversacion,
                Rol = mensaje.Rol.ToString(),
                Contenido = mensaje.Contenido,
                FechaHora = mensaje.FechaHora,
                TiempoRespuestaMs = mensaje.TiempoRespuestaMs
            }).ToList()
        };

        var respuestaIA = await _aiProvider.SendAsync(
            chatRequest,
            cancellationToken);

        await _mensajeService.RegistrarAsync(
            conversacion,
            RolMensaje.Asistente,
            respuestaIA.Contenido,
            respuestaIA.TiempoRespuestaMs,
            cancellationToken);

        return new EnviarMensajeResponseDto
        {
            IdConversacion = conversacion.IdConversacion,
            Respuesta = respuestaIA.Contenido,
            TiempoRespuestaMs = respuestaIA.TiempoRespuestaMs
        };
    }
}