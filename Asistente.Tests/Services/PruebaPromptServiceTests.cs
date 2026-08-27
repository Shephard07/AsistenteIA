using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Application.Validators;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class PruebaPromptServiceTests
{
    private readonly Mock<IAsistenteService> _asistenteServiceMock = new();
    private readonly Mock<IPromptSistemaService> _promptSistemaServiceMock = new();
    private readonly Mock<IPromptBuilder> _promptBuilderMock = new();
    private readonly Mock<IAIProvider> _aiProviderMock = new();
    private readonly ProbarPromptRequestValidator _validator = new();

    [Fact]
    public async Task ProbarAsync_Debe_Construir_Prompt_Y_Devolver_Respuesta_IA()
    {
        var asistente = CrearAsistente();
        var prompt = CrearPrompt();

        _asistenteServiceMock
            .Setup(service => service.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(asistente);

        _promptSistemaServiceMock
            .Setup(service => service.ListarPorAsistenteAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { prompt });

        _promptBuilderMock
            .Setup(builder => builder.ConstruirPromptSistema(
                asistente,
                prompt))
            .Returns("Prompt final generado para la prueba.");

        _promptBuilderMock
            .Setup(builder => builder.ConstruirSolicitudChat(
                asistente,
                prompt,
                It.IsAny<IReadOnlyCollection<MensajeDto>>(),
                null))
            .Returns(new ChatRequestDto
            {
                ModeloIA = "deepseek-r1:7b",
                Temperatura = 0.4m,
                MaxTokens = 512,
                TimeoutSeconds = 120
            });

        _aiProviderMock
            .Setup(provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponseDto
            {
                Contenido = "Respuesta de IA para la prueba.",
                TiempoRespuestaMs = 250
            });

        var service = CrearServicio();

        var response = await service.ProbarAsync(
            new ProbarPromptRequestDto
            {
                IdAsistente = 1,
                IdPrompt = 1,
                Mensaje = "¿Qué es un inventario?"
            });

        Assert.Equal(
            "Prompt final generado para la prueba.",
            response.PromptGenerado);

        Assert.Equal(
            "Respuesta de IA para la prueba.",
            response.Respuesta);

        Assert.Equal(250, response.TiempoRespuestaMs);

        _promptBuilderMock.Verify(
            builder => builder.ConstruirSolicitudChat(
                asistente,
                prompt,
                It.Is<IReadOnlyCollection<MensajeDto>>(
                    mensajes =>
                        mensajes.Count == 1 &&
                        mensajes.First().Contenido ==
                            "¿Qué es un inventario?"),
                null),
            Times.Once);

        _aiProviderMock.Verify(
            provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProbarAsync_Debe_Lanzar_Error_Si_Prompt_No_Pertenece_Al_Asistente()
    {
        _asistenteServiceMock
            .Setup(service => service.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CrearAsistente());

        _promptSistemaServiceMock
            .Setup(service => service.ListarPorAsistenteAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PromptSistemaDto>());

        var service = CrearServicio();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.ProbarAsync(new ProbarPromptRequestDto
            {
                IdAsistente = 1,
                IdPrompt = 99,
                Mensaje = "Consulta de prueba"
            }));

        _aiProviderMock.Verify(
            provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private PruebaPromptService CrearServicio()
    {
        return new PruebaPromptService(
            _asistenteServiceMock.Object,
            _promptSistemaServiceMock.Object,
            _promptBuilderMock.Object,
            _aiProviderMock.Object,
            _validator);
    }

    private static AsistenteDto CrearAsistente()
    {
        return new AsistenteDto
        {
            IdAsistente = 1,
            Nombre = "Asistente Empresarial",
            Descripcion = "Asistente de prueba.",
            ModeloIA = "deepseek-r1:7b",
            Idioma = "Español",
            LongitudRespuesta = "Breve y clara",
            Formalidad = "Profesional",
            FormatoRespuesta = "Texto claro.",
            Restricciones = "No inventar datos.",
            MensajeBienvenida = "Hola.",
            Temperatura = 0.4m,
            MaxTokens = 512,
            TimeoutSeconds = 120,
            Activo = true
        };
    }

    private static PromptSistemaDto CrearPrompt()
    {
        return new PromptSistemaDto
        {
            IdPrompt = 1,
            IdAsistente = 1,
            Nombre = "Prompt de prueba",
            Contenido = "Responde en español.",
            Version = 1,
            Activo = true
        };
    }
}