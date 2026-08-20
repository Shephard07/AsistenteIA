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
    public async Task ObtenerOCrearAsync_Debe_Crear_Conversacion_Cuando_No_Se_Recibe_Id()
    {
        _repositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new ConversacionService(_repositoryMock.Object);

        var conversacion = await service.ObtenerOCrearAsync(null);

        Assert.NotNull(conversacion);

        _repositoryMock.Verify(
            repository => repository.AgregarAsync(
                conversacion,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ObtenerOCrearAsync_Debe_Devolver_Conversacion_Existente()
    {
        var conversacionEsperada = new Conversacion();

        _repositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacionEsperada);

        var service = new ConversacionService(_repositoryMock.Object);

        var conversacion = await service.ObtenerOCrearAsync(1);

        Assert.Same(conversacionEsperada, conversacion);
    }

    [Fact]
    public async Task ObtenerOCrearAsync_Debe_Lanzar_Error_Si_No_Existe_Conversacion()
    {
        _repositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                50,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversacion?)null);

        var service = new ConversacionService(_repositoryMock.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ObtenerOCrearAsync(50));
    }
}