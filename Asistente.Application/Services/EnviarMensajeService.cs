using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Enums;
using FluentValidation;

namespace Asistente.Application.Services;

/// <summary>
/// Coordina el envío de mensajes, la configuración activa,
/// el proveedor de IA y el registro de la conversación.
/// </summary>
public class EnviarMensajeService : IEnviarMensajeService
{
    private readonly IConversacionService _conversacionService;
    private readonly IMensajeService _mensajeService;
    private readonly IAsistenteService _asistenteService;
    private readonly IPromptSistemaService _promptSistemaService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IAIProvider _aiProvider;
    private readonly IValidator<EnviarMensajeRequestDto> _validator;

    public EnviarMensajeService(
        IConversacionService conversacionService,
        IMensajeService mensajeService,
        IAsistenteService asistenteService,
        IPromptSistemaService promptSistemaService,
        IPromptBuilder promptBuilder,
        IAIProvider aiProvider,
        IValidator<EnviarMensajeRequestDto> validator)
    {
        _conversacionService = conversacionService;
        _mensajeService = mensajeService;
        _asistenteService = asistenteService;
        _promptSistemaService = promptSistemaService;
        _promptBuilder = promptBuilder;
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

        // La configuración se obtiene desde SQL en cada solicitud.
        // No existe un prompt de comportamiento escrito en el código.
        var asistente = await _asistenteService.ObtenerActivoAsync(
            cancellationToken);

        var promptActivo = await _promptSistemaService
            .ObtenerActivoPorAsistenteAsync(
                asistente.IdAsistente,
                cancellationToken);

        var conversacion = await _conversacionService.ObtenerOCrearAsync(
            request.IdConversacion,
            asistente.IdAsistente,
            cancellationToken);

        await _mensajeService.RegistrarAsync(
            conversacion,
            RolMensaje.Usuario,
            request.Mensaje,
            null,
            cancellationToken);

        var mensajes = conversacion.Mensajes
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

        var chatRequest = _promptBuilder.ConstruirSolicitudChat(
            asistente,
            promptActivo,
            mensajes);

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