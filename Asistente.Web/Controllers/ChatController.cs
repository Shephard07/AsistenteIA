using Asistente.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.Web.Controllers;

public class ChatController : Controller
{
    private readonly IConfiguration _configuration;

    public ChatController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        var model = new ChatViewModel
        {
            ApiBaseUrl = _configuration["Api:BaseUrl"] ?? string.Empty
        };

        return View(model);
    }
}