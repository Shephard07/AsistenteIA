//IAuditoriaRepository.cs
using Asistente.Domain.Entities;

namespace Asistente.Domain.Interfaces;

public interface IAuditoriaRepository
{
    Task AgregarSesionAsync(
        AuditoriaSesion sesion,
        CancellationToken cancellationToken = default);

    Task<AuditoriaSesion?> ObtenerSesionActivaPorUsuarioAsync(
        int idUsuario,
        CancellationToken cancellationToken = default);

    Task AgregarActividadAsync(
        AuditoriaActividad actividad,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AuditoriaSesion>> ListarSesionesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AuditoriaActividad>> ListarActividadesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AuditoriaActividad>>
    ListarActividadesPorDocumentoAsync(
        int idDocumento,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}