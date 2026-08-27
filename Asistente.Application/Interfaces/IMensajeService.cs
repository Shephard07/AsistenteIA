//IMensajeService.cs
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;

namespace Asistente.Application.Interfaces;

public interface IMensajeService
{
    Task RegistrarAsync(
        Conversacion conversacion,
        RolMensaje rol,
        string contenido,
        int? tiempoRespuestaMs,
        CancellationToken cancellationToken = default);
}