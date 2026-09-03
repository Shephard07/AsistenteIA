using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

public class EmbeddingConfiguracionRepository
    : IEmbeddingConfiguracionRepository
{
    private readonly AsistenteIADbContext _context;

    public EmbeddingConfiguracionRepository(
        AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<EmbeddingConfiguracion> ObtenerActivaAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfiguracionesEmbedding
            .SingleOrDefaultAsync(
                configuracion => configuracion.Activo,
                cancellationToken)
            ?? throw new InvalidOperationException(
                "No existe una configuración de embeddings activa.");
    }
}