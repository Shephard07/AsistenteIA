using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class MensajeServiceTests
{
    [Fact]
    public async Task RegistrarAsync_Debe_Agregar_Mensaje_Y_Guardar_Cambios()
    {
        var repositoryMock = new Mock<IConversacionRepository>();

        repositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new MensajeService(repositoryMock.Object);
        var conversacion = new Conversacion();

        await service.RegistrarAsync(
            conversacion,
            RolMensaje.Usuario,
            "Mensaje de prueba",
            null);

        var mensaje = Assert.Single(conversacion.Mensajes);

        Assert.Equal(RolMensaje.Usuario, mensaje.Rol);
        Assert.Equal("Mensaje de prueba", mensaje.Contenido);

        repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}