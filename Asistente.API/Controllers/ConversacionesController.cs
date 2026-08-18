using Microsoft.AspNetCore.Mvc;
using Asistente.Application.Interfaces;
using Asistente.Shared.Models;

namespace Asistente.API.Controllers;

/// Expone el endpoint REST para enviar mensajes al asistente.

[ApiController]
[Route("api/conversaciones")]
public class ConversacionesController : ControllerBase
{
    private readonly IEnviarMensajeService _enviarMensajeService;

    public ConversacionesController(
        IEnviarMensajeService enviarMensajeService)
    {
        _enviarMensajeService = enviarMensajeService;
    }

    [HttpPost("mensajes")]
    public async Task<ActionResult<EnviarMensajeResponse>> EnviarMensaje(
        [FromBody] EnviarMensajeRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _enviarMensajeService.EjecutarAsync(
            request,
            cancellationToken);

        return Ok(response);
    }
}