using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IConfiguracionMemoriaRepository
{
    Task<ConfiguracionMemoria?> ObtenerActivaAsync(
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}