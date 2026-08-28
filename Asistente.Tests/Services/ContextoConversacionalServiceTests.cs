using Asistente.Application.DTOs;
using Asistente.Application.Services;
using Xunit;

namespace Asistente.Tests.Services;

public class ContextoConversacionalServiceTests
{
    [Fact]
    public void Construir_Debe_Conservar_Solo_Los_Mensajes_Mas_Recientes()
    {
        var fechaBase = new DateTime(2026, 8, 28, 10, 0, 0);

        var mensajes = new[]
        {
            CrearMensaje("Mensaje 1", fechaBase.AddMinutes(1)),
            CrearMensaje("Mensaje 2", fechaBase.AddMinutes(2)),
            CrearMensaje("Mensaje 3", fechaBase.AddMinutes(3)),
            CrearMensaje("Mensaje 4", fechaBase.AddMinutes(4))
        };

        var configuracion = CrearConfiguracion(
            maximoMensajes: 2,
            maximoTokens: 1000);

        var service = new ContextoConversacionalService();

        var resultado = service.Construir(
            mensajes,
            null,
            configuracion);

        Assert.Equal(2, resultado.Mensajes.Count);
        Assert.Equal("Mensaje 3", resultado.Mensajes.First().Contenido);
        Assert.Equal("Mensaje 4", resultado.Mensajes.Last().Contenido);
        Assert.Equal(0, resultado.TokensResumen);
        Assert.True(resultado.TokensMensajes > 0);
    }

    [Fact]
    public void Construir_Debe_Reservar_Tokens_Para_El_Resumen()
    {
        var fechaBase = new DateTime(2026, 8, 28, 10, 0, 0);

        var mensajes = new[]
        {
            CrearMensaje("abcdefgh", fechaBase.AddMinutes(1)),
            CrearMensaje("ijklmnop", fechaBase.AddMinutes(2))
        };

        var configuracion = CrearConfiguracion(
            maximoMensajes: 10,
            maximoTokens: 4);

        var service = new ContextoConversacionalService();

        var resultado = service.Construir(
            mensajes,
            "resumen",
            configuracion);

        Assert.Equal(2, resultado.TokensResumen);
        Assert.Equal(2, resultado.TokensMensajes);
        Assert.Equal(4, resultado.TokensEstimados);
        Assert.Single(resultado.Mensajes);
        Assert.Equal("ijklmnop", resultado.Mensajes.Single().Contenido);
    }

    private static MensajeDto CrearMensaje(
        string contenido,
        DateTime fechaHora)
    {
        return new MensajeDto
        {
            Rol = "Usuario",
            Contenido = contenido,
            FechaHora = fechaHora
        };
    }

    private static ConfiguracionMemoriaDto CrearConfiguracion(
        int maximoMensajes,
        int maximoTokens)
    {
        return new ConfiguracionMemoriaDto
        {
            IdConfiguracion = 1,
            MaximoMensajesContexto = maximoMensajes,
            MaximoTokensContexto = maximoTokens,
            LongitudResumen = 800,
            CantidadConversacionesVisibles = 20,
            Activo = true
        };
    }
}