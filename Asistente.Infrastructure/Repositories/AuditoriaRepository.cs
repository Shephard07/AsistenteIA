using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly AsistenteIADbContext _context;

    public AuditoriaRepository(AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task AgregarSesionAsync(
        AuditoriaSesion sesion,
        CancellationToken cancellationToken = default)
    {
        await _context.AuditoriasSesion.AddAsync(
            sesion,
            cancellationToken);
    }

    public async Task<AuditoriaSesion?> ObtenerSesionActivaPorUsuarioAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditoriasSesion
            .Where(sesion =>
                sesion.IdUsuario == idUsuario &&
                sesion.Estado == EstadoSesion.Activa)
            .OrderByDescending(sesion => sesion.FechaInicio)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AgregarActividadAsync(
        AuditoriaActividad actividad,
        CancellationToken cancellationToken = default)
    {
        await _context.AuditoriasActividad.AddAsync(
            actividad,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuditoriaSesion>> ListarSesionesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditoriasSesion
            .Include(sesion => sesion.Usuario)
            .AsNoTracking()
            .OrderByDescending(sesion => sesion.FechaInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AuditoriaActividad>> ListarActividadesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditoriasActividad
            .Include(actividad => actividad.Usuario)
            .AsNoTracking()
            .OrderByDescending(actividad => actividad.FechaHora)
            .ToListAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}