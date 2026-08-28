using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using System.Globalization;

namespace Asistente.Application.Services;

/// <summary>
/// Genera automáticamente un título breve para una conversación nueva.
/// </summary>
public class GeneradorTituloConversacionService
    : IGeneradorTituloConversacionService
{
    private readonly IAIProvider _aiProvider;
    private readonly IConversacionRepository _conversacionRepository;

    public GeneradorTituloConversacionService(
        IAIProvider aiProvider,
        IConversacionRepository conversacionRepository)
    {
        _aiProvider = aiProvider;
        _conversacionRepository = conversacionRepository;
    }

    public async Task GenerarSiEsNecesarioAsync(
        Conversacion conversacion,
        AsistenteDto asistente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversacion);
        ArgumentNullException.ThrowIfNull(asistente);

        if (!string.IsNullOrWhiteSpace(conversacion.Titulo) ||
            conversacion.TotalMensajes < 2)
        {
            return;
        }

        var mensajeInicialUsuario = conversacion.Mensajes
            .OrderBy(mensaje => mensaje.FechaHora)
            .FirstOrDefault(mensaje => mensaje.Rol == RolMensaje.Usuario);

        if (mensajeInicialUsuario is null)
        {
            return;
        }

        var titulo = string.Empty;

        try
        {
            var solicitudTitulo = new ChatRequestDto
            {
                ModeloIA = asistente.ModeloIA,
                Temperatura = 0.1m,
                MaxTokens = Math.Min(asistente.MaxTokens, 256),
                TimeoutSeconds = asistente.TimeoutSeconds,
                Mensajes =
                [
                    new MensajeDto
                    {
                        Rol = "system",
                        Contenido = string.Join(
                            Environment.NewLine,
                            [
                                "Eres un generador de títulos para conversaciones.",
                                "Responde exclusivamente en español.",
                                "Genera un título descriptivo de entre 3 y 8 palabras.",
                                "El título debe tener como máximo 60 caracteres.",
                                "Devuelve únicamente el título, sin comillas, viñetas ni explicaciones."
                            ]),
                        FechaHora = DateTime.UtcNow
                    },
                    new MensajeDto
                    {
                        Rol = "user",
                        Contenido = mensajeInicialUsuario.Contenido,
                        FechaHora = mensajeInicialUsuario.FechaHora
                    }
                ]
            };

            var respuestaIA = await _aiProvider.SendAsync(
                solicitudTitulo,
                cancellationToken);

            titulo = NormalizarTitulo(respuestaIA.Contenido);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // Si Ollama no puede generar el título, se usa un respaldo.
        }

        if (string.IsNullOrWhiteSpace(titulo))
        {
            titulo = CrearTituloRespaldo(
                mensajeInicialUsuario.Contenido);
        }

        if (string.IsNullOrWhiteSpace(titulo))
        {
            return;
        }

        conversacion.Renombrar(titulo);

        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private static string CrearTituloRespaldo(string contenido)
    {
        var titulo = contenido
            .Trim()
            .TrimEnd('.', '?', '!');

        var iniciosAEliminar = new[]
        {
            "Necesito ",
            "Quiero ",
            "Deseo ",
            "Me gustaría "
        };

        foreach (var inicio in iniciosAEliminar)
        {
            if (titulo.StartsWith(
                inicio,
                StringComparison.OrdinalIgnoreCase))
            {
                titulo = titulo[inicio.Length..].Trim();
                break;
            }
        }

        titulo = string.Join(
            " ",
            titulo.Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries)
            .Take(8));

        return NormalizarTitulo(titulo);
    }

    private static string NormalizarTitulo(string contenido)
    {
        var titulo = contenido
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.Trim()
            ?? string.Empty;

        titulo = titulo.Trim(' ', '"', '\'', '`', '*');

        const string prefijo = "Título:";

        if (titulo.StartsWith(
            prefijo,
            StringComparison.OrdinalIgnoreCase))
        {
            titulo = titulo[prefijo.Length..].Trim();
        }

        if (titulo.Length > 60)
        {
            titulo = titulo[..60].TrimEnd();
        }

        if (string.IsNullOrWhiteSpace(titulo))
        {
            return string.Empty;
        }

        return char.ToUpper(
            titulo[0],
            CultureInfo.GetCultureInfo("es-PE"))
            + titulo[1..];
    }
}