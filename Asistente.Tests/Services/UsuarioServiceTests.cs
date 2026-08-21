using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Application.Validators;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Moq;

namespace Asistente.Tests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly Mock<IRolRepository> _rolRepository = new();
    private readonly Mock<IAuditoriaRepository> _auditoriaRepository = new();
    private readonly Mock<IPasswordService> _passwordService = new();

    private UsuarioService CrearServicio()
    {
        return new UsuarioService(
            _usuarioRepository.Object,
            _rolRepository.Object,
            _auditoriaRepository.Object,
            _passwordService.Object,
            new CrearUsuarioRequestValidator(),
            new ActualizarUsuarioRequestValidator(),
            new AsignarRolesUsuarioRequestValidator(),
            new CambiarPasswordRequestValidator());
    }

    private static ContextoClienteDto CrearContexto()
    {
        return new ContextoClienteDto
        {
            DireccionIP = "127.0.0.1",
            Navegador = "Pruebas unitarias"
        };
    }

    private void ConfigurarDatosUnicos()
    {
        _usuarioRepository
            .Setup(repository => repository.ExisteNombreUsuarioAsync(
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _usuarioRepository
            .Setup(repository => repository.ExisteCorreoAsync(
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    [Fact]
    public async Task ListarAsync_Debe_Devolver_Usuarios_Mapeados()
    {
        var usuarios = new List<Usuario>
        {
            new(
                "usuario1",
                "Usuario",
                "Prueba",
                "usuario1@correo.com",
                "hash")
        };

        _usuarioRepository
            .Setup(repository => repository.ListarAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarios);

        var service = CrearServicio();

        var resultado = await service.ListarAsync();

        Assert.Single(resultado);
        Assert.Equal("usuario1", resultado.First().Usuario);
    }

    [Fact]
    public async Task CrearAsync_Debe_Crear_Usuario_Con_Rol_Y_Auditar()
    {
        ConfigurarDatosUnicos();

        var rolOperador = new Rol(
            "Operador",
            "Acceso al asistente.");

        var usuarioResultado = new Usuario(
            "operador2",
            "Olivia",
            "Operadora",
            "operador2@correo.com",
            "hash-seguro");

        _passwordService
            .Setup(service => service.GenerarHash("Operador123*"))
            .Returns("hash-seguro");

        _rolRepository
            .Setup(repository => repository.ObtenerPorIdAsync(
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolOperador);

        _usuarioRepository
            .Setup(repository => repository.ObtenerConRolesPorIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioResultado);

        var service = CrearServicio();

        var resultado = await service.CrearAsync(
            new CrearUsuarioRequestDto
            {
                Usuario = "operador2",
                Nombres = "Olivia",
                Apellidos = "Operadora",
                Correo = "operador2@correo.com",
                Password = "Operador123*",
                IdsRoles = [2]
            },
            1,
            CrearContexto());

        Assert.Equal("operador2", resultado.Usuario);

        _usuarioRepository.Verify(
            repository => repository.AgregarAsync(
                It.Is<Usuario>(
                    usuario => usuario.NombreUsuario == "operador2"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "CrearUsuario"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActualizarAsync_Debe_Modificar_Usuario_Y_Auditar()
    {
        ConfigurarDatosUnicos();

        var usuario = new Usuario(
            "operador1",
            "Olivia",
            "Operadora",
            "operador1@correo.com",
            "hash");

        _usuarioRepository
            .Setup(repository => repository.ObtenerConRolesPorIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var service = CrearServicio();

        var resultado = await service.ActualizarAsync(
            1,
            new ActualizarUsuarioRequestDto
            {
                Usuario = "operador.actualizado",
                Nombres = "Olivia María",
                Apellidos = "Operadora",
                Correo = "olivia@correo.com"
            },
            99,
            CrearContexto());

        Assert.Equal("operador.actualizado", resultado.Usuario);
        Assert.Equal("Olivia María", resultado.Nombres);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "ActualizarUsuario"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AsignarRolesAsync_Debe_Reemplazar_Roles_Y_Auditar()
    {
        var usuario = new Usuario(
            "usuario1",
            "Usuario",
            "Prueba",
            "usuario1@correo.com",
            "hash");

        var rolSupervisor = new Rol(
            "Supervisor",
            "Consulta auditorías.");

        _usuarioRepository
            .Setup(repository => repository.ObtenerConRolesPorIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _rolRepository
            .Setup(repository => repository.ObtenerPorIdAsync(
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(rolSupervisor);

        var service = CrearServicio();

        await service.AsignarRolesAsync(
            1,
            new AsignarRolesUsuarioRequestDto
            {
                IdsRoles = [3]
            },
            99,
            CrearContexto());

        Assert.Single(usuario.UsuarioRoles);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad =>
                        actividad.Accion == "AsignarRolesUsuario"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CambiarPasswordAsync_Debe_Generar_Nuevo_Hash_Y_Auditar()
    {
        var usuario = new Usuario(
            "usuario1",
            "Usuario",
            "Prueba",
            "usuario1@correo.com",
            "hash-anterior");

        _usuarioRepository
            .Setup(repository => repository.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        _passwordService
            .Setup(service => service.GenerarHash("NuevaClave123*"))
            .Returns("hash-nuevo");

        var service = CrearServicio();

        await service.CambiarPasswordAsync(
            1,
            new CambiarPasswordRequestDto
            {
                NuevaPassword = "NuevaClave123*"
            },
            99,
            CrearContexto());

        Assert.Equal("hash-nuevo", usuario.PasswordHash);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "CambiarPassword"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CambiarEstadoAsync_Debe_Desactivar_Usuario_Y_Auditar()
    {
        var usuario = new Usuario(
            "usuario1",
            "Usuario",
            "Prueba",
            "usuario1@correo.com",
            "hash");

        _usuarioRepository
            .Setup(repository => repository.ObtenerPorIdAsync(
                1,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var service = CrearServicio();

        await service.CambiarEstadoAsync(
            1,
            false,
            99,
            CrearContexto());

        Assert.False(usuario.Activo);

        _auditoriaRepository.Verify(
            repository => repository.AgregarActividadAsync(
                It.Is<AuditoriaActividad>(
                    actividad => actividad.Accion == "DesactivarUsuario"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}