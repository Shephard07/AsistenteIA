using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

/// <summary>
/// Implementa la persistencia de la configuración de memoria conversacional.
/// </summary>
public class ConfiguracionMemoriaRepository
    : IConfiguracionMemoriaRepository
{
    private readonly AsistenteIADbContext _context;

    public ConfiguracionMemoriaRepository(
        AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracionMemoria?> ObtenerActivaAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ConfiguracionesMemoria
            .FirstOrDefaultAsync(
                configuracion => configuracion.Activo,
                cancellationToken);
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}