using System.Security.Claims;
using Asistente.Application.DTOs;
using ApplicationAuthenticationService =
    Asistente.Application.Interfaces.IAuthenticationService;
using Asistente.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

/// <summary>
/// Gestiona el inicio y cierre de sesión de los usuarios.
/// </summary>
[ApiController]
[Route("api/autenticacion")]
[Produces("application/json")]
public class AutenticacionController : ControllerBase
{
    private readonly ApplicationAuthenticationService _authenticationService;

    public AutenticacionController(
        ApplicationAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    /// <summary>
    /// Valida las credenciales y crea una cookie de autenticación.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("iniciar-sesion")]
    [ProducesResponseType(
        typeof(LoginResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> IniciarSesion(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var respuesta = await _authenticationService
            .IniciarSesionAsync(
                request,
                ObtenerContextoCliente(),
                cancellationToken);

        var claims = new List<System.Security.Claims.Claim>
{
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier,
                respuesta.IdUsuario.ToString()),

            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Name,
                respuesta.Usuario),

            new System.Security.Claims.Claim(
                "IdSesion",
                respuesta.IdSesion.ToString())
        };

        claims.AddRange(respuesta.Roles.Select(
            rol => new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Role,
                rol)));

        var identidad = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));

        return Ok(respuesta);
    }

    /// <summary>
    /// Cierra la sesión autenticada, registra la actividad y elimina la cookie.
    /// </summary>
    [Authorize]
    [HttpPost("cerrar-sesion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ErrorResponse),
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CerrarSesion(
        CancellationToken cancellationToken)
    {
        var idUsuarioTexto = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(idUsuarioTexto, out var idUsuario))
        {
            return Unauthorized();
        }

        await _authenticationService.CerrarSesionAsync(
            idUsuario,
            ObtenerContextoCliente(),
            cancellationToken);

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return NoContent();
    }

    private ContextoClienteDto ObtenerContextoCliente()
    {
        var direccionIp = HttpContext.Connection.RemoteIpAddress?
            .ToString() ?? "No disponible";

        var navegador = Request.Headers.UserAgent.ToString();

        return new ContextoClienteDto
        {
            DireccionIP = direccionIp,
            Navegador = string.IsNullOrWhiteSpace(navegador)
                ? "No disponible"
                : navegador
        };
    }
}