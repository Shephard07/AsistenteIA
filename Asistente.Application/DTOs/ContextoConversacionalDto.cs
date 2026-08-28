namespace Asistente.Application.DTOs;

/// <summary>
/// Representa la porción de una conversación que será enviada al modelo.
/// </summary>
public sealed class ContextoConversacionalDto
{
    public IReadOnlyCollection<MensajeDto> Mensajes { get; init; }
        = Array.Empty<MensajeDto>();

    public string? ResumenContexto { get; init; }

    public int TokensResumen { get; init; }

    public int TokensMensajes { get; init; }

    public int TokensEstimados => TokensResumen + TokensMensajes;
}