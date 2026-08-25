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
    private readonly Mock<IAsistenteService> _asistenteServiceMock = new();
    private readonly Mock<IPromptSistemaService> _promptSistemaServiceMock = new();
    private readonly Mock<IPromptBuilder> _promptBuilderMock = new();
    private readonly Mock<IAIProvider> _aiProviderMock = new();
    private readonly EnviarMensajeRequestValidator _validator = new();

    [Fact]
    public async Task EjecutarAsync_Debe_Registrar_Mensajes_Y_Devolver_Respuesta()
    {
        var conversacion = new Conversacion();
        ConfigurarAsistenteYPromptActivos();

        _conversacionServiceMock
            .Setup(service => service.ObtenerOCrearAsync(
                null,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacion);

        ConfigurarRegistroMensajes();

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

        _promptBuilderMock.Verify(
            builder => builder.ConstruirSolicitudChat(
                It.IsAny<AsistenteDto>(),
                It.IsAny<PromptSistemaDto>(),
                It.IsAny<IReadOnlyCollection<MensajeDto>>()),
            Times.Once);

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
        ConfigurarAsistenteYPromptActivos();

        _conversacionServiceMock
            .Setup(service => service.ObtenerOCrearAsync(
                99,
                1,
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
        ConfigurarAsistenteYPromptActivos();

        _conversacionServiceMock
            .Setup(service => service.ObtenerOCrearAsync(
                null,
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacion);

        ConfigurarRegistroMensajes();

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
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void ConfigurarAsistenteYPromptActivos()
    {
        var asistente = new AsistenteDto
        {
            IdAsistente = 1,
            Nombre = "Asistente de prueba",
            ModeloIA = "deepseek-r1:7b",
            Temperatura = 0.5m,
            MaxTokens = 512,
            TimeoutSeconds = 120
        };

        var prompt = new PromptSistemaDto
        {
            IdPrompt = 1,
            IdAsistente = 1,
            Nombre = "Prompt de prueba",
            Contenido = "Responde de manera profesional.",
            Version = 1,
            Activo = true
        };

        _asistenteServiceMock
            .Setup(service => service.ObtenerActivoAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(asistente);

        _promptSistemaServiceMock
            .Setup(service => service.ObtenerActivoPorAsistenteAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(prompt);

        _promptBuilderMock
            .Setup(builder => builder.ConstruirSolicitudChat(
                It.IsAny<AsistenteDto>(),
                It.IsAny<PromptSistemaDto>(),
                It.IsAny<IReadOnlyCollection<MensajeDto>>()))
            .Returns(new ChatRequestDto
            {
                ModeloIA = "deepseek-r1:7b",
                Temperatura = 0.5m,
                MaxTokens = 512,
                TimeoutSeconds = 120
            });
    }

    private void ConfigurarRegistroMensajes()
    {
        _mensajeServiceMock
            .Setup(service => service.RegistrarAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<RolMensaje>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private EnviarMensajeService CrearServicio()
    {
        return new EnviarMensajeService(
            _conversacionServiceMock.Object,
            _mensajeServiceMock.Object,
            _asistenteServiceMock.Object,
            _promptSistemaServiceMock.Object,
            _promptBuilderMock.Object,
            _aiProviderMock.Object,
            _validator);
    }
}