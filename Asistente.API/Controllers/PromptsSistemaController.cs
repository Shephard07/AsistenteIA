using Asistente.API.Helpers;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
public class PromptsSistemaController : ControllerBase
{
    private readonly IPromptSistemaService _promptSistemaService;

    public PromptsSistemaController(
        IPromptSistemaService promptSistemaService)
    {
        _promptSistemaService = promptSistemaService;
    }

    [HttpGet("api/asistentes/{idAsistente:int}/prompts")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PromptSistemaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<PromptSistemaDto>>>
        ListarPorAsistente(
            int idAsistente,
            CancellationToken cancellationToken)
    {
        var prompts = await _promptSistemaService.ListarPorAsistenteAsync(
            idAsistente,
            cancellationToken);

        return Ok(prompts);
    }

    [HttpGet("api/asistentes/{idAsistente:int}/prompts/historial")]
    [ProducesResponseType(typeof(IReadOnlyCollection<HistorialPromptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<HistorialPromptDto>>>
        ListarHistorial(
            int idAsistente,
            CancellationToken cancellationToken)
    {
        var historial = await _promptSistemaService
            .ListarHistorialPorAsistenteAsync(
                idAsistente,
                cancellationToken);

        return Ok(historial);
    }

    [HttpPost("api/asistentes/{idAsistente:int}/prompts")]
    [ProducesResponseType(typeof(PromptSistemaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromptSistemaDto>> Crear(
        int idAsistente,
        [FromBody] CrearPromptSistemaRequestDto request,
        CancellationToken cancellationToken)
    {
        var respuesta = await _promptSistemaService.CrearAsync(
            idAsistente,
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return CreatedAtAction(
            nameof(ListarPorAsistente),
            new { idAsistente },
            respuesta);
    }

    [HttpPost("api/prompts/{idPrompt:int}/versiones")]
    [ProducesResponseType(typeof(PromptSistemaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromptSistemaDto>> CrearNuevaVersion(
        int idPrompt,
        [FromBody] CrearVersionPromptRequestDto request,
        CancellationToken cancellationToken)
    {
        var respuesta = await _promptSistemaService.CrearNuevaVersionAsync(
            idPrompt,
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, respuesta);
    }

    [HttpPatch("api/prompts/{idPrompt:int}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(
        int idPrompt,
        [FromQuery] bool activo,
        CancellationToken cancellationToken)
    {
        await _promptSistemaService.CambiarEstadoAsync(
            idPrompt,
            activo,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }
}