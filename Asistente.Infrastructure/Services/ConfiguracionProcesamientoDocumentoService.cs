using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Infrastructure.Options;
using Microsoft.Extensions.Options;


namespace Asistente.Infrastructure.Services;

public class ConfiguracionProcesamientoDocumentoService
    : IConfiguracionProcesamientoDocumento
{
    private readonly IOptionsMonitor<ProcesamientoDocumentalOptions>
        _optionsMonitor;

    public ConfiguracionProcesamientoDocumentoService(
        IOptionsMonitor<ProcesamientoDocumentalOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public ConfiguracionProcesamientoDocumentoDto Obtener()
    {
        var options = _optionsMonitor.CurrentValue;

        return new ConfiguracionProcesamientoDocumentoDto
        {
            TamanoMaximoChunk = options.TamanoMaximoChunk,
            SolapamientoChunk = options.SolapamientoChunk,
            LongitudMinimaChunk = options.LongitudMinimaChunk,
            FrecuenciaSegundos = options.FrecuenciaSegundos,
            MaximoDocumentosPorCiclo =
                options.MaximoDocumentosPorCiclo
        };
    }
}