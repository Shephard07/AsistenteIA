using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IConversacionRepository
{
    Task<Conversacion?> ObtenerPorIdAsync(
        int idConversacion,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Conversacion conversacion,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}