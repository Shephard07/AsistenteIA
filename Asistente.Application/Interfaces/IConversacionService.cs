using Asistente.Domain.Entities;

namespace Asistente.Application.Interfaces;

public interface IConversacionService
{
    Task<Conversacion> ObtenerOCrearAsync(
        int? idConversacion,
        CancellationToken cancellationToken = default);
}