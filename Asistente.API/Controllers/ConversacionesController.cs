using Asistente.API.Helpers;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

/// <summary>
/// Gestiona los endpoints relacionados con conversaciones del asistente.
/// </summary>
[ApiController]
[Route("api/conversaciones")]
[Produces("application/json")]
[Authorize(Roles = "Administrador,Operador")]
public class ConversacionesController : ControllerBase
{
    private readonly IEnviarMensajeService _enviarMensajeService;
    private readonly IConversacionGestionService _conversacionGestionService;

    public ConversacionesController(
        IEnviarMensajeService enviarMensajeService,
        IConversacionGestionService conversacionGestionService)
    {
        _enviarMensajeService = enviarMensajeService;
        _conversacionGestionService = conversacionGestionService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<ConversacionHistorialDto>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<IReadOnlyCollection<ConversacionHistorialDto>>> Listar(
        [FromQuery] string? terminoBusqueda,
        [FromQuery] bool incluirArchivadas = false,
        [FromQuery] int cantidadMaxima = 20,
        CancellationToken cancellationToken = default)
    {
        var conversaciones = await _conversacionGestionService.ListarAsync(
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            terminoBusqueda,
            incluirArchivadas,
            cantidadMaxima,
            cancellationToken);

        return Ok(conversaciones);
    }

    [HttpPatch("{idConversacion:int}/titulo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Renombrar(
        int idConversacion,
        [FromBody] RenombrarConversacionRequestDto request,
        CancellationToken cancellationToken)
    {
        await _conversacionGestionService.RenombrarAsync(
            idConversacion,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            request.Titulo,
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{idConversacion:int}/archivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Archivar(
        int idConversacion,
        CancellationToken cancellationToken)
    {
        await _conversacionGestionService.ArchivarAsync(
            idConversacion,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{idConversacion:int}/reactivar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reactivar(
        int idConversacion,
        CancellationToken cancellationToken)
    {
        await _conversacionGestionService.ReactivarAsync(
            idConversacion,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{idConversacion:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Eliminar(
        int idConversacion,
        CancellationToken cancellationToken)
    {
        await _conversacionGestionService.EliminarAsync(
            idConversacion,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("mensajes")]
    [Consumes("application/json")]
    [ProducesResponseType(
        typeof(EnviarMensajeResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnviarMensajeResponseDto>> EnviarMensaje(
        [FromBody] EnviarMensajeRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _enviarMensajeService.EjecutarAsync(
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            cancellationToken);

        return Ok(response);
    }
}