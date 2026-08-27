//MensajeService.cs
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

/// <summary>
/// Gestiona el registro de mensajes de una conversación.
/// </summary>
public class MensajeService : IMensajeService
{
    private readonly IConversacionRepository _conversacionRepository;

    public MensajeService(
        IConversacionRepository conversacionRepository)
    {
        _conversacionRepository = conversacionRepository;
    }

    public async Task RegistrarAsync(
        Conversacion conversacion,
        RolMensaje rol,
        string contenido,
        int? tiempoRespuestaMs,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversacion);

        var mensaje = new Mensaje(
            rol,
            contenido,
            tiempoRespuestaMs);

        conversacion.AgregarMensaje(mensaje);

        await _conversacionRepository.GuardarCambiosAsync(
            cancellationToken);
    }
}