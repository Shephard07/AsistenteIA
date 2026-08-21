using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Domain.Entities;

public class Rol
{
    public int IdRol { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public bool Activo { get; private set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; private set; }
        = new List<UsuarioRol>();

    private Rol()
    {
    }

    public Rol(string nombre, string descripcion)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        Activo = true;
    }

    public void Actualizar(string nombre, string descripcion)
    {
        Nombre = nombre;
        Descripcion = descripcion;
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }
}