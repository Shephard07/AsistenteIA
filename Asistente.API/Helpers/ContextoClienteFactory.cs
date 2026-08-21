using System.Security.Claims;
using Asistente.Application.DTOs;

namespace Asistente.API.Helpers;

public static class ContextoClienteFactory
{
    public static ContextoClienteDto Crear(HttpContext httpContext)
    {
        var direccionIp = httpContext.Connection.RemoteIpAddress?
            .ToString() ?? "No disponible";

        var navegador = httpContext.Request.Headers.UserAgent
            .ToString();

        return new ContextoClienteDto
        {
            DireccionIP = direccionIp,
            Navegador = string.IsNullOrWhiteSpace(navegador)
                ? "No disponible"
                : navegador
        };
    }

    public static int ObtenerIdUsuario(HttpContext httpContext)
    {
        var idUsuarioTexto = httpContext.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(idUsuarioTexto, out var idUsuario))
        {
            throw new UnauthorizedAccessException(
                "No fue posible identificar al usuario autenticado.");
        }

        return idUsuario;
    }
}