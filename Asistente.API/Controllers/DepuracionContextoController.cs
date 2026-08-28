using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

/// <summary>
/// Permite a los administradores inspeccionar el contexto enviado al modelo.
/// </summary>
[ApiController]
[Route("api/depuracion-contexto")]
[Produces("application/json")]
[Authorize(Roles = "Administrador")]
public class DepuracionContextoController : ControllerBase
{
    private readonly IDepuracionContextoService
        _depuracionContextoService;

    public DepuracionContextoController(
        IDepuracionContextoService depuracionContextoService)
    {
        _depuracionContextoService = depuracionContextoService;
    }

    [HttpGet("conversaciones/{idConversacion:int}")]
    [ProducesResponseType(
        typeof(ContextoDepuracionDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContextoDepuracionDto>> Obtener(
        int idConversacion,
        CancellationToken cancellationToken)
    {
        var contexto = await _depuracionContextoService.ObtenerAsync(
            idConversacion,
            cancellationToken);

        return Ok(contexto);
    }
}