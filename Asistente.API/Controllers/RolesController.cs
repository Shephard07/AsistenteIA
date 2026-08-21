using Asistente.API.Helpers;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

/// <summary>
/// Gestiona los roles del sistema.
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Administrador")]
[Produces("application/json")]
public class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<RolDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<RolDto>>> Listar(
        CancellationToken cancellationToken)
    {
        var roles = await _rolService.ListarAsync(cancellationToken);

        return Ok(roles);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RolDto>> Crear(
        [FromBody] CrearRolRequestDto request,
        CancellationToken cancellationToken)
    {
        var rol = await _rolService.CrearAsync(
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, rol);
    }

    [HttpPut("{idRol:int}")]
    [ProducesResponseType(typeof(RolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolDto>> Actualizar(
        int idRol,
        [FromBody] ActualizarRolRequestDto request,
        CancellationToken cancellationToken)
    {
        var rol = await _rolService.ActualizarAsync(
            idRol,
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return Ok(rol);
    }

    [HttpPatch("{idRol:int}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        int idRol,
        [FromQuery] bool activar,
        CancellationToken cancellationToken)
    {
        await _rolService.CambiarEstadoAsync(
            idRol,
            activar,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }
}