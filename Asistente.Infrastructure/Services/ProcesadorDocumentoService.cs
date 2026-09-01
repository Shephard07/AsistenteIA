using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Asistente.Infrastructure.Services;

public class ProcesadorDocumentoService : IProcesadorDocumentoService
{
    private readonly IDocumentoProcesadoRepository
        _procesamientoRepository;

    private readonly IAlmacenamientoDocumentoService
        _almacenamientoService;

    private readonly IEnumerable<IExtractorTextoDocumento>
        _extractores;

    private readonly INormalizadorContenidoDocumento
        _normalizador;

    private readonly IChunkingDocumentoService
        _chunkingService;

    private readonly IConfiguracionProcesamientoDocumento
        _configuracionService;

    private readonly IAuditoriaRepository _auditoriaRepository;

    private readonly ILogger<ProcesadorDocumentoService> _logger;

    public ProcesadorDocumentoService(
        IDocumentoProcesadoRepository procesamientoRepository,
        IAlmacenamientoDocumentoService almacenamientoService,
        IEnumerable<IExtractorTextoDocumento> extractores,
        INormalizadorContenidoDocumento normalizador,
        IChunkingDocumentoService chunkingService,
        IConfiguracionProcesamientoDocumento configuracionService,
        IAuditoriaRepository auditoriaRepository,
        ILogger<ProcesadorDocumentoService> logger)
    {
        _procesamientoRepository = procesamientoRepository;
        _almacenamientoService = almacenamientoService;
        _extractores = extractores;
        _normalizador = normalizador;
        _chunkingService = chunkingService;
        _configuracionService = configuracionService;
        _auditoriaRepository = auditoriaRepository;
        _logger = logger;
    }

    public async Task<int> ProcesarPendientesAsync(
        CancellationToken cancellationToken = default)
    {
        var configuracion = _configuracionService.Obtener();

        var versiones = await _procesamientoRepository
            .ObtenerVersionesPendientesAsync(
                configuracion.MaximoDocumentosPorCiclo,
                cancellationToken);

        var totalProcesados = 0;

        foreach (var version in versiones)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ProcesarVersionAsync(
                    version,
                    configuracion,
                    cancellationToken);

                totalProcesados++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Ocurrió un error al procesar la versión {IdVersion}.",
                    version.IdVersion);

                await RegistrarErrorAsync(
                    version,
                    exception,
                    cancellationToken);
            }
        }

        return totalProcesados;
    }

    private async Task ProcesarVersionAsync(
        DocumentoVersion version,
        ConfiguracionProcesamientoDocumentoDto configuracion,
        CancellationToken cancellationToken)
    {
        if (version.Documento is null)
        {
            throw new InvalidOperationException(
                "No se encontró el documento asociado a la versión.");
        }

        var procesamiento = version.Procesamiento;

        if (procesamiento is null)
        {
            procesamiento = new DocumentoProcesado(version.IdVersion);

            await _procesamientoRepository.AgregarAsync(
                procesamiento,
                cancellationToken);
        }

        procesamiento.Iniciar();

        version.Documento.ActualizarEstadoProcesamiento(
            EstadoProcesamientoDocumento.EnProceso);

        await RegistrarAuditoriaAsync(
            version.IdDocumento,
            "Procesamiento iniciado.",
            "ProcesamientoIniciado",
            cancellationToken);

        await _procesamientoRepository.GuardarCambiosAsync(
            cancellationToken);

        var extractor = _extractores.FirstOrDefault(item =>
            item.Soporta(version.NombreArchivo));

        if (extractor is null)
        {
            throw new InvalidOperationException(
                "No existe un extractor configurado para este tipo de archivo.");
        }

        using var contenido = await _almacenamientoService
            .AbrirLecturaAsync(
                version.RutaArchivo,
                cancellationToken);

        var paginasExtraidas = await extractor.ExtraerAsync(
            contenido,
            cancellationToken);

        if (paginasExtraidas.Count == 0)
        {
            throw new InvalidOperationException(
                "El documento no contiene páginas procesables.");
        }

        var paginasNormalizadas = paginasExtraidas
            .Select(pagina => new PaginaTextoDocumentoDto
            {
                NumeroPagina = pagina.NumeroPagina,
                Texto = _normalizador.Normalizar(pagina.Texto)
            })
            .Where(pagina =>
                !string.IsNullOrWhiteSpace(pagina.Texto))
            .ToArray();

        var totalCaracteres = paginasNormalizadas.Sum(
            pagina => pagina.Texto.Length);

        if (totalCaracteres == 0)
        {
            throw new InvalidOperationException(
                "No fue posible extraer texto del documento.");
        }

        var chunksTexto = _chunkingService.GenerarChunks(
            paginasNormalizadas,
            configuracion);

        var chunks = chunksTexto
            .Select((chunk, indice) => new DocumentoChunk(
                version.IdDocumento,
                version.IdVersion,
                version.Documento.IdCategoria,
                indice + 1,
                chunk.PaginaInicial,
                chunk.PaginaFinal,
                chunk.Texto,
                indice + 1))
            .ToArray();

        procesamiento.Completar(
            paginasExtraidas.Count,
            totalCaracteres,
            chunks);

        version.Documento.ActualizarEstadoProcesamiento(
            EstadoProcesamientoDocumento.Procesado);

        await RegistrarAuditoriaAsync(
            version.IdDocumento,
            $"Procesamiento completado: {paginasExtraidas.Count} página(s), " +
            $"{totalCaracteres} caracteres y {chunks.Length} chunk(s).",
            "ProcesamientoCompletado",
            cancellationToken);

        await _procesamientoRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private async Task RegistrarErrorAsync(
        DocumentoVersion version,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var procesamiento = await _procesamientoRepository
            .ObtenerPorVersionAsync(
                version.IdVersion,
                cancellationToken);

        if (procesamiento is null)
        {
            procesamiento = new DocumentoProcesado(version.IdVersion);

            await _procesamientoRepository.AgregarAsync(
                procesamiento,
                cancellationToken);

            procesamiento.Iniciar();
        }

        procesamiento.RegistrarError(
            LimitarTexto(exception.Message, 2000));

        if (version.Documento is not null)
        {
            version.Documento.ActualizarEstadoProcesamiento(
                EstadoProcesamientoDocumento.Error);
        }

        await RegistrarAuditoriaAsync(
            version.IdDocumento,
            $"Error de procesamiento: {exception.Message}",
            "ProcesamientoError",
            cancellationToken);

        await _procesamientoRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private async Task RegistrarAuditoriaAsync(
        int idDocumento,
        string descripcion,
        string accion,
        CancellationToken cancellationToken)
    {
        var actividad = new AuditoriaActividad(
            idUsuario: null,
            modulo: "GestorDocumental",
            accion: accion,
            descripcion: LimitarTexto(descripcion, 1000),
            direccionIP: "Sistema",
            idDocumento: idDocumento);

        await _auditoriaRepository.AgregarActividadAsync(
            actividad,
            cancellationToken);
    }

    private static string LimitarTexto(
        string texto,
        int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return "Sin detalle.";
        }

        return texto.Length <= longitudMaxima
            ? texto
            : texto[..longitudMaxima];
    }
}