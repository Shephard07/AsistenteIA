// DocumentosController.cs
using Asistente.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.Web.Controllers;

[Authorize(Roles = "Administrador,Operador")]
public class DocumentosController : Controller
{
    private readonly IConfiguration _configuration;

    public DocumentosController(IConfiguration configuration)
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