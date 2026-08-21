using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Domain.Entities;

public class UsuarioRol
{
    public int IdUsuario { get; private set; }

    public int IdRol { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public Rol Rol { get; private set; } = null!;

    private UsuarioRol()
    {
    }

    public UsuarioRol(int idUsuario, int idRol)
    {
        IdUsuario = idUsuario;
        IdRol = idRol;
    }
}