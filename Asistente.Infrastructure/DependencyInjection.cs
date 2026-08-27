//DependencyInjection.cs
using Asistente.Application.Interfaces;
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

        services.Configure<UsuarioInicialOptions>(
            configuration.GetSection(UsuarioInicialOptions.SectionName));

        services.AddScoped<InicializadorSeguridad>();

        services.AddScoped<IConversacionRepository, ConversacionRepository>();
        services.AddScoped<IConfiguracionMemoriaRepository,ConfiguracionMemoriaRepository>();
        //
        services.AddScoped<IAsistenteRepository, AsistenteRepository>();
        services.AddScoped<IPromptSistemaRepository, PromptSistemaRepository>();
        services.AddScoped<IHistorialPromptRepository, HistorialPromptRepository>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

        services.AddScoped<IPasswordService, PasswordService>();

        services.AddHttpClient<IAIProvider, OllamaService>(
            (serviceProvider, httpClient) =>
            {
                var ollamaOptions = serviceProvider
                    .GetRequiredService<IOptions<OllamaOptions>>()
                    .Value;

                httpClient.BaseAddress = new Uri(
                    ollamaOptions.BaseUrl.TrimEnd('/') + "/");

                httpClient.Timeout = Timeout.InfiniteTimeSpan;
            });

        return services;
    }
}