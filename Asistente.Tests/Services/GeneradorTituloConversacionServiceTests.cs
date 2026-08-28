using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class GeneradorTituloConversacionServiceTests
{
    private readonly Mock<IAIProvider> _aiProviderMock = new();
    private readonly Mock<IConversacionRepository>
        _conversacionRepositoryMock = new();

    [Fact]
    public async Task GenerarSiEsNecesarioAsync_Debe_Generar_Y_Guardar_Titulo_Con_IA()
    {
        var conversacion = CrearConversacionSinTitulo();

        _aiProviderMock
            .Setup(provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponseDto
            {
                Contenido = "Título: Reforma Agraria: Cambios y Mejoras",
                TiempoRespuestaMs = 100
            });

        var service = CrearServicio();

        await service.GenerarSiEsNecesarioAsync(
            conversacion,
            CrearAsistente());

        Assert.Equal(
            "Reforma Agraria: Cambios y Mejoras",
            conversacion.Titulo);

        _aiProviderMock.Verify(
            provider => provider.SendAsync(
                It.Is<ChatRequestDto>(solicitud =>
                    solicitud.Mensajes.Count == 2 &&
                    solicitud.Mensajes.First().Rol == "system" &&
                    solicitud.Mensajes.Last().Rol == "user"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _conversacionRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerarSiEsNecesarioAsync_Debe_Usar_Titulo_De_Respaldo_Si_IA_Falla()
    {
        var conversacion = CrearConversacionSinTitulo();

        _aiProviderMock
            .Setup(provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(
                "Ollama no está disponible."));

        var service = CrearServicio();

        await service.GenerarSiEsNecesarioAsync(
            conversacion,
            CrearAsistente());

        Assert.Equal(
            "Recomendaciones para ordenar el inventario de mi almacén",
            conversacion.Titulo);

        _conversacionRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerarSiEsNecesarioAsync_No_Debe_Generar_Si_Ya_Existe_Titulo()
    {
        var conversacion = CrearConversacionSinTitulo();

        conversacion.Renombrar("Título existente");

        var service = CrearServicio();

        await service.GenerarSiEsNecesarioAsync(
            conversacion,
            CrearAsistente());

        _aiProviderMock.Verify(
            provider => provider.SendAsync(
                It.IsAny<ChatRequestDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _conversacionRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GeneradorTituloConversacionService CrearServicio()
    {
        return new GeneradorTituloConversacionService(
            _aiProviderMock.Object,
            _conversacionRepositoryMock.Object);
    }

    private static Conversacion CrearConversacionSinTitulo()
    {
        var conversacion = new Conversacion();

        conversacion.AgregarMensaje(new Mensaje(
            RolMensaje.Usuario,
            "Necesito recomendaciones para ordenar el inventario de mi almacén."));

        conversacion.AgregarMensaje(new Mensaje(
            RolMensaje.Asistente,
            "Puedo ayudarte a organizar el inventario."));

        return conversacion;
    }

    private static AsistenteDto CrearAsistente()
    {
        return new AsistenteDto
        {
            IdAsistente = 1,
            Nombre = "Asistente de prueba",
            ModeloIA = "deepseek-r1:7b",
            Idioma = "Español",
            Temperatura = 0.4m,
            MaxTokens = 512,
            TimeoutSeconds = 120,
            Activo = true
        };
    }
}