using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using FluentValidation;

namespace Asistente.Application.Services;

/// <summary>
/// Coordina el envío, la respuesta de IA y el registro de mensajes.
/// </summary>
public class EnviarMensajeService : IEnviarMensajeService
{
    private readonly IConversacionRepository _conversacionRepository;
    private readonly IAIProvider _aiProvider;
    private readonly IValidator<EnviarMensajeRequestDto> _validator;

    public EnviarMensajeService(
    IConversacionRepository conversacionRepository,
    IAIProvider aiProvider,
    IValidator<EnviarMensajeRequestDto> validator)
    {
        _conversacionRepository = conversacionRepository;
        _aiProvider = aiProvider;
        _validator = validator;
    }

    public async Task<EnviarMensajeResponseDto> EjecutarAsync(
        EnviarMensajeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // FluentValidation se ejecuta antes de la lógica de negocio.
        await _validator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        Conversacion conversacion;

        if (request.IdConversacion.HasValue)
        {
            conversacion = await _conversacionRepository.ObtenerPorIdAsync(
                request.IdConversacion.Value,
                cancellationToken)
                ?? throw new KeyNotFoundException(
                    "La conversación solicitada no existe.");
        }
        else
        {
            conversacion = new Conversacion();

            await _conversacionRepository.AgregarAsync(
                conversacion,
                cancellationToken);
        }

        var mensajeUsuario = new Mensaje(
            RolMensaje.Usuario,
            request.Mensaje);

        conversacion.AgregarMensaje(mensajeUsuario);

        // Se guarda primero el mensaje del usuario para conservar
        // el registro incluso si el proveedor IA presenta un error.
        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);

        var chatRequest = new ChatRequestDto
        {
            Mensajes = conversacion.Mensajes.Select(mensaje => new MensajeDto
            {
                IdMensaje = mensaje.IdMensaje,
                IdConversacion = mensaje.IdConversacion,
                Rol = mensaje.Rol.ToString(),
                Contenido = mensaje.Contenido,
                FechaHora = mensaje.FechaHora,
                TiempoRespuestaMs = mensaje.TiempoRespuestaMs
            }).ToList()
        };

        var respuestaIA = await _aiProvider.SendAsync(
            chatRequest,
            cancellationToken);

        var mensajeAsistente = new Mensaje(
            RolMensaje.Asistente,
            respuestaIA.Contenido,
            respuestaIA.TiempoRespuestaMs);

        conversacion.AgregarMensaje(mensajeAsistente);

        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);

        return new EnviarMensajeResponseDto
        {
            IdConversacion = conversacion.IdConversacion,
            Respuesta = respuestaIA.Contenido,
            TiempoRespuestaMs = respuestaIA.TiempoRespuestaMs
        };
    }
}