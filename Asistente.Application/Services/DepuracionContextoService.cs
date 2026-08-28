using System.Diagnostics;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

/// <summary>
/// Construye una vista técnica del contexto que se enviaría al modelo.
/// </summary>
public class DepuracionContextoService : IDepuracionContextoService
{
    private readonly IConversacionRepository _conversacionRepository;
    private readonly IAsistenteService _asistenteService;
    private readonly IPromptSistemaService _promptSistemaService;
    private readonly IConfiguracionMemoriaService
        _configuracionMemoriaService;

    private readonly IContextoConversacionalService
        _contextoConversacionalService;

    private readonly IPromptBuilder _promptBuilder;

    public DepuracionContextoService(
        IConversacionRepository conversacionRepository,
        IAsistenteService asistenteService,
        IPromptSistemaService promptSistemaService,
        IConfiguracionMemoriaService configuracionMemoriaService,
        IContextoConversacionalService contextoConversacionalService,
        IPromptBuilder promptBuilder)
    {
        _conversacionRepository = conversacionRepository;
        _asistenteService = asistenteService;
        _promptSistemaService = promptSistemaService;
        _configuracionMemoriaService = configuracionMemoriaService;
        _contextoConversacionalService = contextoConversacionalService;
        _promptBuilder = promptBuilder;
    }

    public async Task<ContextoDepuracionDto> ObtenerAsync(
        int idConversacion,
        CancellationToken cancellationToken = default)
    {
        if (idConversacion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idConversacion),
                "El identificador de la conversación debe ser mayor que cero.");
        }

        var cronometro = Stopwatch.StartNew();

        var conversacion = await _conversacionRepository
            .ObtenerPorIdAsync(
                idConversacion,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "La conversación solicitada no existe.");

        var idAsistente = conversacion.IdAsistente
            ?? throw new KeyNotFoundException(
                "La conversación no tiene un asistente asociado.");

        var asistente = await _asistenteService.ObtenerPorIdAsync(
            idAsistente,
            cancellationToken);

        var promptActivo = await _promptSistemaService
            .ObtenerActivoPorAsistenteAsync(
                idAsistente,
                cancellationToken);

        var configuracionMemoria = await _configuracionMemoriaService
            .ObtenerActivaAsync(cancellationToken);

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
            .ToArray();

        var contexto = _contextoConversacionalService.Construir(
            mensajes,
            conversacion.ResumenContexto,
            configuracionMemoria);

        var solicitudChat = _promptBuilder.ConstruirSolicitudChat(
            asistente,
            promptActivo,
            contexto.Mensajes,
            contexto.ResumenContexto);

        var promptFinal = string.Join(
            $"{Environment.NewLine}{Environment.NewLine}",
            solicitudChat.Mensajes.Select(mensaje =>
                $"[{mensaje.Rol.ToUpperInvariant()}]{Environment.NewLine}" +
                mensaje.Contenido));

        cronometro.Stop();

        return new ContextoDepuracionDto
        {
            IdConversacion = conversacion.IdConversacion,
            TituloConversacion =
                conversacion.Titulo ?? "Nueva conversación",
            ModeloIA = asistente.ModeloIA,
            PromptFinal = promptFinal,
            MensajesContexto = contexto.Mensajes,
            ResumenContexto = contexto.ResumenContexto,
            CantidadMensajesContexto = contexto.Mensajes.Count,
            CantidadMensajesEnviados = solicitudChat.Mensajes.Count,
            TokensEstimados = solicitudChat.Mensajes.Sum(
                mensaje => _contextoConversacionalService
                    .EstimarTokens(mensaje.Contenido)),
            TiempoConstruccionMs = cronometro.ElapsedMilliseconds
        };
    }
}