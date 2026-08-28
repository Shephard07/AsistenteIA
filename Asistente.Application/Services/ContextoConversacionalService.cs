using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;

namespace Asistente.Application.Services;

/// <summary>
/// Selecciona los mensajes y el resumen que formarán el contexto del modelo.
/// </summary>
public class ContextoConversacionalService
    : IContextoConversacionalService
{
    public ContextoConversacionalDto Construir(
        IReadOnlyCollection<MensajeDto> mensajes,
        string? resumenContexto,
        ConfiguracionMemoriaDto configuracion)
    {
        ArgumentNullException.ThrowIfNull(mensajes);
        ArgumentNullException.ThrowIfNull(configuracion);

        var tokensResumen = string.IsNullOrWhiteSpace(resumenContexto)
            ? 0
            : EstimarTokens(resumenContexto);

        var tokensDisponibles = Math.Max(
            0,
            configuracion.MaximoTokensContexto - tokensResumen);

        var mensajesSeleccionados = new List<MensajeDto>();
        var tokensMensajes = 0;

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
                tokensMensajes + tokensMensaje > tokensDisponibles)
            {
                continue;
            }

            mensajesSeleccionados.Add(mensaje);
            tokensMensajes += tokensMensaje;
        }

        return new ContextoConversacionalDto
        {
            Mensajes = mensajesSeleccionados
                .OrderBy(mensaje => mensaje.FechaHora)
                .ToArray(),
            ResumenContexto = resumenContexto,
            TokensResumen = tokensResumen,
            TokensMensajes = tokensMensajes
        };
    }

    public int EstimarTokens(string contenido)
    {
        return Math.Max(1, (contenido.Length + 3) / 4);
    }
}