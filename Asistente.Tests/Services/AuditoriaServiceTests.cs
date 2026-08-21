using Asistente.Application.DTOs;
using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Moq;

namespace Asistente.Tests.Services;

public class AuditoriaServiceTests
{
    private readonly Mock<IAuditoriaRepository> _auditoriaRepository = new();

    private AuditoriaService CrearServicio()
    {
        return new AuditoriaService(_auditoriaRepository.Object);
    }

    private static ContextoClienteDto CrearContexto()
    {
        return new ContextoClienteDto
        {
            DireccionIP = "127.0.0.1",
            Navegador = "Pruebas unitarias"
        };
    }

    [Fact]
    public async Task ListarSesionesAsync_Debe_Devolver_Sesiones_Y_Auditar_Consulta()
    {
        var sesiones = new List<AuditoriaSesion>
        {
            new(1, "127.0.0.1", "Navegador de prueba")
        };

        _auditoriaRepository
            .Setup(repository => repository.ListarSesionesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sesiones);

        var service = CrearServicio();

        var resultado = await service.ListarSesionesAsync(
            1,
            CrearContexto());

        Assert.Single(resultado);
        Assert.Equal("Activa", resultado.First().Estado);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "ConsultarSesiones"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ListarActividadesAsync_Debe_Devolver_Actividades_Y_Auditar_Consulta()
    {
        var actividades = new List<AuditoriaActividad>
        {
            new(
                1,
                "Seguridad",
                "InicioSesion",
                "Inicio correcto.",
                "127.0.0.1")
        };

        _auditoriaRepository
            .Setup(repository => repository.ListarActividadesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(actividades);

        var service = CrearServicio();

        var resultado = await service.ListarActividadesAsync(
            1,
            CrearContexto());

        Assert.Single(resultado);
        Assert.Equal("InicioSesion", resultado.First().Accion);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "ConsultarActividades"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}