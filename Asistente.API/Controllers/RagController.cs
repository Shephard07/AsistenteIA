using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asistente.API.Helpers;

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

    /// <summary>
    /// Solicita la reindexación de la versión activa de un documento.
    /// </summary>
    [HttpPost("documentos/{idDocumento:int}/reindexar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SolicitarReindexacion(
        int idDocumento,
        CancellationToken cancellationToken)
    {
        await _administracionRagService
            .SolicitarReindexacionAsync(
                idDocumento,
                ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
                ContextoClienteFactory.Crear(HttpContext),
                cancellationToken);

        return NoContent();
    }
}