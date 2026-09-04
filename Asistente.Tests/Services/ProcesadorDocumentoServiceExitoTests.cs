using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class ProcesadorDocumentoServiceExitoTests
{
    [Fact]
    public async Task ProcesarPendientesAsync_Debe_Extraer_Normalizar_Y_Generar_Chunks()
    {
        var documentoRepositoryMock =
            new Mock<IDocumentoProcesadoRepository>();

        var almacenamientoServiceMock =
            new Mock<IAlmacenamientoDocumentoService>();

        var extractorMock = new Mock<IExtractorTextoDocumento>();

        var configuracionMock =
            new Mock<IConfiguracionProcesamientoDocumento>();

        var auditoriaRepositoryMock =
            new Mock<IAuditoriaRepository>();

        var documento = new Documento(
            "PROC-PRU-001",
            "Documento de procesamiento",
            "Documento para pruebas.",
            1,
            "admin");

        AsignarPropiedad(
            documento,
            nameof(Documento.IdDocumento),
            10);

        var version = new DocumentoVersion(
            1,
            "procesamiento.pdf",
            "C:\\Pruebas\\procesamiento.pdf",
            1024,
            "HASH-DE-PRUEBA",
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

        DocumentoProcesado? procesamientoRegistrado = null;

        documentoRepositoryMock
            .Setup(repository =>
                repository.ObtenerVersionesPendientesAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<DocumentoVersion>)new[] { version });

        documentoRepositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<DocumentoProcesado>(),
                It.IsAny<CancellationToken>()))
            .Callback<DocumentoProcesado, CancellationToken>(
                (procesamiento, _) =>
                    procesamientoRegistrado = procesamiento)
            .Returns(Task.CompletedTask);

        documentoRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        almacenamientoServiceMock
            .Setup(service => service.AbrirLecturaAsync(
                version.RutaArchivo,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));

        extractorMock
            .Setup(extractor => extractor.Soporta(version.NombreArchivo))
            .Returns(true);

        extractorMock
            .Setup(extractor => extractor.ExtraerAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<PaginaTextoDocumentoDto>)new[]
                {
                    new PaginaTextoDocumentoDto
                    {
                        NumeroPagina = 1,
                        Texto =
                            "  Primera página con   espacios y contenido " +
                            "de prueba.  "
                    },
                    new PaginaTextoDocumentoDto
                    {
                        NumeroPagina = 2,
                        Texto =
                            "Segunda página con contenido para generar " +
                            "fragmentos."
                    }
                });

        configuracionMock
            .Setup(configuracion => configuracion.Obtener())
            .Returns(new ConfiguracionProcesamientoDocumentoDto
            {
                TamanoMaximoChunk = 200,
                SolapamientoChunk = 20,
                LongitudMinimaChunk = 20,
                FrecuenciaSegundos = 15,
                MaximoDocumentosPorCiclo = 2
            });

        auditoriaRepositoryMock
            .Setup(repository => repository.AgregarActividadAsync(
                It.IsAny<AuditoriaActividad>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ProcesadorDocumentoService(
            documentoRepositoryMock.Object,
            almacenamientoServiceMock.Object,
            new[] { extractorMock.Object },
            new NormalizadorContenidoDocumentoService(),
            new ChunkingDocumentoService(),
            configuracionMock.Object,
            auditoriaRepositoryMock.Object,
            new Mock<ILogger<ProcesadorDocumentoService>>().Object);

        var totalProcesados = await service.ProcesarPendientesAsync();

        var procesamiento = Assert.IsType<DocumentoProcesado>(
            procesamientoRegistrado);

        Assert.Equal(1, totalProcesados);

        Assert.Equal(
            EstadoProcesamientoDocumento.Procesado,
            documento.EstadoProcesamiento);

        Assert.Equal(
            EstadoProcesamientoDocumento.Procesado,
            procesamiento.Estado);

        Assert.Equal(2, procesamiento.TotalPaginas);
        Assert.True(procesamiento.TotalCaracteres > 0);
        Assert.True(procesamiento.TotalChunks > 0);
        Assert.NotNull(procesamiento.FechaInicio);
        Assert.NotNull(procesamiento.FechaFin);
        Assert.Equal(
            procesamiento.TotalChunks,
            procesamiento.Chunks.Count);

        Assert.Contains(
            procesamiento.Chunks,
            chunk => chunk.Texto.Contains(
                "Primera página con espacios y contenido de prueba."));

        Assert.All(procesamiento.Chunks, chunk =>
        {
            Assert.Equal(10, chunk.IdDocumento);
            Assert.Equal(20, chunk.IdVersionDocumento);
            Assert.Equal(1, chunk.IdCategoria);
            Assert.True(chunk.NumeroChunk > 0);
            Assert.True(chunk.Orden > 0);
            Assert.True(chunk.PaginaInicial >= 1);
            Assert.True(chunk.PaginaFinal >= chunk.PaginaInicial);
            Assert.False(string.IsNullOrWhiteSpace(chunk.Texto));
        });

        auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "ProcesamientoIniciado"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "ProcesamientoCompletado"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        documentoRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
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