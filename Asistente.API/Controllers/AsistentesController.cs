using Asistente.API.Helpers;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

[ApiController]
[Route("api/asistentes")]
[Authorize(Roles = "Administrador")]
public class AsistentesController : ControllerBase
{
    private readonly IAsistenteService _asistenteService;

    public AsistentesController(IAsistenteService asistenteService)
    {
        _asistenteService = asistenteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<AsistenteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AsistenteDto>>> Listar(
        CancellationToken cancellationToken)
    {
        var asistentes = await _asistenteService.ListarAsync(
            cancellationToken);

        return Ok(asistentes);
    }

    [HttpGet("{idAsistente:int}")]
    [ProducesResponseType(typeof(AsistenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AsistenteDto>> ObtenerPorId(
        int idAsistente,
        CancellationToken cancellationToken)
    {
        var asistente = await _asistenteService.ObtenerPorIdAsync(
            idAsistente,
            cancellationToken);

        return Ok(asistente);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AsistenteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AsistenteDto>> Crear(
        [FromBody] CrearAsistenteRequestDto request,
        CancellationToken cancellationToken)
    {
        var respuesta = await _asistenteService.CrearAsync(
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { idAsistente = respuesta.IdAsistente },
            respuesta);
    }

    [HttpPut("{idAsistente:int}")]
    [ProducesResponseType(typeof(AsistenteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AsistenteDto>> Actualizar(
        int idAsistente,
        [FromBody] ActualizarAsistenteRequestDto request,
        CancellationToken cancellationToken)
    {
        var respuesta = await _asistenteService.ActualizarAsync(
            idAsistente,
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return Ok(respuesta);
    }

    [HttpPatch("{idAsistente:int}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        int idAsistente,
        [FromQuery] bool activo,
        CancellationToken cancellationToken)
    {
        await _asistenteService.CambiarEstadoAsync(
            idAsistente,
            activo,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }
}