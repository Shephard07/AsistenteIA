using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Asistente.Infrastructure.Persistence;

public class InicializadorSeguridad
{
    private readonly AsistenteIADbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly UsuarioInicialOptions _options;

    public InicializadorSeguridad(
        AsistenteIADbContext context,
        IPasswordService passwordService,
        IOptions<UsuarioInicialOptions> options)
    {
        _context = context;
        _passwordService = passwordService;
        _options = options.Value;
    }

    public async Task InicializarAsync(
        CancellationToken cancellationToken = default)
    {
        await CrearRolSiNoExisteAsync(
            "Administrador",
            "Acceso completo al sistema.",
            cancellationToken);

        await CrearRolSiNoExisteAsync(
            "Operador",
            "Acceso al asistente inteligente.",
            cancellationToken);

        await CrearRolSiNoExisteAsync(
            "Supervisor",
            "Acceso a consultas de auditoría.",
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        if (!_options.Habilitado ||
            string.IsNullOrWhiteSpace(_options.NombreUsuario) ||
            string.IsNullOrWhiteSpace(_options.Password))
        {
            return;
        }

        var existeAdministrador = await _context.Usuarios.AnyAsync(
            usuario => usuario.NombreUsuario == _options.NombreUsuario,
            cancellationToken);

        if (existeAdministrador)
        {
            return;
        }

        var administrador = await _context.Roles
            .FirstAsync(
                rol => rol.Nombre == "Administrador",
                cancellationToken);

        var usuario = new Usuario(
            _options.NombreUsuario,
            _options.Nombres,
            _options.Apellidos,
            _options.Correo,
            _passwordService.GenerarHash(_options.Password));

        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.UsuarioRoles.AddAsync(
            new UsuarioRol(usuario.IdUsuario, administrador.IdRol),
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task CrearRolSiNoExisteAsync(
        string nombre,
        string descripcion,
        CancellationToken cancellationToken)
    {
        var existe = await _context.Roles.AnyAsync(
            rol => rol.Nombre == nombre,
            cancellationToken);

        if (!existe)
        {
            await _context.Roles.AddAsync(
                new Rol(nombre, descripcion),
                cancellationToken);
        }
    }
}