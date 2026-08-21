using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Asistente.Application.Validators;
using FluentValidation;

namespace Asistente.Application.Services;

/// <summary>
/// Valida credenciales y registra la auditoría del inicio de sesión.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IPasswordService _passwordService;
    private readonly IValidator<LoginRequestDto> _loginRequestValidator;

    public AuthenticationService(
        IUsuarioRepository usuarioRepository,
        IAuditoriaRepository auditoriaRepository,
        IPasswordService passwordService,
        IValidator<LoginRequestDto> loginRequestValidator)
    {
        _usuarioRepository = usuarioRepository;
        _auditoriaRepository = auditoriaRepository;
        _passwordService = passwordService;
        _loginRequestValidator = loginRequestValidator;
    }

    public async Task<LoginResponseDto> IniciarSesionAsync(
    LoginRequestDto request,
    ContextoClienteDto contextoCliente,
    CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        await _loginRequestValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var usuario = await _usuarioRepository
                .ObtenerPorNombreUsuarioConRolesAsync(
                request.Usuario,
                cancellationToken);

        if (usuario is null)
        {
            await RegistrarIntentoFallidoAsync(
                null,
                request.Usuario,
                contextoCliente,
                cancellationToken);

            throw new UnauthorizedAccessException(
                "Usuario o contraseña incorrectos.");

        }


        if (!usuario.Activo)
        {
            await RegistrarIntentoFallidoAsync(
                usuario.IdUsuario,
                request.Usuario,
                contextoCliente,
                cancellationToken);

            throw new UnauthorizedAccessException(
                "Usuario o contraseña incorrectos.");
        }

        if (!_passwordService.Verificar(
                usuario.PasswordHash,
                request.Password))
        {
            await RegistrarIntentoFallidoAsync(
                usuario.IdUsuario,
                request.Usuario,
                contextoCliente,
                cancellationToken);

            throw new UnauthorizedAccessException(
                "Usuario o contraseña incorrectos.");
        }

        usuario.RegistrarUltimoAcceso();

        var sesion = new AuditoriaSesion(
            usuario.IdUsuario,
            contextoCliente.DireccionIP,
            contextoCliente.Navegador);

        await _auditoriaRepository.AgregarSesionAsync(
            sesion,
            cancellationToken);

        var actividad = new AuditoriaActividad(
            usuario.IdUsuario,
            "Seguridad",
            "InicioSesion",
            "El usuario inició sesión correctamente.",
            contextoCliente.DireccionIP);

        await _auditoriaRepository.AgregarActividadAsync(
            actividad,
            cancellationToken);

        await _usuarioRepository.GuardarCambiosAsync(
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        return new LoginResponseDto
        {
            IdUsuario = usuario.IdUsuario,
            IdSesion = sesion.IdSesion,
            Usuario = usuario.NombreUsuario,
            NombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}",
            Roles = usuario.UsuarioRoles
                .Where(usuarioRol => usuarioRol.Rol.Activo)
                .Select(usuarioRol => usuarioRol.Rol.Nombre)
                .ToList()
        };
    }

    private async Task RegistrarIntentoFallidoAsync(
        int? idUsuario,
        string nombreUsuario,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken)
    {
        var actividad = new AuditoriaActividad(
            idUsuario,
            "Seguridad",
            "InicioSesionFallido",
            $"Intento de inicio de sesión fallido para el usuario '{nombreUsuario}'.",
            contextoCliente.DireccionIP);

        await _auditoriaRepository.AgregarActividadAsync(
            actividad,
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    public async Task CerrarSesionAsync(
    int idUsuario,
    ContextoClienteDto contextoCliente,
    CancellationToken cancellationToken = default)
    {
        var sesion = await _auditoriaRepository
            .ObtenerSesionActivaPorUsuarioAsync(
                idUsuario,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "No se encontró una sesión activa para el usuario.");

        sesion.Cerrar();

        var actividad = new AuditoriaActividad(
            idUsuario,
            "Seguridad",
            "CierreSesion",
            "El usuario cerró sesión correctamente.",
            contextoCliente.DireccionIP);

        await _auditoriaRepository.AgregarActividadAsync(
            actividad,
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);
    }
}