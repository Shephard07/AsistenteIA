using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Domain.Entities;
using Asistente.Domain.ValueObjects;

namespace Asistente.Domain.Interfaces;

public interface IAsistenteIA
{
    Task<RespuestaIA> GenerarRespuestaAsync(
        IReadOnlyCollection<Mensaje> mensajes,
        CancellationToken cancellationToken = default);
}