using System.Net.Http.Json;
using Asistente.Web.Models;

namespace Asistente.Web.Services;

public class AutenticacionApiClient : IAutenticacionApiClient
{
    private readonly HttpClient _httpClient;

    public AutenticacionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ResultadoLoginApi> IniciarSesionAsync(
        string usuario,
        string password,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/autenticacion/iniciar-sesion",
            new
            {
                Usuario = usuario,
                Password = password
            },
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var respuesta = await response.Content
                .ReadFromJsonAsync<AutenticacionApiResponse>(
                    cancellationToken: cancellationToken);

            return new ResultadoLoginApi
            {
                Exito = respuesta is not null,
                Respuesta = respuesta,
                MensajeError = respuesta is null
                    ? "La API no devolvió una respuesta válida."
                    : string.Empty
            };
        }

        var error = await response.Content
            .ReadFromJsonAsync<ErrorApiResponse>(
                cancellationToken: cancellationToken);

        return new ResultadoLoginApi
        {
            Exito = false,
            MensajeError = string.IsNullOrWhiteSpace(error?.Mensaje)
                ? "No fue posible iniciar sesión."
                : error.Mensaje
        };
    }

    public async Task CerrarSesionAsync(
        string cookie,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "api/autenticacion/cerrar-sesion");

        if (!string.IsNullOrWhiteSpace(cookie))
        {
            request.Headers.TryAddWithoutValidation(
                "Cookie",
                cookie);
        }

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        // Aunque la API no esté disponible, la web eliminará
        // su cookie local para no dejar una sesión visible.
    }
}