namespace Asistente.Application.Interfaces;

public interface IIndexadorDocumentoService
{
    Task<int> IndexarPendientesAsync(
        int cantidadMaxima,
        CancellationToken cancellationToken = default);
}