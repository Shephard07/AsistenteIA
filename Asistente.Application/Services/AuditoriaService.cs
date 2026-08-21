using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _auditoriaRepository;

    public AuditoriaService(IAuditoriaRepository auditoriaRepository)
    {
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<IReadOnlyCollection<AuditoriaSesionDto>> ListarSesionesAsync(
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        var sesiones = await _auditoriaRepository.ListarSesionesAsync(
            cancellationToken);

        await RegistrarConsultaAsync(
            idUsuarioActor,
            "ConsultarSesiones",
            "Se consultó el historial de sesiones.",
            contextoCliente,
            cancellationToken);

        return sesiones.Select(MapearSesion).ToList();
    }

    public async Task<IReadOnlyCollection<AuditoriaActividadDto>>
        ListarActividadesAsync(
            int idUsuarioActor,
            ContextoClienteDto contextoCliente,
            CancellationToken cancellationToken = default)
    {
        var actividades = await _auditoriaRepository.ListarActividadesAsync(
            cancellationToken);

        await RegistrarConsultaAsync(
            idUsuarioActor,
            "ConsultarActividades",
            "Se consultó el historial de actividades.",
            contextoCliente,
            cancellationToken);

        return actividades.Select(MapearActividad).ToList();
    }

    private async Task RegistrarConsultaAsync(
        int idUsuarioActor,
        string accion,
        string descripcion,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken)
    {
        await _auditoriaRepository.AgregarActividadAsync(
            new AuditoriaActividad(
                idUsuarioActor,
                "Auditoria",
                accion,
                descripcion,
                contextoCliente.DireccionIP),
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private static AuditoriaSesionDto MapearSesion(
        AuditoriaSesion sesion)
    {
        return new AuditoriaSesionDto
        {
            IdSesion = sesion.IdSesion,
            IdUsuario = sesion.IdUsuario,
            Usuario = sesion.Usuario?.NombreUsuario ?? "No disponible",
            FechaInicio = MarcarComoUtc(sesion.FechaInicio),
            FechaFin = sesion.FechaFin.HasValue
                ? MarcarComoUtc(sesion.FechaFin.Value)
                : null,
            DireccionIP = sesion.DireccionIP,
            Navegador = sesion.Navegador,
            Estado = sesion.Estado.ToString()
        };
    }

    private static AuditoriaActividadDto MapearActividad(
        AuditoriaActividad actividad)
    {
        return new AuditoriaActividadDto
        {
            IdActividad = actividad.IdActividad,
            IdUsuario = actividad.IdUsuario,
            Usuario = actividad.Usuario?.NombreUsuario ?? "No disponible",
            FechaHora = MarcarComoUtc(actividad.FechaHora),
            Modulo = actividad.Modulo,
            Accion = actividad.Accion,
            Descripcion = actividad.Descripcion,
            DireccionIP = actividad.DireccionIP
        };

    }

    private static DateTime MarcarComoUtc(DateTime fecha)
    {
        return DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
    }
}