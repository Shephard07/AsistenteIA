using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class DocumentoIndexadoRepository
    : IDocumentoIndexadoRepository
{
    private readonly AsistenteIADbContext _context;

    public DocumentoIndexadoRepository(
        AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<DocumentoProcesado>>
        ObtenerProcesamientosPendientesAsync(
            int cantidadMaxima,
            CancellationToken cancellationToken = default)
    {
        if (cantidadMaxima <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadMaxima),
                "La cantidad máxima debe ser mayor que cero.");
        }

        return await _context.DocumentosProcesados
            .Include(procesamiento => procesamiento.VersionDocumento)
            .ThenInclude(version => version!.Documento)
            .Include(procesamiento => procesamiento.Chunks)
            .Include(procesamiento => procesamiento.Indexacion)
            .Where(procesamiento =>
                procesamiento.Estado ==
                    EstadoProcesamientoDocumento.Procesado &&
                procesamiento.VersionDocumento != null &&
                procesamiento.VersionDocumento.Activo &&
                procesamiento.VersionDocumento.Documento != null &&
                procesamiento.VersionDocumento.Documento.Estado !=
                    EstadoDocumento.Eliminado &&
                (procesamiento.Indexacion == null ||
                 procesamiento.Indexacion.Estado ==
                    EstadoIndexacionDocumento.Pendiente))
            .OrderBy(procesamiento =>
                procesamiento.IdDocumentoProcesado)
            .Take(cantidadMaxima)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<DocumentoProcesado>>
    ListarProcesamientosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.DocumentosProcesados
            .AsNoTracking()
            .Include(procesamiento => procesamiento.VersionDocumento)
            .ThenInclude(version => version!.Documento)
            .Include(procesamiento => procesamiento.Indexacion)
            .Where(procesamiento =>
                procesamiento.Estado ==
                    EstadoProcesamientoDocumento.Procesado &&
                procesamiento.VersionDocumento != null &&
                procesamiento.VersionDocumento.Activo &&
                procesamiento.VersionDocumento.Documento != null &&
                procesamiento.VersionDocumento.Documento.Estado !=
                    EstadoDocumento.Eliminado)
            .OrderByDescending(
                procesamiento => procesamiento.IdDocumentoProcesado)
            .ToListAsync(cancellationToken);
    }

    public Task<DocumentoProcesado?>
    ObtenerProcesamientoActivoPorDocumentoAsync(
        int idDocumento,
        CancellationToken cancellationToken = default)
    {
        return _context.DocumentosProcesados
            .Include(procesamiento => procesamiento.VersionDocumento)
            .ThenInclude(version => version!.Documento)
            .Include(procesamiento => procesamiento.Indexacion)
            .FirstOrDefaultAsync(procesamiento =>
                procesamiento.Estado ==
                    EstadoProcesamientoDocumento.Procesado &&
                procesamiento.VersionDocumento != null &&
                procesamiento.VersionDocumento.IdDocumento ==
                    idDocumento &&
                procesamiento.VersionDocumento.Activo &&
                procesamiento.VersionDocumento.Documento != null &&
                procesamiento.VersionDocumento.Documento.Estado !=
                    EstadoDocumento.Eliminado,
                cancellationToken);
    }

    public Task<DocumentoIndexado?> ObtenerPorProcesamientoAsync(
        int idDocumentoProcesado,
        CancellationToken cancellationToken = default)
    {
        return _context.DocumentosIndexados
            .Include(indexacion => indexacion.DocumentoProcesado)
            .ThenInclude(procesamiento => procesamiento!.Chunks)
            .FirstOrDefaultAsync(
                indexacion => indexacion.IdDocumentoProcesado ==
                    idDocumentoProcesado,
                cancellationToken);
    }

    public async Task AgregarAsync(
        DocumentoIndexado documentoIndexado,
        CancellationToken cancellationToken = default)
    {
        await _context.DocumentosIndexados.AddAsync(
            documentoIndexado,
            cancellationToken);
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}