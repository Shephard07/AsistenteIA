namespace Asistente.Application.DTOs;

public class CrearAsistenteRequestDto : IConfiguracionAsistenteRequestDto
{
    public string Nombre { get; init; } = string.Empty;

    public string Descripcion { get; init; } = string.Empty;

    public string ModeloIA { get; init; } = string.Empty;

    public string Idioma { get; init; } = string.Empty;

    public string LongitudRespuesta { get; init; } = string.Empty;

    public string Formalidad { get; init; } = string.Empty;

    public string FormatoRespuesta { get; init; } = string.Empty;

    public string Restricciones { get; init; } = string.Empty;

    public string MensajeBienvenida { get; init; } = string.Empty;

    public decimal Temperatura { get; init; }

    public int MaxTokens { get; init; }

    public int TimeoutSeconds { get; init; }
}