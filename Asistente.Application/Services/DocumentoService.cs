//DocumentoService.cs
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using FluentValidation;

namespace Asistente.Application.Services;

public class DocumentoService : IDocumentoService
{
    private readonly IDocumentoRepository _documentoRepository;
    private readonly ICategoriaDocumentoRepository _categoriaRepository;
    private readonly IAlmacenamientoDocumentoService _almacenamientoService;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IDocumentoChunkRepository _documentoChunkRepository;
    private readonly IValidator<CrearDocumentoRequestDto> _crearValidator;
    private readonly IValidator<ActualizarDocumentoRequestDto>
        _actualizarValidator;

    public DocumentoService(
        IDocumentoRepository documentoRepository,
        ICategoriaDocumentoRepository categoriaRepository,
        IAlmacenamientoDocumentoService almacenamientoService,
        IDocumentoChunkRepository documentoChunkRepository,
        IAuditoriaRepository auditoriaRepository,
        IValidator<CrearDocumentoRequestDto> crearValidator,
        IValidator<ActualizarDocumentoRequestDto> actualizarValidator)
    {
        _documentoRepository = documentoRepository;
        _categoriaRepository = categoriaRepository;
        _almacenamientoService = almacenamientoService;
        _documentoChunkRepository = documentoChunkRepository;
        _auditoriaRepository = auditoriaRepository;
        _crearValidator = crearValidator;
        _actualizarValidator = actualizarValidator;
    }

    public async Task<IReadOnlyCollection<DocumentoResumenDto>> ListarAsync(
        string? terminoBusqueda,
        int? idCategoria,
        EstadoDocumento? estado,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        CancellationToken cancellationToken = default)
    {
        var documentos = await _documentoRepository.ListarAsync(
            terminoBusqueda,
            idCategoria,
            estado,
            fechaDesde,
            fechaHasta,
            cancellationToken);

        return documentos
            .Select(MapearResumen)
            .ToArray();
    }

    public async Task<DocumentoDetalleDto> ObtenerDetalleAsync(
        int idDocumento,
        CancellationToken cancellationToken = default)
    {
        var documento = await ObtenerDocumentoAsync(
            idDocumento,
            cancellationToken);

        return MapearDetalle(documento);
    }

    public async Task<IReadOnlyCollection<DocumentoChunkDetalleDto>>
    ListarChunksAsync(
        int idDocumento,
        int idVersionDocumento,
        CancellationToken cancellationToken = default)
    {
        if (idVersionDocumento <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idVersionDocumento),
                "El identificador de la versión debe ser mayor que cero.");
        }

        var documento = await ObtenerDocumentoAsync(
            idDocumento,
            cancellationToken);

        var version = documento.Versiones.FirstOrDefault(item =>
            item.IdVersion == idVersionDocumento)
            ?? throw new KeyNotFoundException(
                "La versión solicitada no pertenece al documento.");

        var chunks = await _documentoChunkRepository
            .ListarPorDocumentoYVersionAsync(
                idDocumento,
                idVersionDocumento,
                cancellationToken);

        return chunks
            .Select(chunk => new DocumentoChunkDetalleDto
            {
                IdChunk = chunk.IdChunk,
                IdDocumento = documento.IdDocumento,
                CodigoDocumento = documento.Codigo,
                NombreDocumento = documento.Nombre,
                IdVersionDocumento = version.IdVersion,
                NumeroVersion = version.NumeroVersion,
                IdCategoria = chunk.IdCategoria,
                Categoria = documento.Categoria?.Nombre
                    ?? "Sin categoría",
                NumeroChunk = chunk.NumeroChunk,
                Orden = chunk.Orden,
                PaginaInicial = chunk.PaginaInicial,
                PaginaFinal = chunk.PaginaFinal,
                TotalCaracteres = chunk.TotalCaracteres,
                Texto = chunk.Texto
            })
            .ToArray();
    }

    public async Task<DocumentoDetalleDto> CrearAsync(
        CrearDocumentoRequestDto request,
        ArchivoDocumentoCargaDto archivo,
        int idUsuarioActor,
        string usuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(archivo);
        ArgumentNullException.ThrowIfNull(contextoCliente);

        ValidarIdUsuario(idUsuarioActor);

        await _crearValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var codigo = request.Codigo.Trim();

        var documentoExistente = await _documentoRepository
            .ObtenerPorCodigoAsync(codigo, cancellationToken);

        if (documentoExistente is not null)
        {
            throw new InvalidOperationException(
                "Ya existe un documento con ese código.");
        }

        await ObtenerCategoriaActivaAsync(
            request.IdCategoria,
            cancellationToken);

        ArchivoDocumentoAlmacenadoDto? archivoAlmacenado = null;
        Documento documento;

        try
        {
            archivoAlmacenado = await _almacenamientoService.GuardarAsync(
                archivo,
                cancellationToken);

            documento = new Documento(
                codigo,
                request.Nombre,
                request.Descripcion,
                request.IdCategoria,
                NormalizarUsuario(usuarioActor));

            documento.AgregarVersion(new DocumentoVersion(
                numeroVersion: 1,
                archivoAlmacenado.NombreArchivo,
                archivoAlmacenado.RutaArchivo,
                archivoAlmacenado.TamanoArchivo,
                archivoAlmacenado.HashArchivo,
                NormalizarUsuario(usuarioActor)));

            await _documentoRepository.AgregarAsync(
                documento,
                cancellationToken);

            await _documentoRepository.GuardarCambiosAsync(
                cancellationToken);
        }
        catch
        {
            await EliminarArchivoDeRecuperacionAsync(
                archivoAlmacenado?.RutaArchivo,
                cancellationToken);

            throw;
        }

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            documento.IdDocumento,
            "CrearDocumento",
            $"Se creó el documento '{documento.Codigo}' con su versión 1.",
            contextoCliente,
            cancellationToken);

        return await ObtenerDetalleAsync(
            documento.IdDocumento,
            cancellationToken);
    }

    public async Task ActualizarAsync(
        int idDocumento,
        ActualizarDocumentoRequestDto request,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(contextoCliente);

        ValidarIdUsuario(idUsuarioActor);

        await _actualizarValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var documento = await ObtenerDocumentoAsync(
            idDocumento,
            cancellationToken);

        await ObtenerCategoriaActivaAsync(
            request.IdCategoria,
            cancellationToken);

        documento.ActualizarInformacion(
            request.Nombre,
            request.Descripcion,
            request.IdCategoria);

        await _documentoRepository.GuardarCambiosAsync(
            cancellationToken);

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            documento.IdDocumento,
            "ActualizarDocumento",
            $"Se actualizó la información del documento '{documento.Codigo}'.",
            contextoCliente,
            cancellationToken);
    }

    public async Task AgregarVersionAsync(
        int idDocumento,
        ArchivoDocumentoCargaDto archivo,
        int idUsuarioActor,
        string usuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(archivo);
        ArgumentNullException.ThrowIfNull(contextoCliente);

        ValidarIdUsuario(idUsuarioActor);

        var documento = await ObtenerDocumentoAsync(
            idDocumento,
            cancellationToken);

        ValidarDocumentoDisponibleParaVersionar(documento);

        ArchivoDocumentoAlmacenadoDto? archivoAlmacenado = null;

        try
        {
            archivoAlmacenado = await _almacenamientoService.GuardarAsync(
                archivo,
                cancellationToken);

            documento.AgregarVersion(new DocumentoVersion(
                documento.VersionActual + 1,
                archivoAlmacenado.NombreArchivo,
                archivoAlmacenado.RutaArchivo,
                archivoAlmacenado.TamanoArchivo,
                archivoAlmacenado.HashArchivo,
                NormalizarUsuario(usuarioActor)));

            await _documentoRepository.GuardarCambiosAsync(
                cancellationToken);
        }
        catch
        {
            await EliminarArchivoDeRecuperacionAsync(
                archivoAlmacenado?.RutaArchivo,
                cancellationToken);

            throw;
        }

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            documento.IdDocumento,
            "AgregarVersionDocumento",
            $"Se registró la versión {documento.VersionActual} del documento '{documento.Codigo}'.",
            contextoCliente,
            cancellationToken);
    }

    public async Task ActivarAsync(
        int idDocumento,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contextoCliente);

        ValidarIdUsuario(idUsuarioActor);

        var documento = await ObtenerDocumentoAsync(
            idDocumento,
            cancellationToken);

        documento.Activar();

        await _documentoRepository.GuardarCambiosAsync(
            cancellationToken);

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            documento.IdDocumento,
            "ActivarDocumento",
            $"Se activó el documento '{documento.Codigo}'.",
            contextoCliente,
            cancellationToken);
    }

    public async Task ArchivarAsync(
        int idDocumento,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contextoCliente);

        ValidarIdUsuario(idUsuarioActor);

        var documento = await ObtenerDocumentoAsync(
            idDocumento,
            cancellationToken);

        documento.Archivar();

        await _documentoRepository.GuardarCambiosAsync(
            cancellationToken);

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            documento.IdDocumento,
            "ArchivarDocumento",
            $"Se archivó el documento '{documento.Codigo}'.",
            contextoCliente,
            cancellationToken);
    }

    public async Task EliminarAsync(
        int idDocumento,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contextoCliente);

        ValidarIdUsuario(idUsuarioActor);

        var documento = await ObtenerDocumentoAsync(
            idDocumento,
            cancellationToken);

        documento.Eliminar();

        await _documentoRepository.GuardarCambiosAsync(
            cancellationToken);

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            documento.IdDocumento,
            "EliminarDocumento",
            $"Se eliminó lógicamente el documento '{documento.Codigo}'.",
            contextoCliente,
            cancellationToken);
    }

    public async Task<DescargaDocumentoDto> DescargarAsync(
        int idDocumento,
        int idVersion,
        int idUsuarioActor,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contextoCliente);

        ValidarIdUsuario(idUsuarioActor);

        var documento = await ObtenerDocumentoAsync(
            idDocumento,
            cancellationToken);

        if (documento.Estado == EstadoDocumento.Eliminado)
        {
            throw new InvalidOperationException(
                "No se puede descargar un documento eliminado.");
        }

        var version = documento.Versiones.FirstOrDefault(
            item => item.IdVersion == idVersion)
            ?? throw new KeyNotFoundException(
                "La versión solicitada no existe.");

        var contenido = await _almacenamientoService.AbrirLecturaAsync(
            version.RutaArchivo,
            cancellationToken);

        await RegistrarAuditoriaAsync(
            idUsuarioActor,
            documento.IdDocumento,
            "DescargarDocumento",
            $"Se descargó la versión {version.NumeroVersion} del documento '{documento.Codigo}'.",
            contextoCliente,
            cancellationToken);

        return new DescargaDocumentoDto
        {
            NombreArchivo = version.NombreArchivo,
            TipoContenido = "application/pdf",
            Contenido = contenido
        };
    }

    public async Task<IReadOnlyCollection<AuditoriaActividadDto>>
        ListarAuditoriaAsync(
            int idDocumento,
            CancellationToken cancellationToken = default)
    {
        await ObtenerDocumentoAsync(idDocumento, cancellationToken);

        var actividades = await _auditoriaRepository
            .ListarActividadesPorDocumentoAsync(
                idDocumento,
                cancellationToken);

        return actividades
            .Select(actividad => new AuditoriaActividadDto
            {
                IdActividad = actividad.IdActividad,
                IdUsuario = actividad.IdUsuario,
                Usuario = actividad.Usuario?.NombreUsuario
                    ?? "No disponible",
                FechaHora = DateTime.SpecifyKind(
                    actividad.FechaHora,
                    DateTimeKind.Utc),

                Modulo = actividad.Modulo,
                Accion = actividad.Accion,
                Descripcion = actividad.Descripcion,
                DireccionIP = actividad.DireccionIP
            })
            .ToArray();
    }

    private async Task<Documento> ObtenerDocumentoAsync(
        int idDocumento,
        CancellationToken cancellationToken)
    {
        if (idDocumento <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idDocumento),
                "El identificador del documento debe ser mayor que cero.");
        }

        return await _documentoRepository.ObtenerPorIdAsync(
            idDocumento,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "El documento solicitado no existe.");
    }

    private async Task<CategoriaDocumento> ObtenerCategoriaActivaAsync(
        int idCategoria,
        CancellationToken cancellationToken)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdAsync(
            idCategoria,
            cancellationToken)
            ?? throw new KeyNotFoundException(
                "La categoría solicitada no existe.");

        if (!categoria.Activo)
        {
            throw new InvalidOperationException(
                "La categoría seleccionada está inactiva.");
        }

        return categoria;
    }

    private static void ValidarDocumentoDisponibleParaVersionar(
        Documento documento)
    {
        if (documento.Estado == EstadoDocumento.Eliminado)
        {
            throw new InvalidOperationException(
                "No se puede versionar un documento eliminado.");
        }

        if (documento.Estado == EstadoDocumento.Archivado)
        {
            throw new InvalidOperationException(
                "Reactiva el documento antes de registrar una nueva versión.");
        }
    }

    private async Task RegistrarAuditoriaAsync(
        int idUsuarioActor,
        int idDocumento,
        string accion,
        string descripcion,
        ContextoClienteDto contextoCliente,
        CancellationToken cancellationToken)
    {
        await _auditoriaRepository.AgregarActividadAsync(
            new AuditoriaActividad(
                idUsuarioActor,
                "GestorDocumental",
                accion,
                descripcion,
                contextoCliente.DireccionIP,
                idDocumento),
            cancellationToken);

        await _auditoriaRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private async Task EliminarArchivoDeRecuperacionAsync(
        string? rutaArchivo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            return;
        }

        try
        {
            await _almacenamientoService.EliminarAsync(
                rutaArchivo,
                cancellationToken);
        }
        catch
        {
            // El error original siempre debe conservarse.
        }
    }

    private static void ValidarIdUsuario(int idUsuario)
    {
        if (idUsuario <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idUsuario),
                "El identificador del usuario debe ser mayor que cero.");
        }
    }

    private static string NormalizarUsuario(string? usuario)
    {
        return string.IsNullOrWhiteSpace(usuario)
            ? "No disponible"
            : usuario.Trim();
    }

    private static DocumentoResumenDto MapearResumen(
        Documento documento)
    {
        return new DocumentoResumenDto
        {
            IdDocumento = documento.IdDocumento,
            Codigo = documento.Codigo,
            Nombre = documento.Nombre,
            Categoria = documento.Categoria?.Nombre
                ?? "Sin categoría",

            VersionActual = documento.VersionActual,
            Estado = documento.Estado.ToString(),
            EstadoProcesamiento =
                documento.EstadoProcesamiento.ToString(),

            FechaRegistro = documento.FechaRegistro,
            UsuarioRegistro = documento.UsuarioRegistro
        };
    }

    private static DocumentoDetalleDto MapearDetalle(
        Documento documento)
    {

        var versionActual = documento.Versiones.FirstOrDefault(
    version => version.Activo);

        var procesamiento = versionActual?.Procesamiento;

        return new DocumentoDetalleDto
        {
            IdDocumento = documento.IdDocumento,
            Codigo = documento.Codigo,
            Nombre = documento.Nombre,
            Descripcion = documento.Descripcion,
            IdCategoria = documento.IdCategoria,
            Categoria = documento.Categoria?.Nombre
                ?? "Sin categoría",

            VersionActual = documento.VersionActual,
            Estado = documento.Estado.ToString(),
            EstadoProcesamiento =
                documento.EstadoProcesamiento.ToString(),

            FechaRegistro = documento.FechaRegistro,
            UsuarioRegistro = documento.UsuarioRegistro,

            ProcesamientoActual = new ProcesamientoDocumentoDto
            {
                IdVersionDocumento = versionActual?.IdVersion ?? 0,
                Estado = procesamiento?.Estado.ToString()
        ?? documento.EstadoProcesamiento.ToString(),

                FechaInicio = procesamiento?.FechaInicio,
                FechaFin = procesamiento?.FechaFin,
                TotalPaginas = procesamiento?.TotalPaginas ?? 0,
                TotalCaracteres = procesamiento?.TotalCaracteres ?? 0,
                TotalChunks = procesamiento?.TotalChunks ?? 0,
                Observaciones = procesamiento?.Observaciones ?? string.Empty
            },

            Versiones = documento.Versiones
                .OrderByDescending(version => version.NumeroVersion)
                .Select(version => new DocumentoVersionDto
                {
                    IdVersion = version.IdVersion,
                    NumeroVersion = version.NumeroVersion,
                    NombreArchivo = version.NombreArchivo,
                    TamanoArchivo = version.TamanoArchivo,
                    HashArchivo = version.HashArchivo,
                    FechaCarga = version.FechaCarga,
                    UsuarioCarga = version.UsuarioCarga,
                    Activo = version.Activo
                })
                .ToArray()
        };
    }
}