using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Application.Validators;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using FluentValidation;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class EnviarMensajeServiceTests
{
    private readonly Mock<IConversacionRepository> _repositoryMock = new();
    private readonly Mock<IAIProvider> _aiProviderMock = new();
    private readonly EnviarMensajeRequestValidator _validator = new();

    [Fact]
    public async Task EjecutarAsync_Debe_Crear_Conversacion_Y_Devolver_Respuesta()
    {
        _repositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
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

        _repositoryMock.Verify(
            repository => repository.AgregarAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _aiProviderMock.Verify(
            provider => provider.SendAsync(
                It.Is<ChatRequestDto>(request =>
                    request.Mensajes.Count == 1 &&
                    request.Mensajes.First().Contenido ==
                    "Hola, necesito una recomendación."),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_Debe_Lanzar_Error_Cuando_No_Existe_Conversacion()
    {
        _repositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                99,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversacion?)null);

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
        _repositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
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

        _repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
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

        _aiProviderMock.Verify(
            provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private EnviarMensajeService CrearServicio()
    {
        return new EnviarMensajeService(
            _repositoryMock.Object,
            _aiProviderMock.Object,
            _validator);
    }
}