using Asistente.Application.DTOs;
using Asistente.Domain.Entities;

namespace Asistente.Application.Interfaces;

public interface IGeneradorTituloConversacionService
{
    Task GenerarSiEsNecesarioAsync(
        Conversacion conversacion,
        AsistenteDto asistente,
        CancellationToken cancellationToken = default);
}