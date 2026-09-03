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

    private readonly IConfiguracionMemoriaService
        _configuracionMemoriaService;

    private readonly IContextoConversacionalService
        _contextoConversacionalService;

    private readonly IRecuperacionContextoRagService
        _recuperacionContextoRagService;

    private readonly IGeneradorTituloConversacionService
        _generadorTituloConversacionService;

    private readonly IResumenConversacionService
        _resumenConversacionService;

    private readonly IPromptBuilder _promptBuilder;
    private readonly IAIProvider _aiProvider;
    private readonly IValidator<EnviarMensajeRequestDto> _validator;

    public EnviarMensajeService(
        IConversacionService conversacionService,
        IMensajeService mensajeService,
        IAsistenteService asistenteService,
        IPromptSistemaService promptSistemaService,
        IConfiguracionMemoriaService configuracionMemoriaService,
        IContextoConversacionalService contextoConversacionalService,
        IRecuperacionContextoRagService
            recuperacionContextoRagService,
        IGeneradorTituloConversacionService
            generadorTituloConversacionService,
        IPromptBuilder promptBuilder,
        IAIProvider aiProvider,
        IResumenConversacionService resumenConversacionService,
        IValidator<EnviarMensajeRequestDto> validator)
    {
        _conversacionService = conversacionService;
        _mensajeService = mensajeService;
        _asistenteService = asistenteService;
        _promptSistemaService = promptSistemaService;
        _configuracionMemoriaService = configuracionMemoriaService;
        _contextoConversacionalService = contextoConversacionalService;

        _recuperacionContextoRagService =
            recuperacionContextoRagService;

        _generadorTituloConversacionService =
            generadorTituloConversacionService;

        _promptBuilder = promptBuilder;
        _aiProvider = aiProvider;
        _resumenConversacionService = resumenConversacionService;
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

        var contexto = _contextoConversacionalService.Construir(
            mensajes,
            conversacion.ResumenContexto,
            configuracionMemoria);

        var contextoRag = await ObtenerContextoRagAsync(
            request.Mensaje,
            cancellationToken);

        var chatRequest = _promptBuilder.ConstruirSolicitudChat(
            asistente,
            promptActivo,
            contexto.Mensajes,
            contexto.ResumenContexto,
            contextoRag.Contenido);

        var respuestaIA = await _aiProvider.SendAsync(
            chatRequest,
            cancellationToken);

        await _mensajeService.RegistrarAsync(
            conversacion,
            RolMensaje.Asistente,
            respuestaIA.Contenido,
            respuestaIA.TiempoRespuestaMs,
            cancellationToken);

        await _generadorTituloConversacionService
            .GenerarSiEsNecesarioAsync(
                conversacion,
                asistente,
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

    private async Task<ContextoRagDto> ObtenerContextoRagAsync(
        string consulta,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _recuperacionContextoRagService
                .RecuperarAsync(
                    consulta,
                    cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return new ContextoRagDto();
        }
    }
}