using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

public class AdministracionRagService
    : IAdministracionRagService
{
    private readonly IEmbeddingConfiguracionRepository
        _configuracionRepository;

    private readonly IDocumentoIndexadoRepository
        _documentoIndexadoRepository;

    private readonly IVectorStore _vectorStore;

    private readonly IAuditoriaRepository _auditoriaRepository;

    public AdministracionRagService(
        IEmbeddingConfiguracionRepository configuracionRepository,
        IDocumentoIndexadoRepository documentoIndexadoRepository,
        IVectorStore vectorStore,
        IAuditoriaRepository auditoriaRepository)
    {
        _configuracionRepository = configuracionRepository;
        _documentoIndexadoRepository = documentoIndexadoRepository;
        _vectorStore = vectorStore;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<EstadoRagDto> ObtenerEstadoAsync(
        CancellationToken cancellationToken = default)
    {
        var configuracion = await _configuracionRepository
            .ObtenerActivaAsync(cancellationToken);

        var procesamientos = await _documentoIndexadoRepository
            .ListarProcesamientosAsync(cancellationToken);

        var documentos = procesamientos
            .Select(MapearDocumento)
            .ToArray();
        var indexados = documentos
    .Where(documento =>
        documento.Estado ==
        EstadoIndexacionDocumento.Indexado.ToString())
    .ToArray();

        var duraciones = indexados
            .Where(documento =>
                documento.FechaInicio.HasValue &&
                documento.FechaIndexacion.HasValue)
            .Select(documento =>
                (decimal)(documento.FechaIndexacion!.Value -
                    documento.FechaInicio!.Value).TotalSeconds)
            .ToArray();

        var baseVectorialDisponible = await ConsultarBaseVectorialAsync(
            cancellationToken);

        return new EstadoRagDto
        {
            BaseVectorialDisponible = baseVectorialDisponible,
            TotalChunks = documentos.Sum(documento => documento.TotalChunks),
            TotalEmbeddings = documentos.Sum(
    documento => documento.TotalEmbeddings),
            TiempoPromedioIndexacionSegundos = duraciones.Length == 0
    ? 0
    : Math.Round(duraciones.Average(), 2),

            Configuracion = new ConfiguracionRagDto
            {
                Proveedor = configuracion.Proveedor,
                ModeloEmbeddings = configuracion.ModeloEmbeddings,
                BaseVectorial = configuracion.BaseVectorial,
                CantidadResultados = configuracion.CantidadResultados,
                PuntajeMinimo = configuracion.PuntajeMinimo,
                LongitudMaximaContexto =
                    configuracion.LongitudMaximaContexto
            },
            TotalDocumentos = documentos.Length,
            TotalPendientes = documentos.Count(documento =>
            documento.Estado ==
            EstadoIndexacionDocumento.Pendiente.ToString()),
            TotalEnProceso = documentos.Count(documento =>
                documento.Estado ==
                EstadoIndexacionDocumento.EnProceso.ToString()),
            TotalIndexados = documentos.Count(documento =>
                documento.Estado ==
                EstadoIndexacionDocumento.Indexado.ToString()),
            TotalConError = documentos.Count(documento =>
    documento.Estado ==
    EstadoIndexacionDocumento.Error.ToString()),
            Documentos = documentos
        };
    }

    public async Task SolicitarReindexacionAsync(
    int idDocumento,
    int idUsuarioActor,
    ContextoClienteDto contextoCliente,
    CancellationToken cancellationToken = default)
    {
        if (idDocumento <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idDocumento),
                "El identificador del documento debe ser mayor que cero.");
        }

        if (idUsuarioActor <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idUsuarioActor),
                "El identificador del usuario debe ser mayor que cero.");
        }

        ArgumentNullException.ThrowIfNull(contextoCliente);

        var procesamiento = await _documentoIndexadoRepository
            .ObtenerProcesamientoActivoPorDocumentoAsync(
                idDocumento,
                cancellationToken);

        if (procesamiento is null)
        {
            throw new KeyNotFoundException(
                "El documento no tiene una versión procesada disponible " +
                "para reindexar.");
        }

        if (procesamiento.TotalChunks <= 0)
        {
            throw new InvalidOperationException(
                "El documento no contiene chunks disponibles para indexar.");
        }

        var indexacion = procesamiento.Indexacion;
        var esReindexacion = indexacion is not null;

        if (indexacion is null)
        {
            indexacion = new DocumentoIndexado(
                procesamiento.IdDocumentoProcesado);

            await _documentoIndexadoRepository.AgregarAsync(
                indexacion,
                cancellationToken);
        }
        else
        {
            indexacion.MarcarPendiente();

            await _vectorStore.EliminarPorDocumentoAsync(
                indexacion.IdentificadorVectorial,
                cancellationToken);
        }

        var actividad = new AuditoriaActividad(
            idUsuarioActor,
            "RAG",
            "SolicitarReindexacion",
            esReindexacion
                ? "Se solicitó la reindexación del documento."
                : "Se solicitó la indexación del documento.",
            contextoCliente.DireccionIP,
            idDocumento);

        await _auditoriaRepository.AgregarActividadAsync(
            actividad,
            cancellationToken);

        await _documentoIndexadoRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private async Task<bool> ConsultarBaseVectorialAsync(
    CancellationToken cancellationToken)
    {
        try
        {
            return await _vectorStore.EstaDisponibleAsync(
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return false;
        }
    }

    private static DocumentoIndexadoEstadoDto MapearDocumento(
        DocumentoProcesado procesamiento)
    {
        var version = procesamiento.VersionDocumento
            ?? throw new InvalidOperationException(
                "No se encontró la versión del documento procesado.");

        var documento = version.Documento
            ?? throw new InvalidOperationException(
                "No se encontró el documento asociado a la versión.");

        var indexacion = procesamiento.Indexacion;

        var estado = indexacion?.Estado ??
    EstadoIndexacionDocumento.Pendiente;

        return new DocumentoIndexadoEstadoDto
        {
            IdDocumento = documento.IdDocumento,
            CodigoDocumento = documento.Codigo,
            NombreDocumento = documento.Nombre,
            IdVersionDocumento = version.IdVersion,
            NumeroVersion = version.NumeroVersion,
            IdDocumentoProcesado =
                procesamiento.IdDocumentoProcesado,
            IdDocumentoIndexado =
                indexacion?.IdDocumentoIndexado,
            Estado = estado.ToString(),
            TotalChunks = indexacion?.TotalChunks ??
                procesamiento.TotalChunks,
            TotalEmbeddings = indexacion?.TotalEmbeddings ?? 0,
            FechaInicio = indexacion?.FechaInicio,
            FechaIndexacion = indexacion?.FechaIndexacion,
            Observaciones = indexacion?.Observaciones ?? string.Empty
        };
    }
}