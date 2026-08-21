using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using FluentValidation;

namespace Asistente.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IPasswordService _passwordService;
    private readonly IValidator<CrearUsuarioRequestDto> _crearValidator;
    private readonly IValidator<ActualizarUsuarioRequestDto> _actualizarValidator;
    private readonly IValidator<AsignarRolesUsuarioRequestDto> _asignarRolesValidator;
    private readonly IValidator<CambiarPasswordRequestDto> _passwordValidator;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IAuditoriaRepository auditoriaRepository,
        IPasswordService passwordService,
        IValidator<CrearUsuarioRequestDto> crearValidator,
        IValidator<ActualizarUsuarioRequestDto> actualizarValidator,
        IValidator<AsignarRolesUsuarioRequestDto> asignarRolesValidator,
        IValidator<CambiarPasswordRequestDto> passwordValidator)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _auditoriaRepository = auditoriaRepository;
        _passwordService = passwordService;
        _crearValidator = crearValidator;
        _actualizarValidator = actualizarValidator;
        _asignarRolesValidator = asignarRolesValidator;
        _passwordValidator = passwordValidator;
    }

    public async Task<IReadOnlyCollection<UsuarioDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var usuarios = await _usuarioRepository.ListarAsync(
            cancellationToken);

        return usuarios.Select(Mapear).ToList();
    }

    public async Task<UsuarioDto> ObtenerPorIdAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObtenerConRolesPorIdAsync(
            idUsuario,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El usuario solicitado no existe.");

        return Mapear(usuario);
    }

    public async Task<UsuarioDto> CrearAsync(
        CrearUsuarioRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _crearValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var nombreUsuario = request.Usuario.Trim();
        var correo = request.Correo.Trim();

        await ValidarDatosUnicosAsync(
            nombreUsuario,
            correo,
            null,
            cancellationToken);

        var usuario = new Usuario(
            nombreUsuario,
            request.Nombres.Trim(),
            request.Apellidos.Trim(),
            correo,
            _passwordService.GenerarHash(request.Password));

        await _usuarioRepository.AgregarAsync(usuario, cancellationToken);
        await _usuarioRepository.GuardarCambiosAsync(cancellationToken);

        await ReemplazarRolesAsync(
            usuario,
            request.IdsRoles,
            cancellationToken);

        await RegistrarActividadAsync(
            idUsuarioActor,
            "CrearUsuario",
            $"Se creó el usuario '{usuario.NombreUsuario}'.",
            contextoCliente,
            cancellationToken);

        await _usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return await ObtenerPorIdAsync(
            usuario.IdUsuario,
            cancellationToken);
    }

    public async Task<UsuarioDto> ActualizarAsync(
        int idUsuario,
        ActualizarUsuarioRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _actualizarValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var usuario = await _usuarioRepository.ObtenerConRolesPorIdAsync(
            idUsuario,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El usuario solicitado no existe.");

        var nombreUsuario = request.Usuario.Trim();
        var correo = request.Correo.Trim();

        await ValidarDatosUnicosAsync(
            nombreUsuario,
            correo,
            idUsuario,
            cancellationToken);

        usuario.ActualizarDatos(
            nombreUsuario,
            request.Nombres.Trim(),
            request.Apellidos.Trim(),
            correo);

        await RegistrarActividadAsync(
            idUsuarioActor,
            "ActualizarUsuario",
            $"Se actualizó el usuario '{usuario.NombreUsuario}'.",
            contextoCliente,
            cancellationToken);

        await _usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return Mapear(usuario);
    }

    public async Task<UsuarioDto> AsignarRolesAsync(
        int idUsuario,
        AsignarRolesUsuarioRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _asignarRolesValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var usuario = await _usuarioRepository.ObtenerConRolesPorIdAsync(
            idUsuario,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El usuario solicitado no existe.");

        await ReemplazarRolesAsync(
            usuario,
            request.IdsRoles,
            cancellationToken);

        await RegistrarActividadAsync(
            idUsuarioActor,
            "AsignarRolesUsuario",
            $"Se actualizaron los roles del usuario '{usuario.NombreUsuario}'.",
            contextoCliente,
            cancellationToken);

        await _usuarioRepository.GuardarCambiosAsync(cancellationToken);

        return await ObtenerPorIdAsync(
            usuario.IdUsuario,
            cancellationToken);
    }

    public async Task CambiarPasswordAsync(
        int idUsuario,
        CambiarPasswordRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _passwordValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(
            idUsuario,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El usuario solicitado no existe.");

        usuario.CambiarPasswordHash(
            _passwordService.GenerarHash(request.NuevaPassword));

        await RegistrarActividadAsync(
            idUsuarioActor,
            "CambiarPassword",
            $"Se cambió la contraseña del usuario '{usuario.NombreUsuario}'.",
            contextoCliente,
            cancellationToken);

        await _usuarioRepository.GuardarCambiosAsync(cancellationToken);
    }

    public async Task CambiarEstadoAsync(
        int idUsuario,
        bool activar,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        if (!activar && idUsuario == idUsuarioActor)
        {
            throw new ArgumentException(
                "No puedes desactivar tu propio usuario.");
        }

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(
            idUsuario,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El usuario solicitado no existe.");

        if (activar)
        {
            usuario.Activar();
        }
        else
        {
            usuario.Desactivar();
        }

        var accion = activar ? "ActivarUsuario" : "DesactivarUsuario";
        var descripcion = activar
            ? $"Se activó el usuario '{usuario.NombreUsuario}'."
            : $"Se desactivó el usuario '{usuario.NombreUsuario}'.";

        await RegistrarActividadAsync(
            idUsuarioActor,
            accion,
            descripcion,
            contextoCliente,
            cancellationToken);

        await _usuarioRepository.GuardarCambiosAsync(cancellationToken);
    }

    private async Task ValidarDatosUnicosAsync(
        string nombreUsuario,
        string correo,
        int? idUsuarioExcluir,
        CancellationToken cancellationToken)
    {
        if (await _usuarioRepository.ExisteNombreUsuarioAsync(
                nombreUsuario,
                idUsuarioExcluir,
                cancellationToken))
        {
            throw new ArgumentException(
                "Ya existe un usuario con ese nombre.",
                nameof(nombreUsuario));
        }

        if (await _usuarioRepository.ExisteCorreoAsync(
                correo,
                idUsuarioExcluir,
                cancellationToken))
        {
            throw new ArgumentException(
                "Ya existe un usuario con ese correo.",
                nameof(correo));
        }
    }

    private async Task ReemplazarRolesAsync(
        Usuario usuario,
        IEnumerable<int> idsRoles,
        CancellationToken cancellationToken)
    {
        var idsUnicos = idsRoles.Distinct().ToList();

        var roles = new List<Rol>();

        foreach (var idRol in idsUnicos)
        {
            var rol = await _rolRepository.ObtenerPorIdAsync(
                idRol,
                cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"El rol con identificador {idRol} no existe.");

            if (!rol.Activo)
            {
                throw new ArgumentException(
                    $"El rol '{rol.Nombre}' está inactivo.");
            }

            roles.Add(rol);
        }

        usuario.UsuarioRoles.Clear();

        foreach (var rol in roles)
        {
            usuario.UsuarioRoles.Add(
                new UsuarioRol(usuario.IdUsuario, rol.IdRol));
        }
    }

    private async Task RegistrarActividadAsync(
        int idUsuarioActor,
        string accion,
        string descripcion,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken)
    {
        await _auditoriaRepository.AgregarActividadAsync(
            new AuditoriaActividad(
                idUsuarioActor,
                "Usuarios",
                accion,
                descripcion,
                contextoCliente.DireccionIP),
            cancellationToken);
    }

    private static UsuarioDto Mapear(Usuario usuario)
    {
        return new UsuarioDto
        {
            IdUsuario = usuario.IdUsuario,
            Usuario = usuario.NombreUsuario,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Correo = usuario.Correo,
            Activo = usuario.Activo,
            FechaCreacion = usuario.FechaCreacion,
            FechaUltimoAcceso = usuario.FechaUltimoAcceso,
            Roles = usuario.UsuarioRoles
                .Where(usuarioRol => usuarioRol.Rol is not null)
                .Select(usuarioRol => new RolDto
                {
                    IdRol = usuarioRol.Rol.IdRol,
                    Nombre = usuarioRol.Rol.Nombre,
                    Descripcion = usuarioRol.Rol.Descripcion,
                    Activo = usuarioRol.Rol.Activo
                })
                .ToList()
        };
    }
}