using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using FluentValidation;
using AsistenteEntity = Asistente.Domain.Entities.Asistente;

namespace Asistente.Application.Services;

/// <summary>
/// Gestiona prompts inmutables y su historial de versiones.
/// </summary>
public class PromptSistemaService : IPromptSistemaService
{
    private readonly IAsistenteRepository _asistenteRepository;
    private readonly IPromptSistemaRepository _promptRepository;
    private readonly IHistorialPromptRepository _historialRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IValidator<CrearPromptSistemaRequestDto> _crearValidator;
    private readonly IValidator<CrearVersionPromptRequestDto> _versionValidator;

    public PromptSistemaService(
        IAsistenteRepository asistenteRepository,
        IPromptSistemaRepository promptRepository,
        IHistorialPromptRepository historialRepository,
        IAuditoriaRepository auditoriaRepository,
        IValidator<CrearPromptSistemaRequestDto> crearValidator,
        IValidator<CrearVersionPromptRequestDto> versionValidator)
    {
        _asistenteRepository = asistenteRepository;
        _promptRepository = promptRepository;
        _historialRepository = historialRepository;
        _auditoriaRepository = auditoriaRepository;
        _crearValidator = crearValidator;
        _versionValidator = versionValidator;
    }

    public async Task<IReadOnlyCollection<PromptSistemaDto>> ListarPorAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken = default)
    {
        var prompts = await _promptRepository.ListarPorAsistenteAsync(
            idAsistente,
            cancellationToken);

        return prompts
            .Select(MapearPrompt)
            .ToList();
    }

    public async Task<PromptSistemaDto> ObtenerActivoPorAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken = default)
    {
        var prompt = await _promptRepository.ObtenerActivoPorAsistenteAsync(
            idAsistente,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El asistente no tiene un prompt activo configurado.");

        return MapearPrompt(prompt);
    }

    public async Task<PromptSistemaDto> CrearAsync(
        int idAsistente,
        CrearPromptSistemaRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        await _crearValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        await ObtenerAsistenteAsync(idAsistente, cancellationToken);

        await DesactivarPromptActivoAsync(
            idAsistente,
            cancellationToken);

        var ultimaVersion = await _promptRepository.ObtenerUltimaVersionAsync(
            idAsistente,
            cancellationToken);

        var prompt = new PromptSistema(
            idAsistente,
            request.Nombre,
            request.Contenido,
            ultimaVersion + 1,
            true,
            idUsuarioActor.ToString());

        await _promptRepository.AgregarAsync(prompt, cancellationToken);

        // Se guarda primero para obtener el IdPrompt generado por SQL Server.
        await _promptRepository.GuardarCambiosAsync(cancellationToken);

        await RegistrarHistorialAsync(
            prompt,
            idUsuarioActor,
            "Creación inicial del prompt.",
            cancellationToken);

        await RegistrarActividadAsync(
            idUsuarioActor,
            "CrearPrompt",
            $"Se creó la versión {prompt.Version} del prompt '{prompt.Nombre}'.",
            contextoCliente,
            cancellationToken);

        await _historialRepository.GuardarCambiosAsync(cancellationToken);
        await _auditoriaRepository.GuardarCambiosAsync(cancellationToken);

        return MapearPrompt(prompt);
    }

    public async Task<PromptSistemaDto> CrearNuevaVersionAsync(
        int idPromptOrigen,
        CrearVersionPromptRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        await _versionValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var promptOrigen = await ObtenerPromptAsync(
            idPromptOrigen,
            cancellationToken);

        await DesactivarPromptActivoAsync(
            promptOrigen.IdAsistente,
            cancellationToken);

        var ultimaVersion = await _promptRepository.ObtenerUltimaVersionAsync(
            promptOrigen.IdAsistente,
            cancellationToken);

        var nuevaVersion = new PromptSistema(
            promptOrigen.IdAsistente,
            request.Nombre,
            request.Contenido,
            ultimaVersion + 1,
            true,
            idUsuarioActor.ToString());

        await _promptRepository.AgregarAsync(
            nuevaVersion,
            cancellationToken);

        // Nunca se modifica promptOrigen: se persiste una fila nueva.
        await _promptRepository.GuardarCambiosAsync(cancellationToken);

        await RegistrarHistorialAsync(
            nuevaVersion,
            idUsuarioActor,
            request.MotivoCambio,
            cancellationToken);

        await RegistrarActividadAsync(
            idUsuarioActor,
            "CrearVersionPrompt",
            $"Se creó la versión {nuevaVersion.Version} a partir del prompt '{promptOrigen.Nombre}'.",
            contextoCliente,
            cancellationToken);

        await _historialRepository.GuardarCambiosAsync(cancellationToken);
        await _auditoriaRepository.GuardarCambiosAsync(cancellationToken);

        return MapearPrompt(nuevaVersion);
    }

    public async Task CambiarEstadoAsync(
        int idPrompt,
        bool activo,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        var prompt = await ObtenerPromptAsync(idPrompt, cancellationToken);

        if (activo)
        {
            var promptActivo = await _promptRepository
                .ObtenerActivoPorAsistenteAsync(
                    prompt.IdAsistente,
                    cancellationToken);

            if (promptActivo is not null &&
                promptActivo.IdPrompt != prompt.IdPrompt)
            {
                promptActivo.CambiarEstado(false);

                // Evita infringir el índice único de un prompt activo por asistente.
                await _promptRepository.GuardarCambiosAsync(
                    cancellationToken);
            }
        }

        prompt.CambiarEstado(activo);

        await _promptRepository.GuardarCambiosAsync(cancellationToken);

        var accion = activo
            ? "ActivarPrompt"
            : "DesactivarPrompt";

        var descripcion = activo
            ? $"Se activó la versión {prompt.Version} del prompt '{prompt.Nombre}'."
            : $"Se desactivó la versión {prompt.Version} del prompt '{prompt.Nombre}'.";

        await RegistrarActividadAsync(
            idUsuarioActor,
            accion,
            descripcion,
            contextoCliente,
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<HistorialPromptDto>>
        ListarHistorialPorAsistenteAsync(
            int idAsistente,
            CancellationToken cancellationToken = default)
    {
        var historiales = await _historialRepository
            .ListarPorAsistenteAsync(idAsistente, cancellationToken);

        return historiales
            .Select(historial => new HistorialPromptDto
            {
                IdHistorial = historial.IdHistorial,
                IdPrompt = historial.IdPrompt,
                Version = historial.Version,
                Contenido = historial.Contenido,
                FechaModificacion = historial.FechaModificacion,
                UsuarioModificacion = historial.UsuarioModificacion,
                MotivoCambio = historial.MotivoCambio
            })
            .ToList();
    }

    private async Task<AsistenteEntity> ObtenerAsistenteAsync(
        int idAsistente,
        CancellationToken cancellationToken)
    {
        return await _asistenteRepository.ObtenerPorIdAsync(
            idAsistente,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El asistente solicitado no existe.");
    }

    private async Task<PromptSistema> ObtenerPromptAsync(
        int idPrompt,
        CancellationToken cancellationToken)
    {
        return await _promptRepository.ObtenerPorIdAsync(
            idPrompt,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El prompt solicitado no existe.");
    }

    private async Task DesactivarPromptActivoAsync(
        int idAsistente,
        CancellationToken cancellationToken)
    {
        var promptActivo = await _promptRepository
            .ObtenerActivoPorAsistenteAsync(
                idAsistente,
                cancellationToken);

        if (promptActivo is null)
        {
            return;
        }

        promptActivo.CambiarEstado(false);

        // Se guarda antes de activar la nueva versión por el índice único filtrado.
        await _promptRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private async Task RegistrarHistorialAsync(
        PromptSistema prompt,
        int idUsuarioActor,
        string motivoCambio,
        CancellationToken cancellationToken)
    {
        var historial = new HistorialPrompt(
            prompt.IdPrompt,
            prompt.Version,
            prompt.Contenido,
            idUsuarioActor.ToString(),
            motivoCambio);

        await _historialRepository.AgregarAsync(
            historial,
            cancellationToken);
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
            "Prompts",
            accion,
            descripcion,
            contextoCliente.DireccionIP);

        await _auditoriaRepository.AgregarActividadAsync(
            actividad,
            cancellationToken);
    }

    private static PromptSistemaDto MapearPrompt(PromptSistema prompt)
    {
        return new PromptSistemaDto
        {
            IdPrompt = prompt.IdPrompt,
            IdAsistente = prompt.IdAsistente,
            Nombre = prompt.Nombre,
            Contenido = prompt.Contenido,
            Version = prompt.Version,
            Activo = prompt.Activo,
            FechaCreacion = prompt.FechaCreacion,
            UsuarioCreacion = prompt.UsuarioCreacion
        };
    }
}