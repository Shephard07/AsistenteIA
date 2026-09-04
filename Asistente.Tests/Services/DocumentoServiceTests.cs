using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using FluentValidation;
using Moq;
using Xunit;
using FluentValidation.Results;

namespace Asistente.Tests.Services;

public class DocumentoServiceTests
{
    private const int IdDocumentoPrueba = 10;
    private const int IdUsuarioPrueba = 99;

    private readonly Mock<IDocumentoRepository> _documentoRepositoryMock =
        new();

    private readonly Mock<ICategoriaDocumentoRepository>
        _categoriaRepositoryMock = new();

    private readonly Mock<IAlmacenamientoDocumentoService>
        _almacenamientoServiceMock = new();

    private readonly Mock<IDocumentoChunkRepository>
    _documentoChunkRepositoryMock = new();

    private readonly Mock<IAuditoriaRepository> _auditoriaRepositoryMock =
        new();

    private readonly Mock<IValidator<CrearDocumentoRequestDto>>
        _crearValidatorMock = new();

    private readonly Mock<IValidator<ActualizarDocumentoRequestDto>>
        _actualizarValidatorMock = new();

    [Fact]
    public async Task ListarAsync_Debe_Devolver_Resumen_De_Documentos()
    {
        var documento = CrearDocumentoConVersion();
        documento.Activar();

        _documentoRepositoryMock
            .Setup(repository => repository.ListarAsync(
                null,
                null,
                null,
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<Documento>)new[] { documento });

        var service = CrearServicio();

        var resultado = await service.ListarAsync(
            null,
            null,
            null,
            null,
            null);

        var item = Assert.Single(resultado);

        Assert.Equal("MAN-SEG-001", item.Codigo);
        Assert.Equal(1, item.VersionActual);
        Assert.Equal(EstadoDocumento.Activo.ToString(), item.Estado);
    }

    [Fact]
    public async Task ListarChunksAsync_Debe_Devolver_Chunks_Con_Metadatos()
    {
        const int idVersionPrueba = 25;

        var documento = CrearDocumentoConVersion();
        AsignarIdDocumento(documento, IdDocumentoPrueba);

        var version = Assert.Single(documento.Versiones);
        AsignarIdVersion(version, idVersionPrueba);

        ConfigurarDocumentoEncontrado(documento);

        var chunkUno = new DocumentoChunk(
            IdDocumentoPrueba,
            idVersionPrueba,
            1,
            1,
            1,
            1,
            "Primer fragmento del documento.",
            1);

        var chunkDos = new DocumentoChunk(
            IdDocumentoPrueba,
            idVersionPrueba,
            1,
            2,
            2,
            3,
            "Segundo fragmento del documento.",
            2);

        _documentoChunkRepositoryMock
            .Setup(repository => repository.ListarPorDocumentoYVersionAsync(
                IdDocumentoPrueba,
                idVersionPrueba,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<DocumentoChunk>)new[]
                {
                chunkUno,
                chunkDos
                });

        var service = CrearServicio();

        var resultado = await service.ListarChunksAsync(
            IdDocumentoPrueba,
            idVersionPrueba);

        var items = resultado.ToArray();

        Assert.Equal(2, items.Length);

        Assert.Equal("MAN-SEG-001", items[0].CodigoDocumento);
        Assert.Equal("Manual de seguridad", items[0].NombreDocumento);
        Assert.Equal(1, items[0].NumeroVersion);
        Assert.Equal(1, items[0].Orden);
        Assert.Equal(1, items[0].PaginaInicial);
        Assert.Equal(1, items[0].PaginaFinal);
        Assert.Equal(
            "Primer fragmento del documento.",
            items[0].Texto);

        Assert.Equal(2, items[1].Orden);
        Assert.Equal(2, items[1].PaginaInicial);
        Assert.Equal(3, items[1].PaginaFinal);
        Assert.Equal(
            "Segundo fragmento del documento.",
            items[1].Texto);

        _documentoChunkRepositoryMock.Verify(
            repository => repository.ListarPorDocumentoYVersionAsync(
                IdDocumentoPrueba,
                idVersionPrueba,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivarAsync_Debe_Activar_Documento_Y_Registrar_Auditoria()
    {
        var documento = CrearDocumentoConVersion();
        ConfigurarDocumentoEncontrado(documento);

        var service = CrearServicio();

        await service.ActivarAsync(
            IdDocumentoPrueba,
            IdUsuarioPrueba,
            CrearContextoCliente());

        Assert.Equal(EstadoDocumento.Activo, documento.Estado);

        VerificarCambiosYAuditoria("ActivarDocumento");
    }

    [Fact]
    public async Task ArchivarAsync_Debe_Archivar_Documento_Y_Registrar_Auditoria()
    {
        var documento = CrearDocumentoConVersion();
        documento.Activar();

        ConfigurarDocumentoEncontrado(documento);

        var service = CrearServicio();

        await service.ArchivarAsync(
            IdDocumentoPrueba,
            IdUsuarioPrueba,
            CrearContextoCliente());

        Assert.Equal(EstadoDocumento.Archivado, documento.Estado);

        VerificarCambiosYAuditoria("ArchivarDocumento");
    }

    [Fact]
    public async Task EliminarAsync_Debe_Realizar_Eliminacion_Logica()
    {
        var documento = CrearDocumentoConVersion();
        documento.Activar();

        ConfigurarDocumentoEncontrado(documento);

        var service = CrearServicio();

        await service.EliminarAsync(
            IdDocumentoPrueba,
            IdUsuarioPrueba,
            CrearContextoCliente());

        Assert.Equal(EstadoDocumento.Eliminado, documento.Estado);

        VerificarCambiosYAuditoria("EliminarDocumento");
    }

    [Fact]
    public async Task ActivarAsync_Debe_Lanzar_Error_Si_Documento_Esta_Eliminado()
    {
        var documento = CrearDocumentoConVersion();
        documento.Eliminar();

        ConfigurarDocumentoEncontrado(documento);

        var service = CrearServicio();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ActivarAsync(
                IdDocumentoPrueba,
                IdUsuarioPrueba,
                CrearContextoCliente()));

        _documentoRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);

        _auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.IsAny<AuditoriaActividad>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ObtenerDetalleAsync_Debe_Lanzar_Error_Si_Documento_No_Existe()
    {
        _documentoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                IdDocumentoPrueba,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Documento?)null);

        var service = CrearServicio();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ObtenerDetalleAsync(IdDocumentoPrueba));
    }

    [Fact]
    public async Task CrearAsync_Debe_Registrar_Documento_Version_Inicial_Y_Auditoria()
    {
        ConfigurarValidatorCrearValido();

        Documento? documentoRegistrado = null;

        _documentoRepositoryMock
            .Setup(repository => repository.ObtenerPorCodigoAsync(
                "PROC-CAL-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Documento?)null);

        _categoriaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CategoriaDocumento(
                "Calidad",
                "Documentos de calidad."));

        _almacenamientoServiceMock
            .Setup(service => service.GuardarAsync(
                It.IsAny<ArchivoDocumentoCargaDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearArchivoAlmacenado());

        _documentoRepositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<Documento>(),
                It.IsAny<CancellationToken>()))
            .Callback<Documento, CancellationToken>(
                (documento, _) =>
                {
                    AsignarIdDocumento(documento, IdDocumentoPrueba);
                    documentoRegistrado = documento;
                })
            .Returns(Task.CompletedTask);

        _documentoRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _documentoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => documentoRegistrado);

        _auditoriaRepositoryMock
            .Setup(repository => repository.AgregarActividadAsync(
                It.IsAny<AuditoriaActividad>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditoriaRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CrearServicio();

        var resultado = await service.CrearAsync(
            CrearSolicitudDocumento(),
            CrearArchivoCarga(),
            IdUsuarioPrueba,
            "admin",
            CrearContextoCliente());

        Assert.Equal("PROC-CAL-001", resultado.Codigo);
        Assert.Equal(1, resultado.VersionActual);
        Assert.Equal(EstadoDocumento.Borrador.ToString(), resultado.Estado);

        var version = Assert.Single(resultado.Versiones);
        Assert.Equal(1, version.NumeroVersion);
        Assert.True(version.Activo);

        _almacenamientoServiceMock.Verify(
            service => service.GuardarAsync(
                It.IsAny<ArchivoDocumentoCargaDto>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _documentoRepositoryMock.Verify(
            repository => repository.AgregarAsync(
                It.IsAny<Documento>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        VerificarCambiosYAuditoria("CrearDocumento");
    }

    [Fact]
    public async Task CrearAsync_Debe_Rechazar_Categoria_Inactiva()
    {
        ConfigurarValidatorCrearValido();

        var categoria = new CategoriaDocumento(
            "Calidad",
            "Documentos de calidad.");

        categoria.Desactivar();

        _documentoRepositoryMock
            .Setup(repository => repository.ObtenerPorCodigoAsync(
                "PROC-CAL-001",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Documento?)null);

        _categoriaRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);

        var service = CrearServicio();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CrearAsync(
                CrearSolicitudDocumento(),
                CrearArchivoCarga(),
                IdUsuarioPrueba,
                "admin",
                CrearContextoCliente()));

        _almacenamientoServiceMock.Verify(
            service => service.GuardarAsync(
                It.IsAny<ArchivoDocumentoCargaDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _documentoRepositoryMock.Verify(
            repository => repository.AgregarAsync(
                It.IsAny<Documento>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AgregarVersionAsync_Debe_Mantener_Historica_La_Version_Anterior()
    {
        var documento = CrearDocumentoConVersion();
        ConfigurarDocumentoEncontrado(documento);

        _almacenamientoServiceMock
            .Setup(service => service.GuardarAsync(
                It.IsAny<ArchivoDocumentoCargaDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearArchivoAlmacenado());

        var service = CrearServicio();

        await service.AgregarVersionAsync(
            IdDocumentoPrueba,
            CrearArchivoCarga(),
            IdUsuarioPrueba,
            "admin",
            CrearContextoCliente());

        Assert.Equal(2, documento.VersionActual);
        Assert.Equal(2, documento.Versiones.Count);

        var version1 = documento.Versiones.Single(
            version => version.NumeroVersion == 1);

        var version2 = documento.Versiones.Single(
            version => version.NumeroVersion == 2);

        Assert.False(version1.Activo);
        Assert.True(version2.Activo);

        _almacenamientoServiceMock.Verify(
            service => service.GuardarAsync(
                It.IsAny<ArchivoDocumentoCargaDto>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        VerificarCambiosYAuditoria("AgregarVersionDocumento");
    }

    [Fact]
    public async Task AgregarVersionAsync_Debe_Rechazar_Documento_Archivado()
    {
        var documento = CrearDocumentoConVersion();
        documento.Archivar();

        ConfigurarDocumentoEncontrado(documento);

        var service = CrearServicio();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AgregarVersionAsync(
                IdDocumentoPrueba,
                CrearArchivoCarga(),
                IdUsuarioPrueba,
                "admin",
                CrearContextoCliente()));

        _almacenamientoServiceMock.Verify(
            service => service.GuardarAsync(
                It.IsAny<ArchivoDocumentoCargaDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private DocumentoService CrearServicio()
    {
        return new DocumentoService(
            _documentoRepositoryMock.Object,
            _categoriaRepositoryMock.Object,
            _almacenamientoServiceMock.Object,
            _documentoChunkRepositoryMock.Object,
            _auditoriaRepositoryMock.Object,
            _crearValidatorMock.Object,
            _actualizarValidatorMock.Object);
    }

    private void ConfigurarDocumentoEncontrado(Documento documento)
    {
        _documentoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                IdDocumentoPrueba,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(documento);

        _documentoRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditoriaRepositoryMock
            .Setup(repository => repository.AgregarActividadAsync(
                It.IsAny<AuditoriaActividad>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditoriaRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private void VerificarCambiosYAuditoria(string accionEsperada)
    {
        _documentoRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == accionEsperada &&
                        actividad.Modulo == "GestorDocumental"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Documento CrearDocumentoConVersion()
    {
        var documento = new Documento(
            "MAN-SEG-001",
            "Manual de seguridad",
            "Documento de prueba.",
            1,
            "admin");

        documento.AgregarVersion(new DocumentoVersion(
            1,
            "manual-seguridad.pdf",
            "C:\\Pruebas\\manual-seguridad.pdf",
            1024,
            "HASH-DE-PRUEBA",
            "admin"));

        return documento;
    }

    private static ContextoClienteDto CrearContextoCliente()
    {
        return new ContextoClienteDto
        {
            DireccionIP = "127.0.0.1"
        };
    }

    private void ConfigurarValidatorCrearValido()
    {
        _crearValidatorMock
            .Setup(validator => validator.ValidateAsync(
                It.IsAny<ValidationContext<CrearDocumentoRequestDto>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static CrearDocumentoRequestDto CrearSolicitudDocumento()
    {
        return new CrearDocumentoRequestDto
        {
            Codigo = "PROC-CAL-001",
            Nombre = "Procedimiento de control de calidad",
            Descripcion = "Documento de prueba.",
            IdCategoria = 1
        };
    }

    private static ArchivoDocumentoCargaDto CrearArchivoCarga()
    {
        return new ArchivoDocumentoCargaDto
        {
            NombreArchivo = "procedimiento.pdf",
            TipoContenido = "application/pdf",
            TamanoArchivo = 3,
            Contenido = new MemoryStream(new byte[] { 1, 2, 3 })
        };
    }

    private static ArchivoDocumentoAlmacenadoDto CrearArchivoAlmacenado()
    {
        return new ArchivoDocumentoAlmacenadoDto
        {
            NombreArchivo = "procedimiento.pdf",
            RutaArchivo = "C:\\Pruebas\\procedimiento.pdf",
            TamanoArchivo = 1024,
            HashArchivo = "HASH-DE-PRUEBA"
        };
    }

    private static void AsignarIdDocumento(
    Documento documento,
    int idDocumento)
    {
        var propiedad = typeof(Documento).GetProperty(
            nameof(Documento.IdDocumento));

        var setter = propiedad?.GetSetMethod(nonPublic: true);

        setter?.Invoke(documento, new object[] { idDocumento });
    }

    private static void AsignarIdVersion(
    DocumentoVersion version,
    int idVersion)
    {
        var propiedad = typeof(DocumentoVersion).GetProperty(
            nameof(DocumentoVersion.IdVersion));

        var setter = propiedad?.GetSetMethod(nonPublic: true);

        setter?.Invoke(version, new object[] { idVersion });
    }
}