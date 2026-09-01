namespace Asistente.Application.Interfaces;

public interface IProcesadorDocumentoService
{
    Task<int> ProcesarPendientesAsync(
        CancellationToken cancellationToken = default);
}