//DependencyInjection.cs
using Asistente.Application.Interfaces;
using Asistente.Domain.Interfaces;
using Asistente.Infrastructure.Configuration;
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

            services.AddOptions<ChromaDbOptions>()
        .Bind(configuration.GetSection(
            ChromaDbOptions.SectionName))
        .Validate(
            options => !string.IsNullOrWhiteSpace(
                options.BaseUrl),
            "La URL base de ChromaDB es obligatoria.")
        .Validate(
            options => !string.IsNullOrWhiteSpace(
                options.NombreColeccion),
            "El nombre de la colección de ChromaDB es obligatorio.")
        .Validate(
            options => !string.IsNullOrWhiteSpace(
                options.Tenant),
            "El tenant de ChromaDB es obligatorio.")
        .Validate(
            options => !string.IsNullOrWhiteSpace(
                options.Database),
            "La base de datos de ChromaDB es obligatoria.")
        .ValidateOnStart();

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

        services.AddScoped<ICategoriaDocumentoRepository, CategoriaDocumentoRepository>();
        services.AddScoped<IDocumentoRepository, DocumentoRepository>();

        services.AddScoped<
    IDocumentoProcesadoRepository,
    DocumentoProcesadoRepository>();

        services.AddScoped<
    IProcesadorDocumentoService,
    ProcesadorDocumentoService>();

        services.AddHostedService<ProcesamientoDocumentosBackgroundService>();

        services.AddScoped<IPasswordService, PasswordService>();

        services.Configure<ConfiguracionGestorDocumental>(
    configuration.GetSection(
        ConfiguracionGestorDocumental.Seccion));

        services.AddScoped<
            IAlmacenamientoDocumentoService,
            AlmacenamientoDocumentoService>();

        services.AddScoped<
    IExtractorTextoDocumento,
    ExtractorTextoPdfService>();


        services.AddSingleton<
            IConfiguracionProcesamientoDocumento,
            ConfiguracionProcesamientoDocumentoService>();

        services.AddHttpClient<
    IEmbeddingProvider,
    OllamaEmbeddingProvider>(
    (serviceProvider, httpClient) =>
    {
        var ollamaOptions = serviceProvider
            .GetRequiredService<IOptions<OllamaOptions>>()
            .Value;

        httpClient.BaseAddress = new Uri(
            ollamaOptions.BaseUrl.TrimEnd('/') + "/");

        httpClient.Timeout = Timeout.InfiniteTimeSpan;
    });

        services.AddOptions<ProcesamientoDocumentalOptions>()
    .Bind(configuration.GetSection(
        ProcesamientoDocumentalOptions.SectionName))
    .Validate(
        options => options.TamanoMaximoChunk >= 200,
        "El tamaño máximo del chunk debe ser de al menos 200 caracteres.")
    .Validate(
        options => options.LongitudMinimaChunk > 0 &&
            options.LongitudMinimaChunk <= options.TamanoMaximoChunk,
        "La longitud mínima debe ser mayor que cero y no superar el tamaño máximo.")
    .Validate(
        options => options.SolapamientoChunk >= 0 &&
            options.SolapamientoChunk < options.TamanoMaximoChunk,
        "El solapamiento debe ser mayor o igual a cero y menor que el tamaño máximo.")
    .Validate(
        options => options.FrecuenciaSegundos >= 5,
        "La frecuencia debe ser de al menos 5 segundos.")
    .Validate(
        options => options.MaximoDocumentosPorCiclo > 0,
        "La cantidad máxima de documentos por ciclo debe ser mayor que cero.")
    .ValidateOnStart();

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