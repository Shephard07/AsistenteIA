using Asistente.Application.DTOs;
using Asistente.Application.Services;
using Asistente.Application.Validators;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class PromptSistemaServiceTests
{
    private readonly Mock<IAsistenteRepository> _asistenteRepositoryMock = new();
    private readonly Mock<IPromptSistemaRepository> _promptRepositoryMock = new();
    private readonly Mock<IHistorialPromptRepository> _historialRepositoryMock = new();
    private readonly Mock<IAuditoriaRepository> _auditoriaRepositoryMock = new();

    [Fact]
    public async Task CrearNuevaVersionAsync_Debe_Desactivar_Anterior_Y_Registrar_Historial()
    {
        var promptOrigen = new PromptSistema(
            1,
            "Prompt empresarial inicial",
            "Contenido de la versión uno.",
            1,
            true,
            "1");

        PromptSistema? nuevaVersionRegistrada = null;

        _promptRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(promptOrigen);

        _promptRepositoryMock
            .Setup(repository => repository.ObtenerActivoPorAsistenteAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(promptOrigen);

        _promptRepositoryMock
            .Setup(repository => repository.ObtenerUltimaVersionAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _promptRepositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<PromptSistema>(),
                It.IsAny<CancellationToken>()))
            .Callback<PromptSistema, CancellationToken>(
                (prompt, _) => nuevaVersionRegistrada = prompt)
            .Returns(Task.CompletedTask);

        _promptRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _historialRepositoryMock
            .Setup(repository => repository.AgregarAsync(
                It.IsAny<HistorialPrompt>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _historialRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditoriaRepositoryMock
            .Setup(repository => repository.AgregarActividadAsync(
                It.IsAny<AuditoriaActividad>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _auditoriaRepositoryMock
            .Setup(repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CrearServicio();

        var response = await service.CrearNuevaVersionAsync(
            1,
            new CrearVersionPromptRequestDto
            {
                Nombre = "Prompt empresarial refinado",
                Contenido = "Contenido de la versión dos.",
                MotivoCambio = "Se reforzó la respuesta en español."
            },
            1,
            CrearContexto());

        Assert.False(promptOrigen.Activo);

        Assert.NotNull(nuevaVersionRegistrada);
        Assert.True(nuevaVersionRegistrada.Activo);
        Assert.Equal(2, nuevaVersionRegistrada.Version);
        Assert.Equal(
            "Contenido de la versión dos.",
            nuevaVersionRegistrada.Contenido);

        Assert.True(response.Activo);
        Assert.Equal(2, response.Version);

        _promptRepositoryMock.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _historialRepositoryMock.Verify(
            repository => repository.AgregarAsync(
                It.Is<HistorialPrompt>(
                    historial =>
                        historial.Version == 2 &&
                        historial.MotivoCambio ==
                            "Se reforzó la respuesta en español."),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepositoryMock.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Modulo == "Prompts" &&
                        actividad.Accion == "CrearVersionPrompt"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CrearNuevaVersionAsync_Debe_Lanzar_Error_Si_No_Existe_Prompt_Origen()
    {
        _promptRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(
                99,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PromptSistema?)null);

        var service = CrearServicio();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CrearNuevaVersionAsync(
                99,
                new CrearVersionPromptRequestDto
                {
                    Nombre = "Versión inexistente",
                    Contenido = "Contenido de prueba.",
                    MotivoCambio = "Prueba."
                },
                1,
                CrearContexto()));

        _promptRepositoryMock.Verify(
            repository => repository.AgregarAsync(
                It.IsAny<PromptSistema>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private PromptSistemaService CrearServicio()
    {
        return new PromptSistemaService(
            _asistenteRepositoryMock.Object,
            _promptRepositoryMock.Object,
            _historialRepositoryMock.Object,
            _auditoriaRepositoryMock.Object,
            new CrearPromptSistemaRequestValidator(),
            new CrearVersionPromptRequestValidator());
    }

    private static ContextoClienteDto CrearContexto()
    {
        return new ContextoClienteDto
        {
            DireccionIP = "127.0.0.1",
            Navegador = "Prueba unitaria"
        };
    }
}