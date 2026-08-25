using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

/// <summary>
/// Permite al administrador probar prompts sin modificar conversaciones.
/// </summary>
[ApiController]
[Route("api/pruebas-prompts")]
[Authorize(Roles = "Administrador")]
public class PruebasPromptController : ControllerBase
{
    private readonly IPruebaPromptService _pruebaPromptService;

    public PruebasPromptController(
        IPruebaPromptService pruebaPromptService)
    {
        _pruebaPromptService = pruebaPromptService;
    }

    /// <summary>
    /// Construye el prompt final y consulta Ollama sin guardar mensajes.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(
        typeof(ProbarPromptResponseDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ProbarPromptResponseDto>> Probar(
        [FromBody] ProbarPromptRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _pruebaPromptService.ProbarAsync(
            request,
            cancellationToken);

        return Ok(response);
    }
}