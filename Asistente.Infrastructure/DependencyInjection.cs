using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Options;
using Asistente.Infrastructure.Persistence;
using Asistente.Infrastructure.Repositories;
using Asistente.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asistente.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("AsistenteIA")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'AsistenteIA'.");

        services.AddDbContext<AsistenteIADbContext>(options =>
            options.UseSqlServer(connectionString));

        services.Configure<OllamaOptions>(
            configuration.GetSection(OllamaOptions.SectionName));

        services.AddScoped<IConversacionRepository, ConversacionRepository>();

        services.AddHttpClient<IAsistenteIA, OllamaService>(
            (serviceProvider, httpClient) =>
            {
                var ollamaOptions = serviceProvider
                    .GetRequiredService<IOptions<OllamaOptions>>()
                    .Value;

                httpClient.BaseAddress = new Uri(
                    ollamaOptions.BaseUrl.TrimEnd('/') + "/");

                httpClient.Timeout = TimeSpan.FromSeconds(
                    ollamaOptions.TimeoutSeconds);
            });

        return services;
    }
}