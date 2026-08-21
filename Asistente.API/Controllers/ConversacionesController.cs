using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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

    public ConversacionesController(
        IEnviarMensajeService enviarMensajeService)
    {
        _enviarMensajeService = enviarMensajeService;
    }

    /// <summary>
    /// Envía un mensaje al asistente de inteligencia artificial.
    /// </summary>
    /// <param name="request">
    /// Datos del mensaje y, opcionalmente, el identificador de la conversación.
    /// </param>
    /// <param name="cancellationToken">
    /// Token para cancelar la solicitud.
    /// </param>
    /// <returns>
    /// La respuesta generada por la IA y el tiempo de respuesta.
    /// </returns>
    /// <response code="200">
    /// Mensaje procesado correctamente.
    /// </response>
    /// <response code="400">
    /// La solicitud contiene datos inválidos.
    /// </response>
    /// <response code="404">
    /// La conversación solicitada no existe.
    /// </response>
    /// <response code="503">
    /// El proveedor de IA no está disponible.
    /// </response>
    /// <response code="504">
    /// El proveedor de IA superó el tiempo máximo de respuesta.
    /// </response>
    /// <response code="500">
    /// Ocurrió un error inesperado.
    /// </response>
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
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status504GatewayTimeout)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EnviarMensajeResponseDto>> EnviarMensaje(
        [FromBody] EnviarMensajeRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _enviarMensajeService.EjecutarAsync(
            request,
            cancellationToken);

        return Ok(response);
    }
}