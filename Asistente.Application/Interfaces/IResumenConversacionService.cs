using Asistente.Application.DTOs;
using Asistente.Domain.Entities;

namespace Asistente.Application.Interfaces;

public interface IResumenConversacionService
{
    Task ActualizarSiEsNecesarioAsync(
        Conversacion conversacion,
        AsistenteDto asistente,
        ConfiguracionMemoriaDto configuracionMemoria,
        CancellationToken cancellationToken = default);
}