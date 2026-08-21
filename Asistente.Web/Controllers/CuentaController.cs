using System.Security.Claims;
using Asistente.Web.Models;
using Asistente.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.Web.Controllers;

public class CuentaController : Controller
{
    private readonly IAutenticacionApiClient _autenticacionApiClient;

    public CuentaController(
        IAutenticacionApiClient autenticacionApiClient)
    {
        _autenticacionApiClient = autenticacionApiClient;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult IniciarSesion(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirigirUsuarioAutenticado();
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var resultado = await _autenticacionApiClient
            .IniciarSesionAsync(
                model.Usuario,
                model.Password,
                cancellationToken);

        if (!resultado.Exito || resultado.Respuesta is null)
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.MensajeError);

            return View(model);
        }

        var respuesta = resultado.Respuesta;

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                respuesta.IdUsuario.ToString()),

            new(
                ClaimTypes.Name,
                respuesta.Usuario),

            new(
                "NombreCompleto",
                respuesta.NombreCompleto),

            new(
                "IdSesion",
                respuesta.IdSesion.ToString())
        };

        claims.AddRange(respuesta.Roles.Select(
            rol => new Claim(ClaimTypes.Role, rol)));

        var identidad = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));

        var esSoloSupervisor =
            respuesta.Roles.Contains("Supervisor") &&
            !respuesta.Roles.Contains("Administrador") &&
            !respuesta.Roles.Contains("Operador");

        if (esSoloSupervisor)
        {
            return RedirectToAction("Index", "Auditoria");
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
            Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Chat");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarSesion(
        CancellationToken cancellationToken)
    {
        await _autenticacionApiClient.CerrarSesionAsync(
            Request.Headers.Cookie.ToString(),
            cancellationToken);

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(IniciarSesion));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }

    private IActionResult RedirigirUsuarioAutenticado()
    {
        if (User.IsInRole("Administrador") ||
            User.IsInRole("Operador"))
        {
            return RedirectToAction("Index", "Chat");
        }

        if (User.IsInRole("Supervisor"))
        {
            return RedirectToAction("Index", "Auditoria");
        }

        return RedirectToAction(nameof(AccesoDenegado));
    }
}