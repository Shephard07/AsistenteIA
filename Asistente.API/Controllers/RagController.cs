using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

/// <summary>
/// Permite a los administradores consultar el estado de RAG.
/// </summary>
[ApiController]
[Route("api/rag")]
[Produces("application/json")]
[Authorize(Roles = "Administrador")]
public class RagController : ControllerBase
{
    private readonly IAdministracionRagService
        _administracionRagService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador RAG.
    /// </summary>
    public RagController(
        IAdministracionRagService administracionRagService)
    {
        _administracionRagService = administracionRagService;
    }

    /// <summary>
    /// Obtiene la configuración y el estado de los documentos RAG.
    /// </summary>
    [HttpGet("estado")]
    [ProducesResponseType(
        typeof(EstadoRagDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<EstadoRagDto>> ObtenerEstado(
        CancellationToken cancellationToken)
    {
        var estado = await _administracionRagService
            .ObtenerEstadoAsync(cancellationToken);

        return Ok(estado);
    }
}