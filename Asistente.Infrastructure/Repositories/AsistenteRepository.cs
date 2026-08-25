using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using AsistenteEntity = Asistente.Domain.Entities.Asistente;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class AsistenteRepository : IAsistenteRepository
{
    private readonly AsistenteIADbContext _dbContext;

    public AsistenteRepository(AsistenteIADbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AgregarAsync(
        AsistenteEntity asistente,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Asistentes.AddAsync(
            asistente,
            cancellationToken);
    }

    public async Task<AsistenteEntity?> ObtenerPorIdAsync(
        int idAsistente,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Asistentes
            .Include(asistente => asistente.Prompts)
            .FirstOrDefaultAsync(
                asistente => asistente.IdAsistente == idAsistente,
                cancellationToken);
    }

    public async Task<AsistenteEntity?> ObtenerActivoAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Asistentes
            .Include(asistente => asistente.Prompts)
            .Where(asistente => asistente.Activo)
            .OrderBy(asistente => asistente.IdAsistente)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AsistenteEntity>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Asistentes
            .Include(asistente => asistente.Prompts)
            .OrderBy(asistente => asistente.Nombre)
            .ToListAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}