using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using FluentValidation;

namespace Asistente.Application.Services;

public class CategoriaDocumentoService : ICategoriaDocumentoService
{
    private readonly ICategoriaDocumentoRepository _categoriaRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IValidator<CrearCategoriaDocumentoRequestDto>
        _crearValidator;
    private readonly IValidator<ActualizarCategoriaDocumentoRequestDto>
        _actualizarValidator;

    public CategoriaDocumentoService(
        ICategoriaDocumentoRepository categoriaRepository,
        IAuditoriaRepository auditoriaRepository,
        IValidator<CrearCategoriaDocumentoRequestDto> crearValidator,
        IValidator<ActualizarCategoriaDocumentoRequestDto> actualizarValidator)
    {
        _categoriaRepository = categoriaRepository;
        _auditoriaRepository = auditoriaRepository;
        _crearValidator = crearValidator;
        _actualizarValidator = actualizarValidator;
    }

    public async Task<IReadOnlyCollection<CategoriaDocumentoDto>> ListarAsync(
        bool soloActivas,
        CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaRepository.ListarAsync(
            soloActivas,
            cancellationToken);

        return categorias
            .Select(MapearDto)
            .ToArray();
    }

    public async Task<CategoriaDocumentoDto> CrearAsync(
        CrearCategoriaDocumentoRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _crearValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var nombre = request.Nombre.Trim();

        var categoriaExistente = await _categoriaRepository
            .ObtenerPorNombreAsync(nombre, cancellationToken);

        if (categoriaExistente is not null)
        {
            throw new InvalidOperationException(
                "Ya existe una categoría con ese nombre.");
        }

        var categoria = new CategoriaDocumento(
            nombre,
            request.Descripcion);

        await _categoriaRepository.AgregarAsync(
            categoria,
            cancellationToken);

        await _categoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            "CrearCategoria",
            $"Se creó la categoría documental '{categoria.Nombre}'.",
            contextoCliente,
            cancellationToken);

        return MapearDto(categoria);
    }

    public async Task ActualizarAsync(
        int idCategoria,
        ActualizarCategoriaDocumentoRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _actualizarValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var categoria = await ObtenerCategoriaAsync(
            idCategoria,
            cancellationToken);

        var nombre = request.Nombre.Trim();

        var categoriaConMismoNombre = await _categoriaRepository
            .ObtenerPorNombreAsync(nombre, cancellationToken);

        if (categoriaConMismoNombre is not null &&
            categoriaConMismoNombre.IdCategoria != idCategoria)
        {
            throw new InvalidOperationException(
                "Ya existe una categoría con ese nombre.");
        }

        categoria.Actualizar(nombre, request.Descripcion);

        await _categoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            "ActualizarCategoria",
            $"Se actualizó la categoría documental '{categoria.Nombre}'.",
            contextoCliente,
            cancellationToken);
    }

    public async Task CambiarEstadoAsync(
        int idCategoria,
        bool activo,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        var categoria = await ObtenerCategoriaAsync(
            idCategoria,
            cancellationToken);

        if (activo)
        {
            categoria.Activar();
        }
        else
        {
            categoria.Desactivar();
        }

        await _categoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        var accion = activo
            ? "ActivarCategoria"
            : "DesactivarCategoria";

        var descripcion = activo
            ? $"Se activó la categoría documental '{categoria.Nombre}'."
            : $"Se desactivó la categoría documental '{categoria.Nombre}'.";

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            accion,
            descripcion,
            contextoCliente,
            cancellationToken);
    }

    private async Task<CategoriaDocumento> ObtenerCategoriaAsync(
        int idCategoria,
        CancellationToken cancellationToken)
    {
        if (idCategoria <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idCategoria),
                "El identificador de la categoría debe ser mayor que cero.");
        }

        return await _categoriaRepository.ObtenerPorIdAsync(
            idCategoria,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "La categoría solicitada no existe.");
    }

    private async Task RegistrarAuditoriaAsync(
        int idUsuarioActor,
        string accion,
        string descripcion,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken)
    {
        await _auditoriaRepository.AgregarActividadAsync(
            new AuditoriaActividad(
                idUsuarioActor,
                "GestorDocumental",
                accion,
                descripcion,
                contextoCliente.DireccionIP),
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private static CategoriaDocumentoDto MapearDto(
        CategoriaDocumento categoria)
    {
        return new CategoriaDocumentoDto
        {
            IdCategoria = categoria.IdCategoria,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Activo = categoria.Activo,
            FechaCreacion = categoria.FechaCreacion
        };
    }
}