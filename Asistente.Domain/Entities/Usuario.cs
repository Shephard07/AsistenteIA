using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Domain.Entities;

public class Usuario
{
    public int IdUsuario { get; private set; }

    public string NombreUsuario { get; private set; } = string.Empty;

    public string Nombres { get; private set; } = string.Empty;

    public string Apellidos { get; private set; } = string.Empty;

    public string Correo { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public DateTime? FechaUltimoAcceso { get; private set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; private set; }
        = new List<UsuarioRol>();

    public ICollection<AuditoriaSesion> Sesiones { get; private set; }
        = new List<AuditoriaSesion>();

    public ICollection<AuditoriaActividad> Actividades { get; private set; }
        = new List<AuditoriaActividad>();

    private Usuario()
    {
    }

    public Usuario(
        string nombreUsuario,
        string nombres,
        string apellidos,
        string correo,
        string passwordHash)
    {
        NombreUsuario = nombreUsuario;
        Nombres = nombres;
        Apellidos = apellidos;
        Correo = correo;
        PasswordHash = passwordHash;
        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public void ActualizarDatos(
    string nombreUsuario,
    string nombres,
    string apellidos,
    string correo)
    {
        NombreUsuario = nombreUsuario;
        Nombres = nombres;
        Apellidos = apellidos;
        Correo = correo;
    }

    public void CambiarPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }

    public void RegistrarUltimoAcceso()
    {
        FechaUltimoAcceso = DateTime.UtcNow;
    }
}