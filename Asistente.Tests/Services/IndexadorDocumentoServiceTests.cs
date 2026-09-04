using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class IndexadorDocumentoServiceTests
{
    private readonly Mock<IDocumentoIndexadoRepository>
        _indexadoRepositoryMock = new();

    private readonly Mock<IEmbeddingConfiguracionRepository>
        _configuracionRepositoryMock = new();

    private readonly Mock<IEmbeddingProvider>
        _embeddingProviderMock = new();

    private readonly Mock<IVectorStore> _vectorStoreMock = new();

    private readonly Mock<IAuditoriaRepository>
        _auditoriaRepositoryMock = new();

    [Fact]
    public async Task IndexarPendientesAsync_Debe_Lanzar_Error_Cuando_El_Limite_No_Es_Valido()
    {
        var service = CrearServicio();

        var exception = await Assert.ThrowsAsync<
            ArgumentOutOfRangeException>(
            () => service.IndexarPendientesAsync(0));

        Assert.Equal("cantidadMaxima", exception.ParamName);

        _configuracionRepositoryMock.Verify(
            repository => repository.ObtenerActivaAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IndexarPendientesAsync_Debe_Indexar_Chunks_En_Orden_Y_Completar_El_Proceso()
    {
        var procesamiento = CrearProcesamientoConChunks();

        DocumentoIndexado? indexacionRegistrada = null;
        var documentosVectoriales = new List<DocumentoVectorialDto>();

        ConfigurarDependencias(
            procesamiento,
            () => indexacionRegistrada,
            indexacion => indexacionRegistrada = indexacion);

        _embeddingProviderMock
            .Setup(provider => provider.GenerarAsync(
                It.IsAny<string>(),
                "nomic-embed-text",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((string texto, string _, CancellationToken _) =>
                texto == "Primer chunk para indexar."
                    ? new[] { 0.10f, 0.20f }
                    : new[] { 0.30f, 0.40f });

        _vectorStoreMock
            .Setup(store => store.IndexarAsync(
                It.IsAny<DocumentoVectorialDto>(),
                It.IsAny<CancellationToken>()))
            .Callback<DocumentoVectorialDto, CancellationToken>(
                (documento, _) => documentosVectoriales.Add(documento))
            .Returns(Task.CompletedTask);

        var service = CrearServicio();

        var totalIndexados = await service.IndexarPendientesAsync(2);

        var indexacion = Assert.IsType<DocumentoIndexado>(
            indexacionRegistrada);

        Assert.Equal(1, totalIndexados);

        Assert.Equal(
            EstadoIndexacionDocumento.Indexado,
            indexacion.Estado);

        Assert.Equal(2, indexacion.TotalChunks);
        Assert.Equal(2, indexacion.TotalEmbeddings);
        Assert.NotNull(indexacion.FechaInicio);
        Assert.NotNull(indexacion.FechaIndexacion);
        Assert.Empty(indexacion.Observaciones);

        Assert.Equal(2, documentosVectoriales.Count);

        Assert.Collection(
            documentosVectoriales,
            primerDocumento =>
            {
                Assert.Equal(1, primerDocumento.NumeroChunk);
                Assert.Equal(
                    "Primer chunk para indexar.",
                    primerDocumento.Texto);

                Assert.Equal(10, primerDocumento.IdDocumento);
                Assert.Equal(20, primerDocumento.IdVersionDocumento);
                Assert.Equal(30, primerDocumento.IdDocumentoProcesado);
                Assert.Equal(1, primerDocumento.IdCategoria);
            },
            segundoDocumento =>
            {
                Assert.Equal(2, segundoDocumento.NumeroChunk);
                Assert.Equal(
                    "Segundo chunk para indexar.",
                    segundoDocumento.Texto);
            });

        _vectorStoreMock.Verify(
            store => store.EliminarPorDocumentoAsync(
                indexacion.IdentificadorVectorial,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _embeddingProviderMock.Verify(
            provider => provider.GenerarAsync(
                It.IsAny<string>(),
                "nomic-embed-text",
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "IndexacionIniciada"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "IndexacionCompletada"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _indexadoRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task IndexarPendientesAsync_Debe_Registrar_Error_Si_Falla_La_Generacion_Del_Embedding()
    {
        var procesamiento = CrearProcesamientoConChunks();

        DocumentoIndexado? indexacionRegistrada = null;

        ConfigurarDependencias(
            procesamiento,
            () => indexacionRegistrada,
            indexacion => indexacionRegistrada = indexacion);

        _embeddingProviderMock
            .Setup(provider => provider.GenerarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Ollama no disponible."));

        var service = CrearServicio();

        var totalIndexados = await service.IndexarPendientesAsync(2);

        var indexacion = Assert.IsType<DocumentoIndexado>(
            indexacionRegistrada);

        Assert.Equal(0, totalIndexados);

        Assert.Equal(
            EstadoIndexacionDocumento.Error,
            indexacion.Estado);

        Assert.Equal(
            "Ollama no disponible.",
            indexacion.Observaciones);

        Assert.NotNull(indexacion.FechaIndexacion);

        _vectorStoreMock.Verify(
            store => store.EliminarPorDocumentoAsync(
                indexacion.IdentificadorVectorial,
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _vectorStoreMock.Verify(
            store => store.IndexarAsync(
                It.IsAny<DocumentoVectorialDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "IndexacionIniciada"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "IndexacionError"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "IndexacionCompletada"),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _indexadoRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    private IndexadorDocumentoService CrearServicio()
    {
        return new IndexadorDocumentoService(
            _indexadoRepositoryMock.Object,
            _configuracionRepositoryMock.Object,
            _embeddingProviderMock.Object,
            _vectorStoreMock.Object,
            _auditoriaRepositoryMock.Object);
    }

    private void ConfigurarDependencias(
        DocumentoProcesado procesamiento,
        Func<DocumentoIndexado?> obtenerIndexacion,
        Action<DocumentoIndexado> registrarIndexacion)
    {
        var configuracion = new EmbeddingConfiguracion(
            proveedor: "Ollama",
            modeloEmbeddings: "nomic-embed-text",
            baseVectorial: "ChromaDB",
            cantidadResultados: 3,
            puntajeMinimo: 0.60m,
            longitudMaximaContexto: 4000);

        _configuracionRepositoryMock
            .Setup(repository => repository.ObtenerActivaAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configuracion);

        _indexadoRepositoryMock
            .Setup(repository =>
                repository.ObtenerProcesamientosPendientesAsync(
                    2,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<DocumentoProcesado>)new[]
                {
                    procesamiento
                });

        _indexadoRepositoryMock
            .Setup(repository => repository.ObtenerPorProcesamientoAsync(
                procesamiento.IdDocumentoProcesado,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => obtenerIndexacion());

        _indexadoRepositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<DocumentoIndexado>(),
                It.IsAny<CancellationToken>()))
            .Callback<DocumentoIndexado, CancellationToken>(
                (indexacion, _) => registrarIndexacion(indexacion))
            .Returns(Task.CompletedTask);

        _indexadoRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _vectorStoreMock
            .Setup(store => store.EliminarPorDocumentoAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditoriaRepositoryMock
            .Setup(repository => repository.AgregarActividadAsync(
                It.IsAny<AuditoriaActividad>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static DocumentoProcesado CrearProcesamientoConChunks()
    {
        var documento = new Documento(
            "RAG-PRU-001",
            "Documento para indexación",
            "Documento utilizado en pruebas.",
            1,
            "admin");

        AsignarPropiedad(
            documento,
            nameof(Documento.IdDocumento),
            10);

        var version = new DocumentoVersion(
            1,
            "prueba-rag.pdf",
            "C:\\Pruebas\\prueba-rag.pdf",
            1024,
            "HASH-RAG-PRUEBA",
            "admin");

        documento.AgregarVersion(version);

        AsignarPropiedad(
            version,
            nameof(DocumentoVersion.IdVersion),
            20);

        AsignarPropiedad(
            version,
            nameof(DocumentoVersion.IdDocumento),
            10);

        AsignarPropiedad(
            version,
            nameof(DocumentoVersion.Documento),
            documento);

        var procesamiento = new DocumentoProcesado(20);

        AsignarPropiedad(
            procesamiento,
            nameof(DocumentoProcesado.IdDocumentoProcesado),
            30);

        AsignarPropiedad(
            procesamiento,
            nameof(DocumentoProcesado.VersionDocumento),
            version);

        var chunkOrdenDos = new DocumentoChunk(
            idDocumento: 10,
            idVersionDocumento: 20,
            idCategoria: 1,
            numeroChunk: 2,
            paginaInicial: 2,
            paginaFinal: 3,
            texto: "Segundo chunk para indexar.",
            orden: 2);

        var chunkOrdenUno = new DocumentoChunk(
            idDocumento: 10,
            idVersionDocumento: 20,
            idCategoria: 1,
            numeroChunk: 1,
            paginaInicial: 1,
            paginaFinal: 1,
            texto: "Primer chunk para indexar.",
            orden: 1);

        procesamiento.Chunks.Add(chunkOrdenDos);
        procesamiento.Chunks.Add(chunkOrdenUno);

        return procesamiento;
    }

    private static void AsignarPropiedad(
        object entidad,
        string nombrePropiedad,
        object valor)
    {
        var propiedad = entidad.GetType().GetProperty(nombrePropiedad);

        var setter = propiedad?.GetSetMethod(nonPublic: true);

        setter?.Invoke(entidad, new[] { valor });
    }
}