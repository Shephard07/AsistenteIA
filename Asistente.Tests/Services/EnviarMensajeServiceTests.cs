using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Application.Validators;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using FluentValidation;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class EnviarMensajeServiceTests
{
    private readonly Mock<IConversacionService> _conversacionServiceMock = new();
    private readonly Mock<IMensajeService> _mensajeServiceMock = new();
    private readonly Mock<IAIProvider> _aiProviderMock = new();
    private readonly EnviarMensajeRequestValidator _validator = new();

    [Fact]
    public async Task EjecutarAsync_Debe_Registrar_Mensajes_Y_Devolver_Respuesta()
    {
        var conversacion = new Conversacion();

        _conversacionServiceMock
            .Setup(service => service.ObtenerOCrearAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacion);

        _mensajeServiceMock
            .Setup(service => service.RegistrarAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<RolMensaje>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _aiProviderMock
            .Setup(provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponseDto
            {
                Contenido = "Respuesta generada para la prueba.",
                TiempoRespuestaMs = 250
            });

        var service = CrearServicio();

        var response = await service.EjecutarAsync(
            new EnviarMensajeRequestDto
            {
                Mensaje = "Hola, necesito una recomendación."
            });

        Assert.Equal("Respuesta generada para la prueba.", response.Respuesta);
        Assert.Equal(250, response.TiempoRespuestaMs);

        _mensajeServiceMock.Verify(
            service => service.RegistrarAsync(
                conversacion,
                RolMensaje.Usuario,
                "Hola, necesito una recomendación.",
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mensajeServiceMock.Verify(
            service => service.RegistrarAsync(
                conversacion,
                RolMensaje.Asistente,
                "Respuesta generada para la prueba.",
                250,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_Debe_Propagar_Error_Cuando_No_Existe_Conversacion()
    {
        _conversacionServiceMock
            .Setup(service => service.ObtenerOCrearAsync(
                99,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException(
                "La conversación solicitada no existe."));

        var service = CrearServicio();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.EjecutarAsync(new EnviarMensajeRequestDto
            {
                IdConversacion = 99,
                Mensaje = "Consulta de prueba"
            }));
    }

    [Fact]
    public async Task EjecutarAsync_Debe_Propagar_Error_Del_Proveedor_IA()
    {
        var conversacion = new Conversacion();

        _conversacionServiceMock
            .Setup(service => service.ObtenerOCrearAsync(
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacion);

        _mensajeServiceMock
            .Setup(service => service.RegistrarAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<RolMensaje>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _aiProviderMock
            .Setup(provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(
                "Ollama no está disponible."));

        var service = CrearServicio();

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.EjecutarAsync(new EnviarMensajeRequestDto
            {
                Mensaje = "Consulta de prueba"
            }));

        _mensajeServiceMock.Verify(
            service => service.RegistrarAsync(
                conversacion,
                RolMensaje.Usuario,
                "Consulta de prueba",
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_Debe_Lanzar_Error_De_Validacion_Con_Mensaje_Vacio()
    {
        var service = CrearServicio();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            service.EjecutarAsync(new EnviarMensajeRequestDto
            {
                Mensaje = string.Empty
            }));

        Assert.Contains(
            exception.Errors,
            error => error.ErrorMessage == "El mensaje es obligatorio.");

        _conversacionServiceMock.Verify(
            service => service.ObtenerOCrearAsync(
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private EnviarMensajeService CrearServicio()
    {
        return new EnviarMensajeService(
            _conversacionServiceMock.Object,
            _mensajeServiceMock.Object,
            _aiProviderMock.Object,
            _validator);
    }
}