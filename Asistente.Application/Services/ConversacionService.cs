using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

/// <summary>
/// Gestiona la obtención y creación de conversaciones.
/// </summary>
public class ConversacionService : IConversacionService
{
    private readonly IConversacionRepository _conversacionRepository;

    public ConversacionService(
        IConversacionRepository conversacionRepository)
    {
        _conversacionRepository = conversacionRepository;
    }

    public async Task<Conversacion> ObtenerOCrearAsync(
        int? idConversacion,
        int idAsistente,
        CancellationToken cancellationToken = default)
    {
        if (idConversacion.HasValue)
        {
            return await _conversacionRepository.ObtenerPorIdAsync(
                idConversacion.Value,
                cancellationToken)
                ?? throw new KeyNotFoundException(
                    "La conversación solicitada no existe.");
        }

        var conversacion = new Conversacion(idAsistente);

        await _conversacionRepository.AgregarAsync(
            conversacion,
            cancellationToken);

        return conversacion;
    }
}