using System.Net;
using System.Text;
using Asistente.Application.DTOs;
using Asistente.Infrastructure.Options;
using Asistente.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Asistente.Tests.Services;

public class OllamaServiceTests
{
    [Fact]
    public async Task SendAsync_Debe_Devolver_Respuesta_Cuando_Ollama_Responde_Correctamente()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request =>
                    request.Method == HttpMethod.Post &&
                    request.RequestUri!.AbsolutePath == "/api/chat"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "message": {
                        "role": "assistant",
                        "content": "Respuesta simulada de Ollama."
                      }
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            });

        using var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:11434/")
        };

        var service = CrearServicio(httpClient);

        var response = await service.SendAsync(new ChatRequestDto
        {
            Mensajes =
            [
                new MensajeDto
                {
                    Rol = "Usuario",
                    Contenido = "Hola"
                }
            ]
        });

        Assert.Equal(
            "Respuesta simulada de Ollama.",
            response.Contenido);

        Assert.True(response.TiempoRespuestaMs >= 0);
    }

    [Fact]
    public async Task SendAsync_Debe_Lanzar_Error_Cuando_Ollama_No_Esta_Disponible()
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(
                HttpStatusCode.ServiceUnavailable));

        using var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:11434/")
        };

        var service = CrearServicio(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.SendAsync(new ChatRequestDto
            {
                Mensajes =
                [
                    new MensajeDto
                    {
                        Rol = "Usuario",
                        Contenido = "Consulta de prueba"
                    }
                ]
            }));
    }

    private static OllamaService CrearServicio(HttpClient httpClient)
    {
        var options = Options.Create(new OllamaOptions
        {
            BaseUrl = "http://localhost:11434",
            Model = "deepseek-r1:7b",
            TimeoutSeconds = 120,
            KeepAlive = "0"
        });

        var logger = new Mock<ILogger<OllamaService>>();

        return new OllamaService(
            httpClient,
            options,
            logger.Object);
    }
}