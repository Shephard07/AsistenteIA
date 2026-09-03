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
    "CONTEXTO DOCUMENTAL RECUPERADO");

        contenido.AppendLine(
            "INSTRUCCIONES OBLIGATORIAS PARA LA RESPUESTA:");

        contenido.AppendLine(
            "1. Usa los fragmentos siguientes como fuente para responder " +
            "cuando sean relevantes para la pregunta.");

        contenido.AppendLine(
            "2. Cada afirmación basada en un fragmento debe incluir al final " +
            "la cita exacta: [Documento #ID, páginas X-Y].");

        contenido.AppendLine(
            "3. Al final de la respuesta agrega una sección titulada " +
            "\"Fuentes consultadas\" e incluye las citas de los documentos " +
            "que utilizaste.");

        contenido.AppendLine(
            "4. No inventes datos ni atribuyas al documento información que " +
            "no aparezca en los fragmentos.");

        contenido.AppendLine(
            "5. Si los fragmentos no contienen la respuesta, indícalo " +
            "claramente y no incluyas una fuente documental.");

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