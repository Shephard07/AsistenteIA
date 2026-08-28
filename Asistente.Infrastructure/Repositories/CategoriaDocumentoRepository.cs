using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class CategoriaDocumentoRepository : ICategoriaDocumentoRepository
{
    private readonly AsistenteIADbContext _context;

    public CategoriaDocumentoRepository(AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CategoriaDocumento>> ListarAsync(
        bool soloActivas,
        CancellationToken cancellationToken = default)
    {
        var consulta = _context.CategoriasDocumento
            .AsNoTracking()
            .AsQueryable();

        if (soloActivas)
        {
            consulta = consulta.Where(categoria => categoria.Activo);
        }

        return await consulta
            .OrderBy(categoria => categoria.Nombre)
            .ToListAsync(cancellationToken);
    }

    public Task<CategoriaDocumento?> ObtenerPorIdAsync(
        int idCategoria,
        CancellationToken cancellationToken = default)
    {
        return _context.CategoriasDocumento
            .FirstOrDefaultAsync(
                categoria => categoria.IdCategoria == idCategoria,
                cancellationToken);
    }

    public Task<CategoriaDocumento?> ObtenerPorNombreAsync(
        string nombre,
        CancellationToken cancellationToken = default)
    {
        return _context.CategoriasDocumento
            .FirstOrDefaultAsync(
                categoria => categoria.Nombre == nombre,
                cancellationToken);
    }

    public async Task AgregarAsync(
        CategoriaDocumento categoria,
        CancellationToken cancellationToken = default)
    {
        await _context.CategoriasDocumento.AddAsync(
            categoria,
            cancellationToken);
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}