using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;

namespace Asistente.Application.Services;

/// <summary>
/// Construye el mensaje de sistema a partir de la configuración persistida
/// y prepara la solicitud que será enviada al proveedor de IA.
/// </summary>
public class PromptBuilder : IPromptBuilder
{
    public string ConstruirPromptSistema(
        AsistenteDto asistente,
        PromptSistemaDto prompt)
    {
        ArgumentNullException.ThrowIfNull(asistente);
        ArgumentNullException.ThrowIfNull(prompt);

        return string.Join(
            Environment.NewLine,
            [
                $"Asistente configurado: {asistente.Nombre}",
                $"Descripción: {asistente.Descripcion}",
                $"Idioma de respuesta: {asistente.Idioma}",
                $"Longitud esperada: {asistente.LongitudRespuesta}",
                $"Nivel de formalidad: {asistente.Formalidad}",
                $"Formato de salida: {asistente.FormatoRespuesta}",
                "Restricciones configuradas:",
                asistente.Restricciones,
                "Prompt del sistema activo:",
                prompt.Contenido
            ]);
    }

    public ChatRequestDto ConstruirSolicitudChat(
        AsistenteDto asistente,
        PromptSistemaDto prompt,
        IReadOnlyCollection<MensajeDto> mensajes)
    {
        ArgumentNullException.ThrowIfNull(mensajes);

        var mensajeSistema = new MensajeDto
        {
            Rol = "system",
            Contenido = ConstruirPromptSistema(asistente, prompt),
            FechaHora = DateTime.UtcNow
        };

        return new ChatRequestDto
        {
            ModeloIA = asistente.ModeloIA,
            Temperatura = asistente.Temperatura,
            MaxTokens = asistente.MaxTokens,
            TimeoutSeconds = asistente.TimeoutSeconds,
            Mensajes = new[] { mensajeSistema }
                .Concat(mensajes)
                .ToList()
        };
    }
}