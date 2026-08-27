using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class ResumenConversacionServiceTests
{
    private readonly Mock<IAIProvider> _aiProviderMock = new();
    private readonly Mock<IConversacionRepository> _repositoryMock = new();

    [Fact]
    public async Task ActualizarSiEsNecesarioAsync_No_Debe_Resumir_Si_Contexto_Aun_Entra()
    {
        var conversacion = CrearConversacionConMensajes(2);
        var service = CrearServicio();

        await service.ActualizarSiEsNecesarioAsync(
            conversacion,
            CrearAsistente(),
            CrearConfiguracion(maximoMensajesContexto: 2));

        Assert.Null(conversacion.ResumenContexto);
        Assert.Equal(0, conversacion.TotalMensajesResumidos);

        _aiProviderMock.Verify(
            provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ActualizarSiEsNecesarioAsync_Debe_Generar_Resumen_De_Mensajes_Antiguos()
    {
        var conversacion = CrearConversacionConMensajes(3);

        _aiProviderMock
            .Setup(provider => provider.SendAsync(
                It.Is<ChatRequestDto>(solicitud =>
                    solicitud.Mensajes.Count == 2 &&
                    solicitud.Mensajes.Last().Contenido ==
                        "Mensaje 1 de prueba."),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponseDto
            {
                Contenido =
                    "Resumen generado de la conversación.",
                TiempoRespuestaMs = 120
            });

        _repositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CrearServicio();

        await service.ActualizarSiEsNecesarioAsync(
            conversacion,
            CrearAsistente(),
            CrearConfiguracion(maximoMensajesContexto: 2));

        Assert.Equal(
            "Resumen generado de la conversación.",
            conversacion.ResumenContexto);

        Assert.Equal(1, conversacion.TotalMensajesResumidos);

        _aiProviderMock.Verify(
            provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private ResumenConversacionService CrearServicio()
    {
        return new ResumenConversacionService(
            _aiProviderMock.Object,
            _repositoryMock.Object);
    }

    private static Conversacion CrearConversacionConMensajes(
        int cantidadMensajes)
    {
        var conversacion = new Conversacion(1, 99);

        for (var numero = 1; numero <= cantidadMensajes; numero++)
        {
            var rol = numero % 2 == 0
                ? RolMensaje.Asistente
                : RolMensaje.Usuario;

            conversacion.AgregarMensaje(new Mensaje(
                rol,
                $"Mensaje {numero} de prueba."));
        }

        return conversacion;
    }

    private static AsistenteDto CrearAsistente()
    {
        return new AsistenteDto
        {
            IdAsistente = 1,
            Nombre = "Asistente de prueba",
            ModeloIA = "deepseek-r1:7b",
            Temperatura = 0.5m,
            MaxTokens = 512,
            TimeoutSeconds = 120
        };
    }

    private static ConfiguracionMemoriaDto CrearConfiguracion(
        int maximoMensajesContexto)
    {
        return new ConfiguracionMemoriaDto
        {
            IdConfiguracion = 1,
            MaximoMensajesContexto = maximoMensajesContexto,
            MaximoTokensContexto = 3000,
            LongitudResumen = 800,
            CantidadConversacionesVisibles = 20,
            Activo = true
        };
    }
}