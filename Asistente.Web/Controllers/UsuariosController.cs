using Asistente.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly IConfiguration _configuration;

    public UsuariosController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View(new AdministracionViewModel
        {
            ApiBaseUrl = _configuration["Api:BaseUrl"] ?? string.Empty
        });
    }
}