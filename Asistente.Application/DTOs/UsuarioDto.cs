using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Application.DTOs;

public class UsuarioDto
{
    public int IdUsuario { get; init; }

    public string Usuario { get; init; } = string.Empty;

    public string Nombres { get; init; } = string.Empty;

    public string Apellidos { get; init; } = string.Empty;

    public string Correo { get; init; } = string.Empty;

    public bool Activo { get; init; }

    public DateTime FechaCreacion { get; init; }

    public DateTime? FechaUltimoAcceso { get; init; }

    public IReadOnlyCollection<RolDto> Roles { get; init; }
        = Array.Empty<RolDto>();
}