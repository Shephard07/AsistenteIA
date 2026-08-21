using Asistente.API.Helpers;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

/// <summary>
/// Gestiona los usuarios y sus accesos al sistema.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Administrador")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<UsuarioDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UsuarioDto>>> Listar(
        CancellationToken cancellationToken)
    {
        var usuarios = await _usuarioService.ListarAsync(
            cancellationToken);

        return Ok(usuarios);
    }

    [HttpGet("{idUsuario:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(
        int idUsuario,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.ObtenerPorIdAsync(
            idUsuario,
            cancellationToken);

        return Ok(usuario);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioDto>> Crear(
        [FromBody] CrearUsuarioRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.CrearAsync(
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, usuario);
    }

    [HttpPut("{idUsuario:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> Actualizar(
        int idUsuario,
        [FromBody] ActualizarUsuarioRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.ActualizarAsync(
            idUsuario,
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return Ok(usuario);
    }

    [HttpPut("{idUsuario:int}/roles")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UsuarioDto>> AsignarRoles(
        int idUsuario,
        [FromBody] AsignarRolesUsuarioRequestDto request,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarioService.AsignarRolesAsync(
            idUsuario,
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return Ok(usuario);
    }

    [HttpPut("{idUsuario:int}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CambiarPassword(
        int idUsuario,
        [FromBody] CambiarPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        await _usuarioService.CambiarPasswordAsync(
            idUsuario,
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{idUsuario:int}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CambiarEstado(
        int idUsuario,
        [FromQuery] bool activar,
        CancellationToken cancellationToken)
    {
        await _usuarioService.CambiarEstadoAsync(
            idUsuario,
            activar,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }
}