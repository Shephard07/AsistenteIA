using Asistente.Application.DTOs;
using Asistente.Domain.Enums;

namespace Asistente.Application.Interfaces;

public interface IDocumentoService
{
    Task<IReadOnlyCollection<DocumentoResumenDto>> ListarAsync(
        string? terminoBusqueda,
        int? idCategoria,
        EstadoDocumento? estado,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        CancellationToken cancellationToken = default);

    Task<DocumentoDetalleDto> ObtenerDetalleAsync(
        int idDocumento,
        CancellationToken cancellationToken = default);

    Task<DocumentoDetalleDto> CrearAsync(
        CrearDocumentoRequestDto request,
        ArchivoDocumentoCargaDto archivo,
        int idUsuarioActor,
        string usuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        int idDocumento,
        ActualizarDocumentoRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task AgregarVersionAsync(
        int idDocumento,
        ArchivoDocumentoCargaDto archivo,
        int idUsuarioActor,
        string usuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task ActivarAsync(
        int idDocumento,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task ArchivarAsync(
        int idDocumento,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(
        int idDocumento,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task<DescargaDocumentoDto> DescargarAsync(
        int idDocumento,
        int idVersion,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AuditoriaActividadDto>>
        ListarAuditoriaAsync(
            int idDocumento,
            CancellationToken cancellationToken = default);
}