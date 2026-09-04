using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Asistente.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class ProcesadorDocumentoServiceTests
{
    private const int IdDocumentoPrueba = 10;
    private const int IdVersionPrueba = 25;

    [Fact]
    public async Task ProcesarPendientesAsync_Cuando_Extractor_Falla_Registra_Error_Y_Auditoria()
    {
        var documento = new Documento(
            "PRU-ERR-001",
            "Documento inválido",
            "Documento para probar errores.",
            1,
            "admin");

        AsignarPropiedad(
            documento,
            nameof(Documento.IdDocumento),
            IdDocumentoPrueba);

        var version = new DocumentoVersion(
            1,
            "documento-invalido.pdf",
            "documento-invalido.pdf",
            1024,
            "HASH-DE-PRUEBA",
            "admin");

        documento.AgregarVersion(version);

        AsignarPropiedad(
            version,
            nameof(DocumentoVersion.IdVersion),
            IdVersionPrueba);

        AsignarPropiedad(
            version,
            nameof(DocumentoVersion.IdDocumento),
            IdDocumentoPrueba);

        AsignarPropiedad(
            version,
            nameof(DocumentoVersion.Documento),
            documento);

        var procesamientoConError = new DocumentoProcesado(
            IdVersionPrueba);

        var procesamientoRepositoryMock =
            new Mock<IDocumentoProcesadoRepository>();

        var almacenamientoServiceMock =
            new Mock<IAlmacenamientoDocumentoService>();

        var extractorMock = new Mock<IExtractorTextoDocumento>();

        var normalizadorMock =
            new Mock<INormalizadorContenidoDocumento>();

        var chunkingServiceMock =
            new Mock<IChunkingDocumentoService>();

        var configuracionMock =
            new Mock<IConfiguracionProcesamientoDocumento>();

        var auditoriaRepositoryMock =
            new Mock<IAuditoriaRepository>();

        var loggerMock =
            new Mock<ILogger<ProcesadorDocumentoService>>();

        procesamientoRepositoryMock
            .Setup(repository => repository.ObtenerVersionesPendientesAsync(
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<DocumentoVersion>)new[]
                {
                    version
                });

        procesamientoRepositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<DocumentoProcesado>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        procesamientoRepositoryMock
            .Setup(repository => repository.ObtenerPorVersionAsync(
                IdVersionPrueba,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(procesamientoConError);

        procesamientoRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        almacenamientoServiceMock
            .Setup(service => service.AbrirLecturaAsync(
                "documento-invalido.pdf",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (Stream)new MemoryStream(new byte[] { 1, 2, 3 }));

        extractorMock
            .Setup(extractor => extractor.Soporta(
                "documento-invalido.pdf"))
            .Returns(true);

        extractorMock
            .Setup(extractor => extractor.ExtraerAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidDataException(
                    "El PDF está dañado para la prueba."));

        configuracionMock
            .Setup(configuracion => configuracion.Obtener())
            .Returns(CrearConfiguracion());

        auditoriaRepositoryMock
            .Setup(repository => repository.AgregarActividadAsync(
                It.IsAny<AuditoriaActividad>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ProcesadorDocumentoService(
            procesamientoRepositoryMock.Object,
            almacenamientoServiceMock.Object,
            new[] { extractorMock.Object },
            normalizadorMock.Object,
            chunkingServiceMock.Object,
            configuracionMock.Object,
            auditoriaRepositoryMock.Object,
            loggerMock.Object);

        var totalProcesados = await service.ProcesarPendientesAsync();

        Assert.Equal(0, totalProcesados);

        Assert.Equal(
            EstadoProcesamientoDocumento.Error,
            documento.EstadoProcesamiento);

        Assert.Equal(
            EstadoProcesamientoDocumento.Error,
            procesamientoConError.Estado);

        Assert.Contains(
            "El PDF está dañado para la prueba.",
            procesamientoConError.Observaciones);

        procesamientoRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(actividad =>
                    actividad.Accion == "ProcesamientoIniciado"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(actividad =>
                    actividad.Accion == "ProcesamientoError" &&
                    actividad.Descripcion.Contains(
                        "El PDF está dañado para la prueba.")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_Debe_Ejecutar_Ciclo_Inicial_Del_Procesador()
    {
        var procesadorMock = new Mock<IProcesadorDocumentoService>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var optionsMonitorMock =
            new Mock<IOptionsMonitor<ProcesamientoDocumentalOptions>>();

        var cicloEjecutado = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        procesadorMock
            .Setup(procesador => procesador.ProcesarPendientesAsync(
                It.IsAny<CancellationToken>()))
            .Callback<CancellationToken>(
                _ => cicloEjecutado.TrySetResult(true))
            .ReturnsAsync(0);

        scopeFactoryMock
            .Setup(factory => factory.CreateScope())
            .Returns(scopeMock.Object);

        scopeMock
            .SetupGet(scope => scope.ServiceProvider)
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(provider => provider.GetService(
                typeof(IProcesadorDocumentoService)))
            .Returns(procesadorMock.Object);

        optionsMonitorMock
            .SetupGet(monitor => monitor.CurrentValue)
            .Returns(new ProcesamientoDocumentalOptions
            {
                FrecuenciaSegundos = 60
            });

        var service = new ProcesamientoDocumentosBackgroundServicePrueba(
            scopeFactoryMock.Object,
            optionsMonitorMock.Object,
            new Mock<
                ILogger<ProcesamientoDocumentosBackgroundService>>().Object);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        var ejecucion = service.EjecutarParaPruebaAsync(
            cancellationTokenSource.Token);

        await cicloEjecutado.Task.WaitAsync(TimeSpan.FromSeconds(3));

        cancellationTokenSource.Cancel();

        try
        {
            await ejecucion;
        }
        catch (OperationCanceledException)
        {
            // La cancelación controlada finaliza el temporizador del worker.
        }

        procesadorMock.Verify(
            procesador => procesador.ProcesarPendientesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        scopeFactoryMock.Verify(
            factory => factory.CreateScope(),
            Times.Once);

        serviceProviderMock.Verify(
            provider => provider.GetService(
                typeof(IProcesadorDocumentoService)),
            Times.Once);
    }

    private static ConfiguracionProcesamientoDocumentoDto
        CrearConfiguracion()
    {
        return new ConfiguracionProcesamientoDocumentoDto
        {
            TamanoMaximoChunk = 1200,
            SolapamientoChunk = 150,
            LongitudMinimaChunk = 200,
            FrecuenciaSegundos = 15,
            MaximoDocumentosPorCiclo = 2
        };
    }

    private static void AsignarPropiedad(
        object destino,
        string nombrePropiedad,
        object valor)
    {
        var propiedad = destino.GetType().GetProperty(nombrePropiedad);

        var setter = propiedad?.GetSetMethod(nonPublic: true);

        setter?.Invoke(destino, new[] { valor });
    }

    private sealed class ProcesamientoDocumentosBackgroundServicePrueba
    : ProcesamientoDocumentosBackgroundService
    {
        public ProcesamientoDocumentosBackgroundServicePrueba(
            IServiceScopeFactory scopeFactory,
            IOptionsMonitor<ProcesamientoDocumentalOptions> optionsMonitor,
            ILogger<ProcesamientoDocumentosBackgroundService> logger)
            : base(scopeFactory, optionsMonitor, logger)
        {
        }

        public Task EjecutarParaPruebaAsync(
            CancellationToken cancellationToken)
        {
            return ExecuteAsync(cancellationToken);
        }
    }
}