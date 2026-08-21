using Asistente.Web.Models;

namespace Asistente.Web.Services;

public interface IAutenticacionApiClient
{
    Task<ResultadoLoginApi> IniciarSesionAsync(
        string usuario,
        string password,
        CancellationToken cancellationToken = default);

    Task CerrarSesionAsync(
        string cookie,
        CancellationToken cancellationToken = default);
}