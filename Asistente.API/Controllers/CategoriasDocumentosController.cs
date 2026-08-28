using Asistente.API.Helpers;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

[ApiController]
[Route("api/categorias-documentos")]
[Produces("application/json")]
[Authorize(Roles = "Administrador")]
public class CategoriasDocumentosController : ControllerBase
{
    private readonly ICategoriaDocumentoService _categoriaService;

    public CategoriasDocumentosController(
        ICategoriaDocumentoService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CategoriaDocumentoDto>>>
        Listar(
            [FromQuery] bool soloActivas = true,
            CancellationToken cancellationToken = default)
    {
        var categorias = await _categoriaService.ListarAsync(
            soloActivas,
            cancellationToken);

        return Ok(categorias);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDocumentoDto>> Crear(
        [FromBody] CrearCategoriaDocumentoRequestDto request,
        CancellationToken cancellationToken)
    {
        var categoria = await _categoriaService.CrearAsync(
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return Ok(categoria);
    }

    [HttpPut("{idCategoria:int}")]
    public async Task<IActionResult> Actualizar(
        int idCategoria,
        [FromBody] ActualizarCategoriaDocumentoRequestDto request,
        CancellationToken cancellationToken)
    {
        await _categoriaService.ActualizarAsync(
            idCategoria,
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{idCategoria:int}/estado")]
    public async Task<IActionResult> CambiarEstado(
        int idCategoria,
        [FromQuery] bool activo,
        CancellationToken cancellationToken)
    {
        await _categoriaService.CambiarEstadoAsync(
            idCategoria,
            activo,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }
}