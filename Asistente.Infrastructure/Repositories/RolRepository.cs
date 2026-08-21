using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class RolRepository : IRolRepository
{
    private readonly AsistenteIADbContext _context;

    public RolRepository(AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<Rol?> ObtenerPorIdAsync(
        int idRol,
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(
                rol => rol.IdRol == idRol,
                cancellationToken);
    }

    public async Task<Rol?> ObtenerPorNombreAsync(
        string nombre,
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(
                rol => rol.Nombre == nombre,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Rol>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(rol => rol.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteNombreAsync(
        string nombre,
        int? idRolExcluir = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.Roles.AnyAsync(
            rol =>
                rol.Nombre == nombre &&
                (!idRolExcluir.HasValue ||
                 rol.IdRol != idRolExcluir.Value),
            cancellationToken);
    }

    public async Task AgregarAsync(
        Rol rol,
        CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(rol, cancellationToken);
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}