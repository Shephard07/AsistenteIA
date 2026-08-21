using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public class ContextoClienteDto
{
    public string DireccionIP { get; init; } = string.Empty;

    public string Navegador { get; init; } = string.Empty;
}