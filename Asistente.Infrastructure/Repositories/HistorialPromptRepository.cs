using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class HistorialPromptRepository : IHistorialPromptRepository
{
    private readonly AsistenteIADbContext _dbContext;

    public HistorialPromptRepository(AsistenteIADbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AgregarAsync(
        HistorialPrompt historial,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.HistorialesPrompt.AddAsync(
            historial,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<HistorialPrompt>> ListarPorPromptAsync(
        int idPrompt,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HistorialesPrompt
            .Where(historial => historial.IdPrompt == idPrompt)
            .OrderByDescending(historial => historial.Version)
            .ThenByDescending(historial => historial.FechaModificacion)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<HistorialPrompt>> ListarPorAsistenteAsync(
    int idAsistente,
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.HistorialesPrompt
            .Include(historial => historial.PromptSistema)
            .Where(historial =>
                historial.PromptSistema.IdAsistente == idAsistente)
            .OrderByDescending(historial => historial.Version)
            .ThenByDescending(historial => historial.FechaModificacion)
            .ToListAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}