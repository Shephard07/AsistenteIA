using Asistente.Domain.Entities;

namespace Asistente.Application.Interfaces;

public interface IConversacionService
{
    Task<Conversacion> ObtenerOCrearAsync(
        int? idConversacion,
        int idAsistente,
        CancellationToken cancellationToken = default);
}