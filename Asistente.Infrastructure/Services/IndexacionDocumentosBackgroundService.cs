using Asistente.Application.Interfaces;
using Asistente.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asistente.Infrastructure.Services;

public class IndexacionDocumentosBackgroundService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly IOptionsMonitor<IndexacionDocumentalOptions>
        _optionsMonitor;

    private readonly ILogger<
        IndexacionDocumentosBackgroundService> _logger;

    public IndexacionDocumentosBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<IndexacionDocumentalOptions> optionsMonitor,
        ILogger<IndexacionDocumentosBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "El servicio de indexación documental se inició.");

        await EjecutarCicloAsync(stoppingToken);

        var intervalo = TimeSpan.FromSeconds(
            _optionsMonitor.CurrentValue.FrecuenciaSegundos);

        using var temporizador = new PeriodicTimer(intervalo);

        while (await temporizador.WaitForNextTickAsync(stoppingToken))
        {
            await EjecutarCicloAsync(stoppingToken);
        }
    }

    private async Task EjecutarCicloAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var indexador = scope.ServiceProvider
                .GetRequiredService<IIndexadorDocumentoService>();

            var totalIndexados = await indexador
                .IndexarPendientesAsync(
                    _optionsMonitor.CurrentValue
                        .MaximoDocumentosPorCiclo,
                    cancellationToken);

            if (totalIndexados > 0)
            {
                _logger.LogInformation(
                    "Se indexaron {TotalIndexados} documento(s).",
                    totalIndexados);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // La aplicación se está apagando normalmente.
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Ocurrió un error en el ciclo de indexación documental.");
        }
    }
}