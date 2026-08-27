using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class ConversacionGestionServiceTests
{
    private const int IdUsuarioPrueba = 99;
    private const int IdConversacionPrueba = 10;

    private readonly Mock<IConversacionRepository> _repositoryMock = new();

    [Fact]
    public async Task ListarAsync_Debe_Devolver_Historial_Del_Usuario()
    {
        var conversacion = new Conversacion(1, IdUsuarioPrueba);
        conversacion.Renombrar("Consulta sobre inventarios");

        _repositoryMock
            .Setup(repository => repository.ListarPorUsuarioAsync(
                IdUsuarioPrueba,
                null,
                false,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<Conversacion>)new[] { conversacion });

        var service = CrearServicio();

        var resultado = await service.ListarAsync(
            IdUsuarioPrueba,
            null,
            false,
            20);

        var item = Assert.Single(resultado);

        Assert.Equal("Consulta sobre inventarios", item.Titulo);
        Assert.Equal(
            EstadoConversacion.Activa.ToString(),
            item.Estado);
        Assert.Equal(0, item.TotalMensajes);
    }

    [Fact]
    public async Task RenombrarAsync_Debe_Actualizar_Titulo_Y_Guardar_Cambios()
    {
        var conversacion = CrearConversacionDelUsuario();

        ConfigurarConversacionEncontrada(conversacion);

        var service = CrearServicio();

        await service.RenombrarAsync(
            IdConversacionPrueba,
            IdUsuarioPrueba,
            "  Consulta de producción  ");

        Assert.Equal("Consulta de producción", conversacion.Titulo);

        _repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ArchivarAsync_Debe_Archivar_Conversacion_Y_Guardar_Cambios()
    {
        var conversacion = CrearConversacionDelUsuario();

        ConfigurarConversacionEncontrada(conversacion);

        var service = CrearServicio();

        await service.ArchivarAsync(
            IdConversacionPrueba,
            IdUsuarioPrueba);

        Assert.Equal(
            EstadoConversacion.Archivada,
            conversacion.Estado);

        _repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReactivarAsync_Debe_Activar_Conversacion_Archivada()
    {
        var conversacion = CrearConversacionDelUsuario();
        conversacion.Archivar();

        ConfigurarConversacionEncontrada(conversacion);

        var service = CrearServicio();

        await service.ReactivarAsync(
            IdConversacionPrueba,
            IdUsuarioPrueba);

        Assert.Equal(
            EstadoConversacion.Activa,
            conversacion.Estado);

        _repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_Debe_Realizar_Eliminacion_Logica()
    {
        var conversacion = CrearConversacionDelUsuario();

        ConfigurarConversacionEncontrada(conversacion);

        var service = CrearServicio();

        await service.EliminarAsync(
            IdConversacionPrueba,
            IdUsuarioPrueba);

        Assert.Equal(
            EstadoConversacion.Eliminada,
            conversacion.Estado);

        _repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ArchivarAsync_Debe_Lanzar_Error_Si_Conversacion_No_Pertenece_Al_Usuario()
    {
        _repositoryMock
            .Setup(repository => repository.ObtenerPorIdYUsuarioAsync(
                IdConversacionPrueba,
                IdUsuarioPrueba,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversacion?)null);

        var service = CrearServicio();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ArchivarAsync(
                IdConversacionPrueba,
                IdUsuarioPrueba));

        _repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ConversacionGestionService CrearServicio()
    {
        return new ConversacionGestionService(
            _repositoryMock.Object);
    }

    private static Conversacion CrearConversacionDelUsuario()
    {
        return new Conversacion(1, IdUsuarioPrueba);
    }

    private void ConfigurarConversacionEncontrada(
        Conversacion conversacion)
    {
        _repositoryMock
            .Setup(repository => repository.ObtenerPorIdYUsuarioAsync(
                IdConversacionPrueba,
                IdUsuarioPrueba,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacion);

        _repositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}