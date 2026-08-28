//ConversacionRepository.cs
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

/// <summary>
/// Implementa la persistencia de conversaciones mediante SQL Server.
/// </summary>
public class ConversacionRepository : IConversacionRepository
{
    private readonly AsistenteIADbContext _context;

    public ConversacionRepository(AsistenteIADbContext context)
    {
        _context = context;
    }

    public async Task<Conversacion?> ObtenerPorIdAsync(
        int idConversacion,
        CancellationToken cancellationToken = default)
    {
        return await _context.Conversaciones
            .Include(x => x.Mensajes)
            .FirstOrDefaultAsync(
                x => x.IdConversacion == idConversacion,
                cancellationToken);
    }

    public async Task<Conversacion?> ObtenerPorIdYUsuarioAsync(
        int idConversacion,
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        return await _context.Conversaciones
            .Include(x => x.Mensajes)
            .FirstOrDefaultAsync(
                x => x.IdConversacion == idConversacion &&
                     x.IdUsuario == idUsuario &&
                     x.Estado != EstadoConversacion.Eliminada,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Conversacion>>
        ListarPorUsuarioAsync(
            int idUsuario,
            string? terminoBusqueda,
            bool incluirArchivadas,
            int cantidadMaxima,
            CancellationToken cancellationToken = default)
    {
        var consulta = _context.Conversaciones
            .AsNoTracking()
            .Where(conversacion =>
                conversacion.IdUsuario == idUsuario &&
                conversacion.Estado != EstadoConversacion.Eliminada);

        if (!incluirArchivadas)
        {
            consulta = consulta.Where(conversacion =>
                conversacion.Estado != EstadoConversacion.Archivada);
        }

        if (!string.IsNullOrWhiteSpace(terminoBusqueda))
        {
            var termino = terminoBusqueda.Trim();

            var coincideTituloPredeterminado =
                "Nueva conversación".Contains(
                    termino,
                    StringComparison.OrdinalIgnoreCase);

            consulta = consulta.Where(conversacion =>
                (conversacion.Titulo != null &&
                 conversacion.Titulo.Contains(termino)) ||
                (conversacion.Titulo == null &&
                 coincideTituloPredeterminado));
        }

        return await consulta
            .OrderByDescending(conversacion =>
                conversacion.FechaUltimaActividad)
            .Take(cantidadMaxima)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        Conversacion conversacion,
        CancellationToken cancellationToken = default)
    {
        await _context.Conversaciones.AddAsync(
            conversacion,
            cancellationToken);
    }

    public async Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}