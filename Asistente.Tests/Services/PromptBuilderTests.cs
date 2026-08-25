using Asistente.Application.DTOs;
using Asistente.Application.Services;
using Xunit;

namespace Asistente.Tests.Services;

public class PromptBuilderTests
{
    [Fact]
    public void ConstruirPromptSistema_Debe_Incluir_Configuracion_Y_Contenido_Del_Prompt()
    {
        var asistente = new AsistenteDto
        {
            IdAsistente = 1,
            Nombre = "Asistente Empresarial",
            Descripcion = "Asistente para consultas empresariales.",
            ModeloIA = "deepseek-r1:7b",
            Idioma = "Español",
            LongitudRespuesta = "Breve y clara",
            Formalidad = "Profesional",
            FormatoRespuesta = "Usa viñetas cuando sean necesarias.",
            Restricciones = "No inventes datos.",
            MensajeBienvenida = "Hola.",
            Temperatura = 0.4m,
            MaxTokens = 512,
            TimeoutSeconds = 120,
            Activo = true
        };

        var prompt = new PromptSistemaDto
        {
            IdPrompt = 1,
            IdAsistente = 1,
            Nombre = "Prompt de prueba",
            Contenido = "Responde únicamente en español.",
            Version = 2,
            Activo = true
        };

        var builder = new PromptBuilder();

        var resultado = builder.ConstruirPromptSistema(
            asistente,
            prompt);

        Assert.Contains(
            "Asistente configurado: Asistente Empresarial",
            resultado);

        Assert.Contains(
            "Idioma de respuesta: Español",
            resultado);

        Assert.Contains(
            "Nivel de formalidad: Profesional",
            resultado);

        Assert.Contains(
            "No inventes datos.",
            resultado);

        Assert.Contains(
            "Responde únicamente en español.",
            resultado);
    }

    [Fact]
    public void ConstruirSolicitudChat_Debe_Agregar_Mensaje_De_Sistema_Y_Configuracion_Modelo()
    {
        var asistente = new AsistenteDto
        {
            IdAsistente = 1,
            Nombre = "Asistente Empresarial",
            Descripcion = "Asistente para consultas empresariales.",
            ModeloIA = "deepseek-r1:7b",
            Idioma = "Español",
            LongitudRespuesta = "Breve y clara",
            Formalidad = "Profesional",
            FormatoRespuesta = "Texto claro.",
            Restricciones = "No inventes datos.",
            MensajeBienvenida = "Hola.",
            Temperatura = 0.4m,
            MaxTokens = 512,
            TimeoutSeconds = 120,
            Activo = true
        };

        var prompt = new PromptSistemaDto
        {
            IdPrompt = 1,
            IdAsistente = 1,
            Nombre = "Prompt de prueba",
            Contenido = "Responde en español.",
            Version = 1,
            Activo = true
        };

        var mensajes = new[]
        {
            new MensajeDto
            {
                Rol = "Usuario",
                Contenido = "¿Qué es un inventario?",
                FechaHora = DateTime.UtcNow
            }
        };

        var builder = new PromptBuilder();

        var solicitud = builder.ConstruirSolicitudChat(
            asistente,
            prompt,
            mensajes);

        Assert.Equal("deepseek-r1:7b", solicitud.ModeloIA);
        Assert.Equal(0.4m, solicitud.Temperatura);
        Assert.Equal(512, solicitud.MaxTokens);
        Assert.Equal(120, solicitud.TimeoutSeconds);
        Assert.Equal(2, solicitud.Mensajes.Count);

        var mensajeSistema = solicitud.Mensajes.First();

        Assert.Equal("system", mensajeSistema.Rol);
        Assert.Contains(
            "Responde en español.",
            mensajeSistema.Contenido);

        Assert.Equal(
            "¿Qué es un inventario?",
            solicitud.Mensajes.Last().Contenido);
    }
}