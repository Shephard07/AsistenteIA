using Asistente.API.Helpers;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

/// <summary>
/// Administra los límites de memoria y contexto conversacional.
/// </summary>
[ApiController]
[Route("api/configuracion-memoria")]
[Authorize(Roles = "Administrador")]
public class ConfiguracionMemoriaController : ControllerBase
{
    private readonly IConfiguracionMemoriaService
        _configuracionMemoriaService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador.
    /// </summary>
    public ConfiguracionMemoriaController(
        IConfiguracionMemoriaService configuracionMemoriaService)
    {
        _configuracionMemoriaService =
            configuracionMemoriaService;
    }

    /// <summary>
    /// Obtiene la configuración de memoria activa.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(ConfiguracionMemoriaDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ConfiguracionMemoriaDto>>
        ObtenerActiva(CancellationToken cancellationToken)
    {
        var configuracion = await _configuracionMemoriaService
            .ObtenerActivaAsync(cancellationToken);

        return Ok(configuracion);
    }

    /// <summary>
    /// Actualiza la configuración de memoria activa.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(
        typeof(ConfiguracionMemoriaDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ConfiguracionMemoriaDto>>
        Actualizar(
            [FromBody] ActualizarConfiguracionMemoriaRequestDto request,
            CancellationToken cancellationToken)
    {
        var configuracion = await _configuracionMemoriaService
            .ActualizarAsync(
                request,
                ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
                ContextoClienteFactory.Crear(HttpContext),
                cancellationToken);

        return Ok(configuracion);
    }
}