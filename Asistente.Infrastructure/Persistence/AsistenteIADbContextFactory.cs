using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Asistente.Infrastructure.Persistence;

/// <summary>
/// Crea el DbContext para comandos de Entity Framework sin iniciar toda la API.
/// </summary>
public class AsistenteIADbContextFactory
    : IDesignTimeDbContextFactory<AsistenteIADbContext>
{
    public AsistenteIADbContext CreateDbContext(string[] args)
    {
        var rutaActual = Directory.GetCurrentDirectory();

        var posiblesRutasApi = new[]
        {
            Path.Combine(rutaActual, "Asistente.API"),
            rutaActual,
            Path.GetFullPath(
                Path.Combine(rutaActual, "..", "Asistente.API"))
        };

        var rutaApi = posiblesRutasApi.FirstOrDefault(ruta =>
            File.Exists(Path.Combine(ruta, "appsettings.json")))
            ?? throw new InvalidOperationException(
                "No se encontró appsettings.json de Asistente.API.");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(rutaApi)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString =
            configuration.GetConnectionString("AsistenteIA")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'AsistenteIA'.");

        var optionsBuilder =
            new DbContextOptionsBuilder<AsistenteIADbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new AsistenteIADbContext(optionsBuilder.Options);
    }
}