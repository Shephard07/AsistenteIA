using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Asistente.Infrastructure.Repositories;

/// Implementa la persistencia de conversaciones mediante SQL Server.
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