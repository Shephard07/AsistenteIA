using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asistente.Shared.Models;

namespace Asistente.Application.Interfaces;

public interface IEnviarMensajeService
{
    Task<EnviarMensajeResponse> EjecutarAsync(
        EnviarMensajeRequest request,
        CancellationToken cancellationToken = default);
}