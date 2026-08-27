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
    private readonly IConfiguracionMemoriaService _configuracionMemoriaService;
    private readonly IResumenConversacionService _resumenConversacionService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IAIProvider _aiProvider;
    private readonly IValidator<EnviarMensajeRequestDto> _validator;

    public EnviarMensajeService(
        IConversacionService conversacionService,
        IMensajeService mensajeService,
        IAsistenteService asistenteService,
        IPromptSistemaService promptSistemaService,
        IConfiguracionMemoriaService configuracionMemoriaService,
        IResumenConversacionService resumenConversacionService,
        IPromptBuilder promptBuilder,
        IAIProvider aiProvider,
        IValidator<EnviarMensajeRequestDto> validator)
    {
        _conversacionService = conversacionService;
        _mensajeService = mensajeService;
        _asistenteService = asistenteService;
        _promptSistemaService = promptSistemaService;
        _configuracionMemoriaService = configuracionMemoriaService;
        _resumenConversacionService = resumenConversacionService;
        _promptBuilder = promptBuilder;
        _aiProvider = aiProvider;
        _validator = validator;
    }

    public async Task<EnviarMensajeResponseDto> EjecutarAsync(
        EnviarMensajeRequestDto request,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (idUsuario <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idUsuario),
                "El identificador del usuario debe ser mayor que cero.");
        }

        await _validator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var asistente = await _asistenteService.ObtenerActivoAsync(
            cancellationToken);

        var promptActivo = await _promptSistemaService
            .ObtenerActivoPorAsistenteAsync(
                asistente.IdAsistente,
                cancellationToken);

        var configuracionMemoria = await _configuracionMemoriaService
            .ObtenerActivaAsync(cancellationToken);

        var conversacion = await _conversacionService.ObtenerOCrearAsync(
            request.IdConversacion,
            asistente.IdAsistente,
            idUsuario,
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

        var mensajesContexto = SeleccionarMensajesContexto(
            mensajes,
            conversacion.ResumenContexto,
            configuracionMemoria);

        var chatRequest = _promptBuilder.ConstruirSolicitudChat(
            asistente,
            promptActivo,
            mensajesContexto,
            conversacion.ResumenContexto);

        var respuestaIA = await _aiProvider.SendAsync(
            chatRequest,
            cancellationToken);

        await _mensajeService.RegistrarAsync(
            conversacion,
            RolMensaje.Asistente,
            respuestaIA.Contenido,
            respuestaIA.TiempoRespuestaMs,
            cancellationToken);

        await _resumenConversacionService.ActualizarSiEsNecesarioAsync(
            conversacion,
            asistente,
            configuracionMemoria,
            cancellationToken);

        return new EnviarMensajeResponseDto
        {
            IdConversacion = conversacion.IdConversacion,
            Respuesta = respuestaIA.Contenido,
            TiempoRespuestaMs = respuestaIA.TiempoRespuestaMs
        };
    }

    private static IReadOnlyCollection<MensajeDto>
        SeleccionarMensajesContexto(
            IReadOnlyCollection<MensajeDto> mensajes,
            string? resumenContexto,
            ConfiguracionMemoriaDto configuracion)
    {
        var tokensResumen = string.IsNullOrWhiteSpace(resumenContexto)
            ? 0
            : EstimarTokens(resumenContexto);

        var tokensDisponibles = Math.Max(
            0,
            configuracion.MaximoTokensContexto - tokensResumen);

        var mensajesSeleccionados = new List<MensajeDto>();
        var tokensUsados = 0;

        foreach (var mensaje in mensajes
            .OrderByDescending(mensaje => mensaje.FechaHora))
        {
            if (mensajesSeleccionados.Count >=
                configuracion.MaximoMensajesContexto)
            {
                break;
            }

            var tokensMensaje = EstimarTokens(mensaje.Contenido);

            if (mensajesSeleccionados.Count > 0 &&
                tokensUsados + tokensMensaje > tokensDisponibles)
            {
                continue;
            }

            mensajesSeleccionados.Add(mensaje);
            tokensUsados += tokensMensaje;
        }

        return mensajesSeleccionados
            .OrderBy(mensaje => mensaje.FechaHora)
            .ToArray();
    }

    private static int EstimarTokens(string contenido)
    {
        return Math.Max(1, (contenido.Length + 3) / 4);
    }
}