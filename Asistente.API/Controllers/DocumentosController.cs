using Asistente.API.Helpers;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asistente.API.Controllers;

[ApiController]
[Route("api/documentos")]
[Produces("application/json")]
[Authorize]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoService _documentoService;

    public DocumentosController(IDocumentoService documentoService)
    {
        _documentoService = documentoService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<ActionResult<IReadOnlyCollection<DocumentoResumenDto>>>
        Listar(
            [FromQuery] string? terminoBusqueda,
            [FromQuery] int? idCategoria,
            [FromQuery] EstadoDocumento? estado,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            CancellationToken cancellationToken = default)
    {
        var documentos = await _documentoService.ListarAsync(
            terminoBusqueda,
            idCategoria,
            estado,
            fechaDesde,
            fechaHasta,
            cancellationToken);

        return Ok(documentos);
    }

    [HttpGet("{idDocumento:int}")]
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<ActionResult<DocumentoDetalleDto>> ObtenerDetalle(
        int idDocumento,
        CancellationToken cancellationToken)
    {
        var documento = await _documentoService.ObtenerDetalleAsync(
            idDocumento,
            cancellationToken);

        return Ok(documento);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<DocumentoDetalleDto>> Crear(
        [FromForm] CrearDocumentoRequestDto request,
        IFormFile archivo,
        CancellationToken cancellationToken)
    {
        await using var contenido = archivo.OpenReadStream();

        var documento = await _documentoService.CrearAsync(
            request,
            CrearArchivoCarga(archivo, contenido),
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ObtenerUsuarioActor(),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerDetalle),
            new { idDocumento = documento.IdDocumento },
            documento);
    }

    [HttpPut("{idDocumento:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Actualizar(
        int idDocumento,
        [FromBody] ActualizarDocumentoRequestDto request,
        CancellationToken cancellationToken)
    {
        await _documentoService.ActualizarAsync(
            idDocumento,
            request,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{idDocumento:int}/versiones")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> AgregarVersion(
        int idDocumento,
        IFormFile archivo,
        CancellationToken cancellationToken)
    {
        await using var contenido = archivo.OpenReadStream();

        await _documentoService.AgregarVersionAsync(
            idDocumento,
            CrearArchivoCarga(archivo, contenido),
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ObtenerUsuarioActor(),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{idDocumento:int}/activar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Activar(
        int idDocumento,
        CancellationToken cancellationToken)
    {
        await _documentoService.ActivarAsync(
            idDocumento,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{idDocumento:int}/archivar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Archivar(
        int idDocumento,
        CancellationToken cancellationToken)
    {
        await _documentoService.ArchivarAsync(
            idDocumento,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{idDocumento:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(
        int idDocumento,
        CancellationToken cancellationToken)
    {
        await _documentoService.EliminarAsync(
            idDocumento,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{idDocumento:int}/versiones/{idVersion:int}/descarga")]
    [Authorize(Roles = "Administrador,Operador")]
    public async Task<IActionResult> Descargar(
        int idDocumento,
        int idVersion,
        CancellationToken cancellationToken)
    {
        var descarga = await _documentoService.DescargarAsync(
            idDocumento,
            idVersion,
            ContextoClienteFactory.ObtenerIdUsuario(HttpContext),
            ContextoClienteFactory.Crear(HttpContext),
            cancellationToken);

        return File(
            descarga.Contenido,
            descarga.TipoContenido,
            descarga.NombreArchivo);
    }

    [HttpGet("{idDocumento:int}/auditoria")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<ActionResult<IReadOnlyCollection<AuditoriaActividadDto>>>
        ListarAuditoria(
            int idDocumento,
            CancellationToken cancellationToken)
    {
        var actividades = await _documentoService.ListarAuditoriaAsync(
            idDocumento,
            cancellationToken);

        return Ok(actividades);
    }

    private static ArchivoDocumentoCargaDto CrearArchivoCarga(
        IFormFile archivo,
        Stream contenido)
    {
        return new ArchivoDocumentoCargaDto
        {
            NombreArchivo = archivo.FileName,
            TipoContenido = archivo.ContentType,
            TamanoArchivo = archivo.Length,
            Contenido = contenido
        };
    }

    private string ObtenerUsuarioActor()
    {
        return User.FindFirst("NombreCompleto")?.Value
            ?? User.Identity?.Name
            ?? "No disponible";
    }
}