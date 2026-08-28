namespace Asistente.Application.DTOs;

/// <summary>
/// Contiene el contexto final que se preparó para una conversación.
/// </summary>
public sealed class ContextoDepuracionDto
{
    public int IdConversacion { get; init; }

    public string TituloConversacion { get; init; } = string.Empty;

    public string ModeloIA { get; init; } = string.Empty;

    public string PromptFinal { get; init; } = string.Empty;

    public IReadOnlyCollection<MensajeDto> MensajesContexto { get; init; }
        = Array.Empty<MensajeDto>();

    public string? ResumenContexto { get; init; }

    public int CantidadMensajesContexto { get; init; }

    public int CantidadMensajesEnviados { get; init; }

    public int TokensEstimados { get; init; }

    public long TiempoConstruccionMs { get; init; }
}