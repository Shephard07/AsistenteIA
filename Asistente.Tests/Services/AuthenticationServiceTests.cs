using Asistente.Application.DTOs;
using Asistente.Application.Services;
using Asistente.Application.Validators;
using Asistente.Domain.Entities;
using Asistente.Domain.Enums;
using Asistente.Domain.Interfaces;
using Asistente.Application.Interfaces;
using Moq;

namespace Asistente.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly Mock<IAuditoriaRepository> _auditoriaRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    private AuthenticationService CrearServicio()
    {
        return new AuthenticationService(
            _usuarioRepository.Object,
            _auditoriaRepository.Object,
            _passwordService.Object,
            new LoginRequestValidator());
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
    public async Task IniciarSesionAsync_Debe_Registrar_Sesion_Cuando_Las_Credenciales_Son_Validas()
    {
        var usuario = new Usuario(
            "usuario1",
            "Usuario",
            "Prueba",
            "usuario1@correo.com",
            "hash-seguro");

        _usuarioRepository
            .Setup(repository =>
                repository.ObtenerPorNombreUsuarioConRolesAsync(
                    "usuario1",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _passwordService
            .Setup(service =>
                service.Verificar("hash-seguro", "Clave123*"))
            .Returns(true);

        var service = CrearServicio();

        var respuesta = await service.IniciarSesionAsync(
            new LoginRequestDto
            {
                Usuario = "usuario1",
                Password = "Clave123*"
            },
            CrearContexto());

        Assert.Equal("usuario1", respuesta.Usuario);
        Assert.NotNull(usuario.FechaUltimoAcceso);

        _auditoriaRepository.Verify(
            repository => repository.AgregarSesionAsync(
                It.IsAny<AuditoriaSesion>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "InicioSesion"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IniciarSesionAsync_Debe_Registrar_Auditoria_Cuando_El_Usuario_No_Existe()
    {
        _usuarioRepository
            .Setup(repository =>
                repository.ObtenerPorNombreUsuarioConRolesAsync(
                    "desconocido",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        var service = CrearServicio();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.IniciarSesionAsync(
                new LoginRequestDto
                {
                    Usuario = "desconocido",
                    Password = "Clave123*"
                },
                CrearContexto()));

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "InicioSesionFallido" &&
                        actividad.IdUsuario == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IniciarSesionAsync_Debe_Rechazar_Usuario_Inactivo_Y_Auditar()
    {
        var usuario = new Usuario(
            "inactivo",
            "Usuario",
            "Inactivo",
            "inactivo@correo.com",
            "hash-seguro");

        usuario.Desactivar();

        _usuarioRepository
            .Setup(repository =>
                repository.ObtenerPorNombreUsuarioConRolesAsync(
                    "inactivo",
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var service = CrearServicio();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.IniciarSesionAsync(
                new LoginRequestDto
                {
                    Usuario = "inactivo",
                    Password = "Clave123*"
                },
                CrearContexto()));

        _passwordService.Verify(
            service => service.Verificar(
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "InicioSesionFallido" &&
                        actividad.IdUsuario == usuario.IdUsuario),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CerrarSesionAsync_Debe_Cerrar_La_Sesion_Y_Registrar_Actividad()
    {
        var sesion = new AuditoriaSesion(
            1,
            "127.0.0.1",
            "Pruebas unitarias");

        _auditoriaRepository
            .Setup(repository =>
                repository.ObtenerSesionActivaPorUsuarioAsync(
                    1,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(sesion);

        var service = CrearServicio();

        await service.CerrarSesionAsync(
            1,
            CrearContexto());

        Assert.Equal(EstadoSesion.Cerrada, sesion.Estado);
        Assert.NotNull(sesion.FechaFin);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "CierreSesion"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepository.Verify(
            repository => repository.GuardarCambiosAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}