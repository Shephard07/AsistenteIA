using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

public class IndexadorDocumentoService : IIndexadorDocumentoService
{
    private readonly IDocumentoIndexadoRepository _indexadoRepository;
    private readonly IEmbeddingConfiguracionRepository
        _configuracionRepository;

    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;
    private readonly IAuditoriaRepository _auditoriaRepository;

    public IndexadorDocumentoService(
        IDocumentoIndexadoRepository indexadoRepository,
        IEmbeddingConfiguracionRepository configuracionRepository,
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore,
        IAuditoriaRepository auditoriaRepository)
    {
        _indexadoRepository = indexadoRepository;
        _configuracionRepository = configuracionRepository;
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
        _auditoriaRepository = auditoriaRepository;
    }

    public async Task<int> IndexarPendientesAsync(
        int cantidadMaxima,
        CancellationToken cancellationToken = default)
    {
        if (cantidadMaxima <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadMaxima),
                "La cantidad máxima debe ser mayor que cero.");
        }

        var configuracion = await _configuracionRepository
            .ObtenerActivaAsync(cancellationToken);

        var procesamientos = await _indexadoRepository
            .ObtenerProcesamientosPendientesAsync(
                cantidadMaxima,
                cancellationToken);

        var totalIndexados = 0;

        foreach (var procesamiento in procesamientos)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await IndexarProcesamientoAsync(
                    procesamiento,
                    configuracion,
                    cancellationToken);

                totalIndexados++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                await RegistrarErrorAsync(
                    procesamiento,
                    exception,
                    cancellationToken);
            }
        }

        return totalIndexados;
    }

    private async Task IndexarProcesamientoAsync(
        DocumentoProcesado procesamiento,
        EmbeddingConfiguracion configuracion,
        CancellationToken cancellationToken)
    {
        var version = procesamiento.VersionDocumento
            ?? throw new InvalidOperationException(
                "No se encontró la versión del documento procesado.");

        var documento = version.Documento
            ?? throw new InvalidOperationException(
                "No se encontró el documento asociado a la versión.");

        var chunks = procesamiento.Chunks
            .OrderBy(chunk => chunk.Orden)
            .ToArray();

        if (chunks.Length == 0)
        {
            throw new InvalidOperationException(
                "El documento procesado no tiene chunks para indexar.");
        }

        var indexacion = await _indexadoRepository
            .ObtenerPorProcesamientoAsync(
                procesamiento.IdDocumentoProcesado,
                cancellationToken);

        if (indexacion is null)
        {
            indexacion = new DocumentoIndexado(
                procesamiento.IdDocumentoProcesado);

            await _indexadoRepository.AgregarAsync(
                indexacion,
                cancellationToken);
        }

        indexacion.Iniciar(chunks.Length);

        await RegistrarAuditoriaAsync(
            documento.IdDocumento,
            "IndexacionIniciada",
            $"Indexación iniciada para {chunks.Length} chunk(s).",
            cancellationToken);

        await _indexadoRepository.GuardarCambiosAsync(
            cancellationToken);

        await _vectorStore.EliminarPorDocumentoAsync(
            indexacion.IdentificadorVectorial,
            cancellationToken);

        var totalEmbeddings = 0;

        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingProvider.GenerarAsync(
                chunk.Texto,
                configuracion.ModeloEmbeddings,
                cancellationToken);

            await _vectorStore.IndexarAsync(
                new DocumentoVectorialDto
                {
                    IdentificadorDocumentoIndexado =
                        indexacion.IdentificadorVectorial,

                    IdDocumento = documento.IdDocumento,
                    IdVersionDocumento = version.IdVersion,
                    IdDocumentoProcesado =
                        procesamiento.IdDocumentoProcesado,

                    IdCategoria = documento.IdCategoria,
                    NumeroChunk = chunk.NumeroChunk,
                    PaginaInicial = chunk.PaginaInicial,
                    PaginaFinal = chunk.PaginaFinal,
                    Texto = chunk.Texto,
                    Embedding = embedding
                },
                cancellationToken);

            totalEmbeddings++;
        }

        indexacion.Completar(totalEmbeddings);

        await RegistrarAuditoriaAsync(
            documento.IdDocumento,
            "IndexacionCompletada",
            $"Indexación completada: {chunks.Length} chunk(s) y " +
            $"{totalEmbeddings} embedding(s).",
            cancellationToken);

        await _indexadoRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private async Task RegistrarErrorAsync(
        DocumentoProcesado procesamiento,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var indexacion = await _indexadoRepository
            .ObtenerPorProcesamientoAsync(
                procesamiento.IdDocumentoProcesado,
                cancellationToken);

        if (indexacion is null)
        {
            indexacion = new DocumentoIndexado(
                procesamiento.IdDocumentoProcesado);

            await _indexadoRepository.AgregarAsync(
                indexacion,
                cancellationToken);
        }

        try
        {
            await _vectorStore.EliminarPorDocumentoAsync(
                indexacion.IdentificadorVectorial,
                cancellationToken);
        }
        catch
        {
            // Se conserva el error original de indexación.
        }

        indexacion.RegistrarError(
            LimitarTexto(exception.Message, 2000));

        if (procesamiento.VersionDocumento?.Documento is not null)
        {
            await RegistrarAuditoriaAsync(
                procesamiento.VersionDocumento.IdDocumento,
                "IndexacionError",
                $"Error de indexación: {exception.Message}",
                cancellationToken);
        }

        await _indexadoRepository.GuardarCambiosAsync(
            cancellationToken);
    }

    private async Task RegistrarAuditoriaAsync(
        int idDocumento,
        string accion,
        string descripcion,
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