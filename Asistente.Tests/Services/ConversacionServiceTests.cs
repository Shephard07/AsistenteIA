using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class ConversacionServiceTests
{
    private readonly Mock<IConversacionRepository> _repositoryMock = new();

    [Fact]
    public async Task ObtenerOCrearAsync_Debe_Crear_Conversacion_Con_Propietario_Cuando_No_Se_Recibe_Id()
    {
        _repositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ConversacionService(_repositoryMock.Object);

        var conversacion = await service.ObtenerOCrearAsync(
            null,
            1,
            99);

        Assert.NotNull(conversacion);
        Assert.Equal(1, conversacion.IdAsistente);
        Assert.Equal(99, conversacion.IdUsuario);

        _repositoryMock.Verify(
            repository => repository.AgregarAsync(
                conversacion,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerOCrearAsync_Debe_Devolver_Conversacion_Del_Usuario()
    {
        var conversacionEsperada = new Conversacion(
            1,
            99);

        _repositoryMock
            .Setup(repository => repository.ObtenerPorIdYUsuarioAsync(
                1,
                99,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacionEsperada);

        var service = new ConversacionService(_repositoryMock.Object);

        var conversacion = await service.ObtenerOCrearAsync(
            1,
            1,
            99);

        Assert.Same(conversacionEsperada, conversacion);
    }

    [Fact]
    public async Task ObtenerOCrearAsync_Debe_Lanzar_Error_Si_No_Existe_Conversacion_Del_Usuario()
    {
        _repositoryMock
            .Setup(repository => repository.ObtenerPorIdYUsuarioAsync(
                50,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversacion?)null);

        var service = new ConversacionService(_repositoryMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ObtenerOCrearAsync(
                50,
                1,
                1));
    }
}