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
    private const int IdUsuarioPrueba = 99;

    private readonly Mock<IConversacionService> _conversacionServiceMock = new();
    private readonly Mock<IMensajeService> _mensajeServiceMock = new();
    private readonly Mock<IAsistenteService> _asistenteServiceMock = new();
    private readonly Mock<IPromptSistemaService> _promptSistemaServiceMock = new();

    private readonly Mock<IConfiguracionMemoriaService>
        _configuracionMemoriaServiceMock = new();

    private readonly Mock<IContextoConversacionalService>
        _contextoConversacionalServiceMock = new();

    private readonly Mock<IGeneradorTituloConversacionService>
        _generadorTituloConversacionServiceMock = new();

    private readonly Mock<IResumenConversacionService>
        _resumenConversacionServiceMock = new();

    private readonly Mock<IPromptBuilder> _promptBuilderMock = new();

    private readonly Mock<IRecuperacionContextoRagService>
    _recuperacionContextoRagServiceMock = new();

    private readonly Mock<IAIProvider> _aiProviderMock = new();
    private readonly EnviarMensajeRequestValidator _validator = new();

    [Fact]
    public async Task EjecutarAsync_Debe_Registrar_Mensajes_Y_Devolver_Respuesta()
    {
        var conversacion = new Conversacion();
        ConfigurarAsistenteYPromptActivos();
        ConfigurarRegistroMensajes();

        _conversacionServiceMock
            .Setup(service => service.ObtenerOCrearAsync(
                null,
                1,
                IdUsuarioPrueba,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacion);

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
            },
            IdUsuarioPrueba);

        Assert.Equal("Respuesta generada para la prueba.", response.Respuesta);
        Assert.Equal(250, response.TiempoRespuestaMs);

        _promptBuilderMock.Verify(
    builder => builder.ConstruirSolicitudChat(
        It.IsAny<AsistenteDto>(),
        It.IsAny<PromptSistemaDto>(),
        It.IsAny<IReadOnlyCollection<MensajeDto>>(),
        It.IsAny<string?>(),
        It.IsAny<string?>()),
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
                IdUsuarioPrueba,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException(
                "La conversación solicitada no existe."));

        var service = CrearServicio();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.EjecutarAsync(
                new EnviarMensajeRequestDto
                {
                    IdConversacion = 99,
                    Mensaje = "Consulta de prueba"
                },
                IdUsuarioPrueba));
    }

    [Fact]
    public async Task EjecutarAsync_Debe_Propagar_Error_Del_Proveedor_IA()
    {
        var conversacion = new Conversacion();
        ConfigurarAsistenteYPromptActivos();
        ConfigurarRegistroMensajes();

        _conversacionServiceMock
            .Setup(service => service.ObtenerOCrearAsync(
                null,
                1,
                IdUsuarioPrueba,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacion);

        _aiProviderMock
            .Setup(provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(
                "Ollama no está disponible."));

        var service = CrearServicio();

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.EjecutarAsync(
                new EnviarMensajeRequestDto
                {
                    Mensaje = "Consulta de prueba"
                },
                IdUsuarioPrueba));

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
    public async Task EjecutarAsync_Debe_Enviar_Solo_Mensajes_Permitidos_Y_Resumen()
    {
        var conversacion = new Conversacion(1, IdUsuarioPrueba);

        conversacion.AgregarMensaje(new Mensaje(
            RolMensaje.Usuario,
            "Primer mensaje de la conversación."));

        conversacion.AgregarMensaje(new Mensaje(
            RolMensaje.Asistente,
            "Primera respuesta de la conversación.",
            100));

        conversacion.AgregarMensaje(new Mensaje(
            RolMensaje.Usuario,
            "Segundo mensaje de la conversación."));

        conversacion.ActualizarResumenContexto(
            "El usuario solicita apoyo para organizar inventarios.");

        ConfigurarAsistenteYPromptActivos();
        ConfigurarRegistroMensajes();

        _conversacionServiceMock
            .Setup(service => service.ObtenerOCrearAsync(
                null,
                1,
                IdUsuarioPrueba,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversacion);

        _configuracionMemoriaServiceMock
            .Setup(service => service.ObtenerActivaAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConfiguracionMemoriaDto
            {
                IdConfiguracion = 1,
                MaximoMensajesContexto = 2,
                MaximoTokensContexto = 1000,
                LongitudResumen = 500,
                CantidadConversacionesVisibles = 20,
                Activo = true
            });

        IReadOnlyCollection<MensajeDto>? mensajesEnviados = null;
        string? resumenEnviado = null;

        _promptBuilderMock
    .Setup(builder => builder.ConstruirSolicitudChat(
        It.IsAny<AsistenteDto>(),
        It.IsAny<PromptSistemaDto>(),
        It.IsAny<IReadOnlyCollection<MensajeDto>>(),
        It.IsAny<string?>(),
        It.IsAny<string?>()))
    .Callback((
        AsistenteDto asistente,
        PromptSistemaDto prompt,
        IReadOnlyCollection<MensajeDto> mensajes,
        string? resumen,
        string? contextoDocumental) =>
    {
        mensajesEnviados = mensajes;
        resumenEnviado = resumen;
    })
    .Returns(new ChatRequestDto());

        _aiProviderMock
            .Setup(provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponseDto
            {
                Contenido = "Respuesta de prueba.",
                TiempoRespuestaMs = 100
            });

        var service = CrearServicio();

        await service.EjecutarAsync(
            new EnviarMensajeRequestDto
            {
                Mensaje = "Nuevo mensaje de prueba."
            },
            IdUsuarioPrueba);

        Assert.NotNull(mensajesEnviados);
        Assert.Equal(2, mensajesEnviados.Count);
        Assert.Equal(
            "Primera respuesta de la conversación.",
            mensajesEnviados.First().Contenido);
        Assert.Equal(
            "Segundo mensaje de la conversación.",
            mensajesEnviados.Last().Contenido);

        Assert.Equal(
            "El usuario solicita apoyo para organizar inventarios.",
            resumenEnviado);
    }

    [Fact]
    public async Task EjecutarAsync_Debe_Lanzar_Error_De_Validacion_Con_Mensaje_Vacio()
    {
        var service = CrearServicio();

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            service.EjecutarAsync(
                new EnviarMensajeRequestDto
                {
                    Mensaje = string.Empty
                },
                IdUsuarioPrueba));

        Assert.Contains(
            exception.Errors,
            error => error.ErrorMessage == "El mensaje es obligatorio.");

        _conversacionServiceMock.Verify(
            service => service.ObtenerOCrearAsync(
                It.IsAny<int?>(),
                It.IsAny<int>(),
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

        _configuracionMemoriaServiceMock
            .Setup(service => service.ObtenerActivaAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConfiguracionMemoriaDto
            {
                IdConfiguracion = 1,
                MaximoMensajesContexto = 10,
                MaximoTokensContexto = 1000,
                LongitudResumen = 500,
                CantidadConversacionesVisibles = 20,
                Activo = true
            });

        _contextoConversacionalServiceMock
            .Setup(service => service.Construir(
                It.IsAny<IReadOnlyCollection<MensajeDto>>(),
                It.IsAny<string?>(),
                It.IsAny<ConfiguracionMemoriaDto>()))
            .Returns((
                IReadOnlyCollection<MensajeDto> mensajes,
                string? resumenContexto,
                ConfiguracionMemoriaDto configuracion) =>
                new ContextoConversacionalService().Construir(
                    mensajes,
                    resumenContexto,
                    configuracion));

        _generadorTituloConversacionServiceMock
            .Setup(service => service.GenerarSiEsNecesarioAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<AsistenteDto>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _resumenConversacionServiceMock
            .Setup(service => service.ActualizarSiEsNecesarioAsync(
                It.IsAny<Conversacion>(),
                It.IsAny<AsistenteDto>(),
                It.IsAny<ConfiguracionMemoriaDto>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _promptBuilderMock
        .Setup(builder => builder.ConstruirSolicitudChat(
            It.IsAny<AsistenteDto>(),
            It.IsAny<PromptSistemaDto>(),
            It.IsAny<IReadOnlyCollection<MensajeDto>>(),
            It.IsAny<string?>(),
            It.IsAny<string?>()))
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
        _recuperacionContextoRagServiceMock
            .Setup(service => service.RecuperarAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContextoRagDto());

        return new EnviarMensajeService(
            _conversacionServiceMock.Object,
            _mensajeServiceMock.Object,
            _asistenteServiceMock.Object,
            _promptSistemaServiceMock.Object,
            _configuracionMemoriaServiceMock.Object,
            _contextoConversacionalServiceMock.Object,
            _recuperacionContextoRagServiceMock.Object,
            _generadorTituloConversacionServiceMock.Object,
            _promptBuilderMock.Object,
            _aiProviderMock.Object,
            _resumenConversacionServiceMock.Object,
            _validator);
    }
}