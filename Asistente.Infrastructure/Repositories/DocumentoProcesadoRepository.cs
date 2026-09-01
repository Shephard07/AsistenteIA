using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class DocumentoProcesadoRepository
    : IDocumentoProcesadoRepository
{
    private readonly AsistenteIADbContext _context;

    public DocumentoProcesadoRepository(
        AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<DocumentoVersion>>
        ObtenerVersionesPendientesAsync(
            int cantidadMaxima,
            CancellationToken cancellationToken = default)
    {
        if (cantidadMaxima <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadMaxima),
                "La cantidad máxima debe ser mayor que cero.");
        }

        return await _context.DocumentosVersiones
            .Include(version => version.Documento)
            .Include(version => version.Procesamiento)
            .ThenInclude(procesamiento => procesamiento!.Chunks)
            .Where(version =>
                version.Activo &&
                version.Documento != null &&
                version.Documento.Estado != EstadoDocumento.Eliminado &&
                (version.Procesamiento == null ||
                 version.Procesamiento.Estado ==
                    EstadoProcesamientoDocumento.PendienteProcesamiento))
            .OrderBy(version => version.FechaCarga)
            .Take(cantidadMaxima)
            .ToListAsync(cancellationToken);
    }

    public Task<DocumentoProcesado?> ObtenerPorVersionAsync(
        int idVersionDocumento,
        CancellationToken cancellationToken = default)
    {
        return _context.DocumentosProcesados
            .Include(procesamiento => procesamiento.Chunks)
            .FirstOrDefaultAsync(
                procesamiento =>
                    procesamiento.IdVersionDocumento ==
                    idVersionDocumento,
                cancellationToken);
    }

    public async Task AgregarAsync(
        DocumentoProcesado procesamiento,
        CancellationToken cancellationToken = default)
    {
        await _context.DocumentosProcesados.AddAsync(
            procesamiento,
            cancellationToken);
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}