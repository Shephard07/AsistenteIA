//ConfiguracionMemoriaService.cs
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using FluentValidation;

namespace Asistente.Application.Services;

/// <summary>
/// Gestiona la configuración activa de memoria conversacional.
/// </summary>
public class ConfiguracionMemoriaService
    : IConfiguracionMemoriaService
{
    private readonly IConfiguracionMemoriaRepository
        _configuracionMemoriaRepository;

    private readonly IAuditoriaRepository _auditoriaRepository;

    private readonly IValidator<ActualizarConfiguracionMemoriaRequestDto>
        _validator;

    public ConfiguracionMemoriaService(
        IConfiguracionMemoriaRepository configuracionMemoriaRepository,
        IAuditoriaRepository auditoriaRepository,
        IValidator<ActualizarConfiguracionMemoriaRequestDto> validator)
    {
        _configuracionMemoriaRepository =
            configuracionMemoriaRepository;

        _auditoriaRepository = auditoriaRepository;
        _validator = validator;
    }

    public async Task<ConfiguracionMemoriaDto> ObtenerActivaAsync(
        CancellationToken cancellationToken = default)
    {
        var configuracion = await ObtenerEntidadActivaAsync(
            cancellationToken);

        return Mapear(configuracion);
    }

    public async Task<ConfiguracionMemoriaDto> ActualizarAsync(
        ActualizarConfiguracionMemoriaRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(contextoCliente);

        await _validator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var configuracion = await ObtenerEntidadActivaAsync(
            cancellationToken);

        configuracion.Actualizar(
            request.MaximoMensajesContexto,
            request.MaximoTokensContexto,
            request.LongitudResumen,
            request.CantidadConversacionesVisibles);

        await _configuracionMemoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        await _auditoriaRepository.AgregarActividadAsync(
            new AuditoriaActividad(
                idUsuarioActor,
                "MemoriaConversacional",
                "ActualizarConfiguracionMemoria",
                "Se actualizó la configuración de memoria conversacional.",
                contextoCliente.DireccionIP),
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);

        return Mapear(configuracion);
    }

    private async Task<ConfiguracionMemoria>
        ObtenerEntidadActivaAsync(
            CancellationToken cancellationToken)
    {
        return await _configuracionMemoriaRepository
            .ObtenerActivaAsync(cancellationToken)
            ?? throw new KeyNotFoundException(
                "No existe una configuración de memoria activa.");
    }

    private static ConfiguracionMemoriaDto Mapear(
        ConfiguracionMemoria configuracion)
    {
        return new ConfiguracionMemoriaDto
        {
            IdConfiguracion = configuracion.IdConfiguracion,
            MaximoMensajesContexto =
                configuracion.MaximoMensajesContexto,
            MaximoTokensContexto =
                configuracion.MaximoTokensContexto,
            LongitudResumen = configuracion.LongitudResumen,
            CantidadConversacionesVisibles =
                configuracion.CantidadConversacionesVisibles,
            Activo = configuracion.Activo
        };
    }
}