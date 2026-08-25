namespace Asistente.Application.DTOs;

public interface IConfiguracionAsistenteRequestDto
{
    string Nombre { get; }

    string Descripcion { get; }

    string ModeloIA { get; }

    string Idioma { get; }

    string LongitudRespuesta { get; }

    string Formalidad { get; }

    string FormatoRespuesta { get; }

    string Restricciones { get; }

    string MensajeBienvenida { get; }

    decimal Temperatura { get; }

    int MaxTokens { get; }

    int TimeoutSeconds { get; }
}