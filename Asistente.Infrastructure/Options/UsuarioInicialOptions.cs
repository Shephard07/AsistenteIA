using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Infrastructure.Options;

public class UsuarioInicialOptions
{
    public const string SectionName = "UsuarioInicial";

    public bool Habilitado { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}