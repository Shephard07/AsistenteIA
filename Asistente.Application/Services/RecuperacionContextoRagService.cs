using System.Text;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Domain.Interfaces;

namespace Asistente.Application.Services;

public class RecuperacionContextoRagService
    : IRecuperacionContextoRagService
{
    private readonly IEmbeddingConfiguracionRepository
        _configuracionRepository;

    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly IVectorStore _vectorStore;

    public RecuperacionContextoRagService(
        IEmbeddingConfiguracionRepository configuracionRepository,
        IEmbeddingProvider embeddingProvider,
        IVectorStore vectorStore)
    {
        _configuracionRepository = configuracionRepository;
        _embeddingProvider = embeddingProvider;
        _vectorStore = vectorStore;
    }

    public async Task<ContextoRagDto> RecuperarAsync(
        string consulta,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(consulta))
        {
            throw new ArgumentException(
                "La consulta para recuperar contexto es obligatoria.",
                nameof(consulta));
        }

        var configuracion = await _configuracionRepository
            .ObtenerActivaAsync(cancellationToken);

        var embeddingConsulta = await _embeddingProvider
            .GenerarAsync(
                consulta,
                configuracion.ModeloEmbeddings,
                cancellationToken);

        var resultados = await _vectorStore.BuscarAsync(
            new BusquedaVectorialRequestDto
            {
                EmbeddingConsulta = embeddingConsulta,
                CantidadResultados = configuracion.CantidadResultados,
                PuntajeMinimo = configuracion.PuntajeMinimo
            },
            cancellationToken);

        return ConstruirContexto(
            resultados,
            configuracion.LongitudMaximaContexto);
    }

    private static ContextoRagDto ConstruirContexto(
        IReadOnlyCollection<ResultadoBusquedaVectorialDto>
            resultados,
        int longitudMaxima)
    {
        if (resultados.Count == 0)
        {
            return new ContextoRagDto();
        }

        var contenido = new StringBuilder();

        contenido.AppendLine(
            "Contexto documental recuperado para responder al usuario.");

        contenido.AppendLine(
            "Usa estos fragmentos solo si son relevantes. " +
            "Si no contienen la respuesta, indícalo con claridad.");

        contenido.AppendLine(
            "Cuando uses información documental, cita la fuente con el " +
            "formato [Documento #ID, páginas X-Y].");

        var fragmentos = new List<FragmentoContextoRagDto>();

        foreach (var resultado in resultados)
        {
            var texto = resultado.Texto.Trim();

            if (string.IsNullOrWhiteSpace(texto))
            {
                continue;
            }

            var encabezado = string.Join(
                Environment.NewLine,
                [
                    string.Empty,
                    $"[Documento #{resultado.IdDocumento}, " +
                    $"versión {resultado.IdVersionDocumento}, " +
                    $"páginas {resultado.PaginaInicial}-" +
                    $"{resultado.PaginaFinal}, " +
                    $"chunk {resultado.NumeroChunk}]"
                ]) + Environment.NewLine;

            var espacioDisponible = longitudMaxima -
                contenido.Length -
                encabezado.Length;

            if (espacioDisponible <= 0)
            {
                break;
            }

            var textoSeleccionado = texto.Length <= espacioDisponible
                ? texto
                : texto[..espacioDisponible];

            contenido.Append(encabezado);
            contenido.AppendLine(textoSeleccionado);

            fragmentos.Add(new FragmentoContextoRagDto
            {
                IdDocumento = resultado.IdDocumento,
                IdVersionDocumento = resultado.IdVersionDocumento,
                NumeroChunk = resultado.NumeroChunk,
                PaginaInicial = resultado.PaginaInicial,
                PaginaFinal = resultado.PaginaFinal,
                Texto = textoSeleccionado,
                Puntaje = resultado.Puntaje
            });

            if (textoSeleccionado.Length < texto.Length)
            {
                break;
            }
        }

        return fragmentos.Count == 0
            ? new ContextoRagDto()
            : new ContextoRagDto
            {
                Contenido = contenido.ToString().Trim(),
                Fragmentos = fragmentos
            };
    }
}