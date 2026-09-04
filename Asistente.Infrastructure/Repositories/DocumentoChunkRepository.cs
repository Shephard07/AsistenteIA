using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class DocumentoChunkRepository : IDocumentoChunkRepository
{
    private readonly AsistenteIADbContext _context;

    public DocumentoChunkRepository(AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<DocumentoChunk>>
        ListarPorDocumentoYVersionAsync(
            int idDocumento,
            int idVersionDocumento,
            CancellationToken cancellationToken = default)
    {
        return await _context.DocumentosChunks
            .AsNoTracking()
            .Where(chunk =>
                chunk.IdDocumento == idDocumento &&
                chunk.IdVersionDocumento == idVersionDocumento)
            .OrderBy(chunk => chunk.Orden)
            .ThenBy(chunk => chunk.NumeroChunk)
            .ToListAsync(cancellationToken);
    }
}