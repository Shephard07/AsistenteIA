using Asistente.Application.DTOs;

namespace Asistente.Application.Interfaces;

public interface ICategoriaDocumentoService
{
    Task<IReadOnlyCollection<CategoriaDocumentoDto>> ListarAsync(
        bool soloActivas,
        CancellationToken cancellationToken = default);

    Task<CategoriaDocumentoDto> CrearAsync(
        CrearCategoriaDocumentoRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        int idCategoria,
        ActualizarCategoriaDocumentoRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task CambiarEstadoAsync(
        int idCategoria,
        bool activo,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);
}