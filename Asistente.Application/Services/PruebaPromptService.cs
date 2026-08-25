using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using FluentValidation;

namespace Asistente.Application.Services;

/// <summary>
/// Construye y prueba una solicitud de IA sin alterar conversaciones ni prompts.
/// </summary>
public class PruebaPromptService : IPruebaPromptService
{
    private readonly IAsistenteService _asistenteService;
    private readonly IPromptSistemaService _promptSistemaService;
    private readonly IPromptBuilder _promptBuilder;
    private readonly IAIProvider _aiProvider;
    private readonly IValidator<ProbarPromptRequestDto> _validator;

    public PruebaPromptService(
        IAsistenteService asistenteService,
        IPromptSistemaService promptSistemaService,
        IPromptBuilder promptBuilder,
        IAIProvider aiProvider,
        IValidator<ProbarPromptRequestDto> validator)
    {
        _asistenteService = asistenteService;
        _promptSistemaService = promptSistemaService;
        _promptBuilder = promptBuilder;
        _aiProvider = aiProvider;
        _validator = validator;
    }

    public async Task<ProbarPromptResponseDto> ProbarAsync(
        ProbarPromptRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _validator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var asistente = await _asistenteService.ObtenerPorIdAsync(
            request.IdAsistente,
            cancellationToken);

        var prompts = await _promptSistemaService.ListarPorAsistenteAsync(
            request.IdAsistente,
            cancellationToken);

        var prompt = prompts.SingleOrDefault(item =>
            item.IdPrompt == request.IdPrompt)
            ?? throw new KeyNotFoundException(
                "El prompt solicitado no pertenece al asistente indicado.");

        var mensajes = new[]
        {
            new MensajeDto
            {
                Rol = "Usuario",
                Contenido = request.Mensaje,
                FechaHora = DateTime.UtcNow
            }
        };

        var solicitudChat = _promptBuilder.ConstruirSolicitudChat(
            asistente,
            prompt,
            mensajes);

        var respuestaIA = await _aiProvider.SendAsync(
            solicitudChat,
            cancellationToken);

        return new ProbarPromptResponseDto
        {
            PromptGenerado = _promptBuilder.ConstruirPromptSistema(
                asistente,
                prompt),
            Respuesta = respuestaIA.Contenido,
            TiempoRespuestaMs = respuestaIA.TiempoRespuestaMs
        };
    }
}