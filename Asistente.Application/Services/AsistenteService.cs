using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using FluentValidation;
using AsistenteEntity = Asistente.Domain.Entities.Asistente;

namespace Asistente.Application.Services;

public class AsistenteService : IAsistenteService
{
    private readonly IAsistenteRepository _asistenteRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IValidator<CrearAsistenteRequestDto> _crearValidator;
    private readonly IValidator<ActualizarAsistenteRequestDto> _actualizarValidator;

    public AsistenteService(
        IAsistenteRepository asistenteRepository,
        IAuditoriaRepository auditoriaRepository,
        IValidator<CrearAsistenteRequestDto> crearValidator,
        IValidator<ActualizarAsistenteRequestDto> actualizarValidator)
    {
        _asistenteRepository = asistenteRepository;
        _auditoriaRepository = auditoriaRepository;
        _crearValidator = crearValidator;
        _actualizarValidator = actualizarValidator;
    }

    public async Task<IReadOnlyCollection<AsistenteDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var asistentes = await _asistenteRepository.ListarAsync(
            cancellationToken);

        return asistentes
            .Select(Mapear)
            .ToList();
    }

    public async Task<AsistenteDto> ObtenerPorIdAsync(
        int idAsistente,
        CancellationToken cancellationToken = default)
    {
        var asistente = await ObtenerEntidadPorIdAsync(
            idAsistente,
            cancellationToken);

        return Mapear(asistente);
    }

    public async Task<AsistenteDto> ObtenerActivoAsync(
        CancellationToken cancellationToken = default)
    {
        var asistente = await _asistenteRepository.ObtenerActivoAsync(
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "No existe un asistente activo configurado.");

        return Mapear(asistente);
    }

    public async Task<AsistenteDto> CrearAsync(
        CrearAsistenteRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        await _crearValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        // El último asistente creado queda seleccionado como activo.
        // Por ello se desactivan los asistentes activos previos.
        var asistentesActuales = await _asistenteRepository.ListarAsync(
            cancellationToken);

        foreach (var asistenteActual in asistentesActuales
                     .Where(asistente => asistente.Activo))
        {
            asistenteActual.CambiarEstado(false);
        }

        var asistente = new AsistenteEntity(
            request.Nombre,
            request.Descripcion,
            request.ModeloIA,
            request.Idioma,
            request.LongitudRespuesta,
            request.Formalidad,
            request.FormatoRespuesta,
            request.Restricciones,
            request.MensajeBienvenida,
            request.Temperatura,
            request.MaxTokens,
            request.TimeoutSeconds);

        await _asistenteRepository.AgregarAsync(
            asistente,
            cancellationToken);

        await RegistrarActividadAsync(
            idUsuarioActor,
            "CrearAsistente",
            $"Se creó y activó el asistente '{asistente.Nombre}'.",
            contextoCliente,
            cancellationToken);

        await _asistenteRepository.GuardarCambiosAsync(
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        return Mapear(asistente);
    }

    public async Task<AsistenteDto> ActualizarAsync(
        int idAsistente,
        ActualizarAsistenteRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        await _actualizarValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var asistente = await ObtenerEntidadPorIdAsync(
            idAsistente,
            cancellationToken);

        asistente.Actualizar(
            request.Nombre,
            request.Descripcion,
            request.ModeloIA,
            request.Idioma,
            request.LongitudRespuesta,
            request.Formalidad,
            request.FormatoRespuesta,
            request.Restricciones,
            request.MensajeBienvenida,
            request.Temperatura,
            request.MaxTokens,
            request.TimeoutSeconds);

        await RegistrarActividadAsync(
            idUsuarioActor,
            "ActualizarAsistente",
            $"Se actualizó la configuración del asistente '{asistente.Nombre}'.",
            contextoCliente,
            cancellationToken);

        await _asistenteRepository.GuardarCambiosAsync(
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        return Mapear(asistente);
    }

    public async Task CambiarEstadoAsync(
        int idAsistente,
        bool activo,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        var asistente = await ObtenerEntidadPorIdAsync(
            idAsistente,
            cancellationToken);

        if (activo)
        {
            var asistentes = await _asistenteRepository.ListarAsync(
                cancellationToken);

            foreach (var asistenteActual in asistentes
                         .Where(asistenteActual =>
                             asistenteActual.Activo &&
                             asistenteActual.IdAsistente != idAsistente))
            {
                asistenteActual.CambiarEstado(false);
            }
        }

        asistente.CambiarEstado(activo);

        var accion = activo
            ? "ActivarAsistente"
            : "DesactivarAsistente";

        var descripcion = activo
            ? $"Se activó el asistente '{asistente.Nombre}'."
            : $"Se desactivó el asistente '{asistente.Nombre}'.";

        await RegistrarActividadAsync(
            idUsuarioActor,
            accion,
            descripcion,
            contextoCliente,
            cancellationToken);

        await _asistenteRepository.GuardarCambiosAsync(
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private async Task<AsistenteEntity> ObtenerEntidadPorIdAsync(
        int idAsistente,
        CancellationToken cancellationToken)
    {
        return await _asistenteRepository.ObtenerPorIdAsync(
            idAsistente,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El asistente solicitado no existe.");
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
            "ConfiguracionAsistente",
            accion,
            descripcion,
            contextoCliente.DireccionIP);

        await _auditoriaRepository.AgregarActividadAsync(
            actividad,
            cancellationToken);
    }

    private static AsistenteDto Mapear(AsistenteEntity asistente)
    {
        return new AsistenteDto
        {
            IdAsistente = asistente.IdAsistente,
            Nombre = asistente.Nombre,
            Descripcion = asistente.Descripcion,
            ModeloIA = asistente.ModeloIA,
            Idioma = asistente.Idioma,
            LongitudRespuesta = asistente.LongitudRespuesta,
            Formalidad = asistente.Formalidad,
            FormatoRespuesta = asistente.FormatoRespuesta,
            Restricciones = asistente.Restricciones,
            MensajeBienvenida = asistente.MensajeBienvenida,
            Temperatura = asistente.Temperatura,
            MaxTokens = asistente.MaxTokens,
            TimeoutSeconds = asistente.TimeoutSeconds,
            Activo = asistente.Activo,
            FechaCreacion = asistente.FechaCreacion
        };
    }
}