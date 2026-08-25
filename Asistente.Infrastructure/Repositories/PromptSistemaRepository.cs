using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class PromptSistemaRepository : IPromptSistemaRepository
{
    private readonly AsistenteIADbContext _dbContext;

    public PromptSistemaRepository(AsistenteIADbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AgregarAsync(
        PromptSistema prompt,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.PromptsSistema.AddAsync(
            prompt,
            cancellationToken);
    }

    public async Task<PromptSistema?> ObtenerPorIdAsync(
        int idPrompt,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromptsSistema
            .Include(prompt => prompt.Historiales)
            .FirstOrDefaultAsync(
                prompt => prompt.IdPrompt == idPrompt,
                cancellationToken);
    }

    public async Task<PromptSistema?> ObtenerActivoPorAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromptsSistema
            .FirstOrDefaultAsync(
                prompt =>
                    prompt.IdAsistente == idAsistente &&
                    prompt.Activo,
                cancellationToken);
    }

    public async Task<int> ObtenerUltimaVersionAsync(
        int idAsistente,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromptsSistema
            .Where(prompt => prompt.IdAsistente == idAsistente)
            .Select(prompt => (int?)prompt.Version)
            .MaxAsync(cancellationToken)
            ?? 0;
    }

    public async Task<IReadOnlyCollection<PromptSistema>> ListarPorAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromptsSistema
            .Where(prompt => prompt.IdAsistente == idAsistente)
            .OrderByDescending(prompt => prompt.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}