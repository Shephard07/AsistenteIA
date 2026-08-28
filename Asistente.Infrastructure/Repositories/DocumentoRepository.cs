using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class DocumentoRepository : IDocumentoRepository
{
    private readonly AsistenteIADbContext _context;

    public DocumentoRepository(AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Documento>> ListarAsync(
        string? terminoBusqueda,
        int? idCategoria,
        EstadoDocumento? estado,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.Documentos
            .Include(documento => documento.Categoria)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(terminoBusqueda))
        {
            var termino = terminoBusqueda.Trim();

            consulta = consulta.Where(documento =>
                documento.Codigo.Contains(termino) ||
                documento.Nombre.Contains(termino));
        }

        if (idCategoria.HasValue)
        {
            consulta = consulta.Where(documento =>
                documento.IdCategoria == idCategoria.Value);
        }

        if (estado.HasValue)
        {
            consulta = consulta.Where(documento =>
                documento.Estado == estado.Value);
        }

        if (fechaDesde.HasValue)
        {
            consulta = consulta.Where(documento =>
                documento.FechaRegistro >= fechaDesde.Value.Date);
        }

        if (fechaHasta.HasValue)
        {
            var limiteSuperior = fechaHasta.Value.Date.AddDays(1);

            consulta = consulta.Where(documento =>
                documento.FechaRegistro < limiteSuperior);
        }

        return await consulta
            .OrderByDescending(documento => documento.FechaRegistro)
            .ToListAsync(cancellationToken);
    }

    public Task<Documento?> ObtenerPorIdAsync(
        int idDocumento,
        CancellationToken cancellationToken = default)
    {
        return _context.Documentos
            .Include(documento => documento.Categoria)
            .Include(documento => documento.Versiones)
            .FirstOrDefaultAsync(
                documento => documento.IdDocumento == idDocumento,
                cancellationToken);
    }

    public Task<Documento?> ObtenerPorCodigoAsync(
        string codigo,
        CancellationToken cancellationToken = default)
    {
        return _context.Documentos
            .FirstOrDefaultAsync(
                documento => documento.Codigo == codigo,
                cancellationToken);
    }

    public async Task AgregarAsync(
        Documento documento,
        CancellationToken cancellationToken = default)
    {
        await _context.Documentos.AddAsync(
            documento,
            cancellationToken);
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}