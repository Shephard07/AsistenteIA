using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using FluentValidation;

namespace Asistente.Application.Services;

public class RolService : IRolService
{
    private readonly IRolRepository _rolRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IValidator<CrearRolRequestDto> _crearRolValidator;
    private readonly IValidator<ActualizarRolRequestDto> _actualizarRolValidator;

    public RolService(
        IRolRepository rolRepository,
        IAuditoriaRepository auditoriaRepository,
        IValidator<CrearRolRequestDto> crearRolValidator,
        IValidator<ActualizarRolRequestDto> actualizarRolValidator)
    {
        _rolRepository = rolRepository;
        _auditoriaRepository = auditoriaRepository;
        _crearRolValidator = crearRolValidator;
        _actualizarRolValidator = actualizarRolValidator;
    }

    public async Task<IReadOnlyCollection<RolDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var roles = await _rolRepository.ListarAsync(cancellationToken);

        return roles.Select(Mapear).ToList();
    }

    public async Task<RolDto> CrearAsync(
        CrearRolRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _crearRolValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var nombre = request.Nombre.Trim();
        var descripcion = request.Descripcion.Trim();

        if (await _rolRepository.ExisteNombreAsync(
                nombre,
                cancellationToken: cancellationToken))
        {
            throw new ArgumentException(
                "Ya existe un rol con ese nombre.",
                nameof(request.Nombre));
        }

        var rol = new Rol(nombre, descripcion);

        await _rolRepository.AgregarAsync(rol, cancellationToken);

        await RegistrarActividadAsync(
            idUsuarioActor,
            "CrearRol",
            $"Se creó el rol '{nombre}'.",
            contextoCliente,
            cancellationToken);

        await _rolRepository.GuardarCambiosAsync(cancellationToken);

        return Mapear(rol);
    }

    public async Task<RolDto> ActualizarAsync(
        int idRol,
        ActualizarRolRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _actualizarRolValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var rol = await _rolRepository.ObtenerPorIdAsync(
            idRol,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El rol solicitado no existe.");

        var nombre = request.Nombre.Trim();
        var descripcion = request.Descripcion.Trim();

        if (await _rolRepository.ExisteNombreAsync(
                nombre,
                idRol,
                cancellationToken))
        {
            throw new ArgumentException(
                "Ya existe un rol con ese nombre.",
                nameof(request.Nombre));
        }

        rol.Actualizar(nombre, descripcion);

        await RegistrarActividadAsync(
            idUsuarioActor,
            "ActualizarRol",
            $"Se actualizó el rol '{nombre}'.",
            contextoCliente,
            cancellationToken);

        await _rolRepository.GuardarCambiosAsync(cancellationToken);

        return Mapear(rol);
    }

    public async Task CambiarEstadoAsync(
        int idRol,
        bool activar,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        var rol = await _rolRepository.ObtenerPorIdAsync(
            idRol,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El rol solicitado no existe.");

        if (activar)
        {
            rol.Activar();
        }
        else
        {
            rol.Desactivar();
        }

        var accion = activar ? "ActivarRol" : "DesactivarRol";
        var descripcion = activar
            ? $"Se activó el rol '{rol.Nombre}'."
            : $"Se desactivó el rol '{rol.Nombre}'.";

        await RegistrarActividadAsync(
            idUsuarioActor,
            accion,
            descripcion,
            contextoCliente,
            cancellationToken);

        await _rolRepository.GuardarCambiosAsync(cancellationToken);
    }

    private async Task RegistrarActividadAsync(
        int idUsuarioActor,
        string accion,
        string descripcion,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken)
    {
        var actividad = new AuditoriaActividad(
            idUsuarioActor,
            "Roles",
            accion,
            descripcion,
            contextoCliente.DireccionIP);

        await _auditoriaRepository.AgregarActividadAsync(
            actividad,
            cancellationToken);
    }

    private static RolDto Mapear(Rol rol)
    {
        return new RolDto
        {
            IdRol = rol.IdRol,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            Activo = rol.Activo
        };
    }
}