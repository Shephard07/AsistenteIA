using Asistente.Application.Interfaces;
using Asistente.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asistente.Infrastructure.Services;

public class ProcesamientoDocumentosBackgroundService
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly IOptionsMonitor<ProcesamientoDocumentalOptions>
        _optionsMonitor;

    private readonly ILogger<
        ProcesamientoDocumentosBackgroundService> _logger;

    public ProcesamientoDocumentosBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<ProcesamientoDocumentalOptions> optionsMonitor,
        ILogger<ProcesamientoDocumentosBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "El servicio de procesamiento documental se inició.");

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

            var procesador = scope.ServiceProvider
                .GetRequiredService<IProcesadorDocumentoService>();

            var totalProcesados = await procesador
                .ProcesarPendientesAsync(cancellationToken);

            if (totalProcesados > 0)
            {
                _logger.LogInformation(
                    "Se procesaron {TotalProcesados} documento(s).",
                    totalProcesados);
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
                "Ocurrió un error en el ciclo de procesamiento documental.");
        }
    }
}