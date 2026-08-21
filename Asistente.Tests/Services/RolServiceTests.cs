using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Application.Validators;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Moq;

namespace Asistente.Tests.Services;

public class RolServiceTests
{
    private readonly Mock<IRolRepository> _rolRepository = new();
    private readonly Mock<IAuditoriaRepository> _auditoriaRepository = new();

    private RolService CrearServicio()
    {
        return new RolService(
            _rolRepository.Object,
            _auditoriaRepository.Object,
            new CrearRolRequestValidator(),
            new ActualizarRolRequestValidator());
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
    public async Task ListarAsync_Debe_Devolver_Los_Roles_Mapeados()
    {
        var roles = new List<Rol>
        {
            new("Administrador", "Acceso completo."),
            new("Operador", "Acceso al asistente.")
        };

        _rolRepository
            .Setup(repository => repository.ListarAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var service = CrearServicio();

        var resultado = await service.ListarAsync();

        Assert.Equal(2, resultado.Count);
        Assert.Contains(resultado, rol => rol.Nombre == "Administrador");
    }

    [Fact]
    public async Task CrearAsync_Debe_Crear_Rol_Y_Registrar_Auditoria()
    {
        _rolRepository
            .Setup(repository => repository.ExisteNombreAsync(
                "Analista",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CrearServicio();

        var resultado = await service.CrearAsync(
            new CrearRolRequestDto
            {
                Nombre = "Analista",
                Descripcion = "Consulta información."
            },
            1,
            CrearContexto());

        Assert.Equal("Analista", resultado.Nombre);
        Assert.True(resultado.Activo);

        _rolRepository.Verify(
            repository => repository.AgregarAsync(
                It.Is<Rol>(rol => rol.Nombre == "Analista"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "CrearRol"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_Debe_Modificar_Rol_Y_Auditar()
    {
        var rol = new Rol("Analista", "Descripción inicial.");

        _rolRepository
            .Setup(repository => repository.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rol);

        _rolRepository
            .Setup(repository => repository.ExisteNombreAsync(
                "Analista Senior",
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CrearServicio();

        var resultado = await service.ActualizarAsync(
            1,
            new ActualizarRolRequestDto
            {
                Nombre = "Analista Senior",
                Descripcion = "Descripción actualizada."
            },
            1,
            CrearContexto());

        Assert.Equal("Analista Senior", resultado.Nombre);
        Assert.Equal("Descripción actualizada.", resultado.Descripcion);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "ActualizarRol"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoAsync_Debe_Desactivar_Rol_Y_Auditar()
    {
        var rol = new Rol("Analista", "Consulta información.");

        _rolRepository
            .Setup(repository => repository.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rol);

        var service = CrearServicio();

        await service.CambiarEstadoAsync(
            1,
            false,
            1,
            CrearContexto());

        Assert.False(rol.Activo);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "DesactivarRol"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}