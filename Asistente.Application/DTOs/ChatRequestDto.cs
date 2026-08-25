namespace Asistente.Application.DTOs;

public sealed class ChatRequestDto
{
    public string ModeloIA { get; init; } = string.Empty;

    public decimal Temperatura { get; init; }

    public int MaxTokens { get; init; }

    public int TimeoutSeconds { get; init; }

    public IReadOnlyCollection<MensajeDto> Mensajes { get; init; }
        = Array.Empty<MensajeDto>();
}