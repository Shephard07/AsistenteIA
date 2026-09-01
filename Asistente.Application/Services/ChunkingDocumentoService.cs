using System.Text;
using System.Text.RegularExpressions;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;

namespace Asistente.Application.Services;

public class ChunkingDocumentoService
    : IChunkingDocumentoService
{
    public IReadOnlyCollection<ChunkTextoDocumentoDto> GenerarChunks(
        IReadOnlyCollection<PaginaTextoDocumentoDto> paginas,
        ConfiguracionProcesamientoDocumentoDto configuracion)
    {
        ArgumentNullException.ThrowIfNull(paginas);
        ArgumentNullException.ThrowIfNull(configuracion);

        ValidarConfiguracion(configuracion);

        if (paginas.Any(pagina => pagina.NumeroPagina <= 0))
        {
            throw new ArgumentException(
                "Todas las páginas deben tener un número válido.",
                nameof(paginas));
        }

        var unidades = paginas
            .Where(pagina => !string.IsNullOrWhiteSpace(pagina.Texto))
            .OrderBy(pagina => pagina.NumeroPagina)
            .SelectMany(pagina => CrearUnidades(
                pagina.NumeroPagina,
                pagina.Texto,
                configuracion.TamanoMaximoChunk))
            .ToArray();

        if (unidades.Length == 0)
        {
            return Array.Empty<ChunkTextoDocumentoDto>();
        }

        var chunks = new List<ChunkTextoDocumentoDto>();
        var textoActual = new StringBuilder();
        var paginaInicial = 0;
        var paginaFinal = 0;

        foreach (var unidad in unidades)
        {
            if (textoActual.Length == 0)
            {
                textoActual.Append(unidad.Texto);
                paginaInicial = unidad.NumeroPagina;
                paginaFinal = unidad.NumeroPagina;
                continue;
            }

            const string separador = "\n\n";

            if (textoActual.Length +
                separador.Length +
                unidad.Texto.Length <=
                configuracion.TamanoMaximoChunk)
            {
                textoActual.Append(separador);
                textoActual.Append(unidad.Texto);
                paginaFinal = unidad.NumeroPagina;
                continue;
            }

            var textoAnterior = textoActual.ToString();
            var paginaFinalAnterior = paginaFinal;

            AgregarChunk(
                chunks,
                textoAnterior,
                paginaInicial,
                paginaFinal);

            textoActual.Clear();

            var solapamiento = ObtenerSolapamiento(
                textoAnterior,
                configuracion.SolapamientoChunk);

            if (!string.IsNullOrWhiteSpace(solapamiento) &&
                solapamiento.Length +
                separador.Length +
                unidad.Texto.Length <=
                configuracion.TamanoMaximoChunk)
            {
                textoActual.Append(solapamiento);
                textoActual.Append(separador);
                textoActual.Append(unidad.Texto);
                paginaInicial = paginaFinalAnterior;
                paginaFinal = unidad.NumeroPagina;
            }
            else
            {
                textoActual.Append(unidad.Texto);
                paginaInicial = unidad.NumeroPagina;
                paginaFinal = unidad.NumeroPagina;
            }
        }

        AgregarChunk(
            chunks,
            textoActual.ToString(),
            paginaInicial,
            paginaFinal);

        return ConsolidarChunksPequenos(
            chunks,
            configuracion);
    }

    private static IEnumerable<UnidadTexto> CrearUnidades(
        int numeroPagina,
        string texto,
        int tamanoMaximoChunk)
    {
        var parrafos = Regex.Split(
            texto.Trim(),
            @"\n\s*\n");

        foreach (var parrafo in parrafos)
        {
            var textoParrafo = parrafo.Trim();

            if (string.IsNullOrWhiteSpace(textoParrafo))
            {
                continue;
            }

            if (textoParrafo.Length <= tamanoMaximoChunk)
            {
                yield return new UnidadTexto(
                    numeroPagina,
                    textoParrafo);

                continue;
            }

            foreach (var fragmento in DividirPorPalabras(
                textoParrafo,
                tamanoMaximoChunk))
            {
                yield return new UnidadTexto(
                    numeroPagina,
                    fragmento);
            }
        }
    }

    private static IEnumerable<string> DividirPorPalabras(
        string texto,
        int tamanoMaximoChunk)
    {
        var palabras = Regex.Split(
            texto.Trim(),
            @"\s+");

        var fragmentoActual = new StringBuilder();

        foreach (var palabra in palabras)
        {
            if (fragmentoActual.Length == 0)
            {
                fragmentoActual.Append(palabra);
                continue;
            }

            if (fragmentoActual.Length +
                1 +
                palabra.Length <= tamanoMaximoChunk)
            {
                fragmentoActual.Append(' ');
                fragmentoActual.Append(palabra);
                continue;
            }

            yield return fragmentoActual.ToString();

            fragmentoActual.Clear();
            fragmentoActual.Append(palabra);
        }

        if (fragmentoActual.Length > 0)
        {
            yield return fragmentoActual.ToString();
        }
    }

    private static string ObtenerSolapamiento(
        string texto,
        int longitudMaxima)
    {
        if (longitudMaxima <= 0 ||
            string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var longitud = Math.Min(
            longitudMaxima,
            texto.Length);

        var inicio = texto.Length - longitud;

        while (inicio > 0 &&
            !char.IsWhiteSpace(texto[inicio - 1]))
        {
            inicio--;
        }

        return texto[inicio..].Trim();
    }

    private static void AgregarChunk(
        ICollection<ChunkTextoDocumentoDto> chunks,
        string texto,
        int paginaInicial,
        int paginaFinal)
    {
        var textoNormalizado = texto.Trim();

        if (string.IsNullOrWhiteSpace(textoNormalizado))
        {
            return;
        }

        chunks.Add(new ChunkTextoDocumentoDto
        {
            PaginaInicial = paginaInicial,
            PaginaFinal = paginaFinal,
            Texto = textoNormalizado
        });
    }

    private static IReadOnlyCollection<ChunkTextoDocumentoDto>
        ConsolidarChunksPequenos(
            IReadOnlyCollection<ChunkTextoDocumentoDto> chunks,
            ConfiguracionProcesamientoDocumentoDto configuracion)
    {
        var resultado = new List<ChunkTextoDocumentoDto>();

        foreach (var chunk in chunks)
        {
            if (resultado.Count == 0 ||
                chunk.Texto.Length >=
                    configuracion.LongitudMinimaChunk)
            {
                resultado.Add(chunk);
                continue;
            }

            var chunkAnterior = resultado[^1];

            var textoCombinado =
                $"{chunkAnterior.Texto}\n\n{chunk.Texto}";

            if (textoCombinado.Length >
                configuracion.TamanoMaximoChunk)
            {
                resultado.Add(chunk);
                continue;
            }

            resultado[^1] = new ChunkTextoDocumentoDto
            {
                PaginaInicial = chunkAnterior.PaginaInicial,
                PaginaFinal = chunk.PaginaFinal,
                Texto = textoCombinado
            };
        }

        return resultado;
    }

    private static void ValidarConfiguracion(
        ConfiguracionProcesamientoDocumentoDto configuracion)
    {
        if (configuracion.TamanoMaximoChunk < 200)
        {
            throw new ArgumentOutOfRangeException(
                nameof(configuracion),
                "El tamaño máximo del chunk debe ser de al menos 200.");
        }

        if (configuracion.LongitudMinimaChunk <= 0 ||
            configuracion.LongitudMinimaChunk >
                configuracion.TamanoMaximoChunk)
        {
            throw new ArgumentOutOfRangeException(
                nameof(configuracion),
                "La longitud mínima del chunk no es válida.");
        }

        if (configuracion.SolapamientoChunk < 0 ||
            configuracion.SolapamientoChunk >=
                configuracion.TamanoMaximoChunk)
        {
            throw new ArgumentOutOfRangeException(
                nameof(configuracion),
                "El solapamiento del chunk no es válido.");
        }
    }

    private sealed record UnidadTexto(
        int NumeroPagina,
        string Texto);
}