using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public class LoginResponseDto
{
    public int IdUsuario { get; init; }

    public int IdSesion { get; init; }

    public string Usuario { get; init; } = string.Empty;

    public string NombreCompleto { get; init; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; init; }
        = Array.Empty<string>();
}