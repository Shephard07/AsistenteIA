using Asistente.API.Helpers;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

/// <summary>
/// Expone consultas de sesiones y actividades auditadas.
/// </summary>
[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = "Administrador,Supervisor")]
[Produces("application/json")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public AuditoriaController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    [HttpGet("sesiones")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<AuditoriaSesionDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AuditoriaSesionDto>>>
        ListarSesiones(
            CancellationToken cancellationToken)
    {
        var sesiones = await _auditoriaService.ListarSesionesAsync(
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return Ok(sesiones);
    }

    [HttpGet("actividades")]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<AuditoriaActividadDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AuditoriaActividadDto>>>
        ListarActividades(
            CancellationToken cancellationToken)
    {
        var actividades = await _auditoriaService.ListarActividadesAsync(
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return Ok(actividades);
    }
}