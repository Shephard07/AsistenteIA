using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Shared.Models;

public sealed class ErrorResponse
{
    public string Mensaje { get; init; } = string.Empty;

    public IReadOnlyCollection<string> Errores { get; init; }
        = Array.Empty<string>();
}