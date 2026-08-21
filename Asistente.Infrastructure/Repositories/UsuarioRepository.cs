using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AsistenteIADbContext _context;

    public UsuarioRepository(AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(
                usuario => usuario.IdUsuario == idUsuario,
                cancellationToken);
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioAsync(
        string nombreUsuario,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(
                usuario => usuario.NombreUsuario == nombreUsuario,
                cancellationToken);
    }

    public async Task<Usuario?> ObtenerPorNombreUsuarioConRolesAsync(
        string nombreUsuario,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(usuario => usuario.UsuarioRoles)
                .ThenInclude(usuarioRol => usuarioRol.Rol)
            .FirstOrDefaultAsync(
                usuario => usuario.NombreUsuario == nombreUsuario,
                cancellationToken);
    }

    public async Task<Usuario?> ObtenerConRolesPorIdAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(usuario => usuario.UsuarioRoles)
                .ThenInclude(usuarioRol => usuarioRol.Rol)
            .FirstOrDefaultAsync(
                usuario => usuario.IdUsuario == idUsuario,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Usuario>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(usuario => usuario.UsuarioRoles)
                .ThenInclude(usuarioRol => usuarioRol.Rol)
            .AsNoTracking()
            .OrderBy(usuario => usuario.NombreUsuario)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteNombreUsuarioAsync(
        string nombreUsuario,
        int? idUsuarioExcluir = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios.AnyAsync(
            usuario =>
                usuario.NombreUsuario == nombreUsuario &&
                (!idUsuarioExcluir.HasValue ||
                 usuario.IdUsuario != idUsuarioExcluir.Value),
            cancellationToken);
    }

    public async Task<bool> ExisteCorreoAsync(
        string correo,
        int? idUsuarioExcluir = null,
        CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios.AnyAsync(
            usuario =>
                usuario.Correo == correo &&
                (!idUsuarioExcluir.HasValue ||
                 usuario.IdUsuario != idUsuarioExcluir.Value),
            cancellationToken);
    }

    public async Task AgregarAsync(
        Usuario usuario,
        CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}