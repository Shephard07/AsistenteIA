using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asistente.Infrastructure.Services;

public class ChromaVectorStore : IVectorStore
{
    private readonly HttpClient _httpClient;
    private readonly ChromaDbOptions _options;
    private readonly ILogger<ChromaVectorStore> _logger;

    public ChromaVectorStore(
        HttpClient httpClient,
        IOptions<ChromaDbOptions> options,
        ILogger<ChromaVectorStore> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> EstaDisponibleAsync(
    CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(
                "api/v2/heartbeat",
                cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "No fue posible verificar la disponibilidad de ChromaDB.");

            return false;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "No fue posible verificar la disponibilidad de ChromaDB.");

            return false;
        }
    }

    public async Task IndexarAsync(
        DocumentoVectorialDto documento,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(documento);
        ValidarDocumento(documento);

        var idColeccion = await ObtenerOCrearColeccionAsync(
            cancellationToken);

        var request = new Dictionary<string, object>
        {
            ["ids"] = new[] { documento.IdVector },
            ["embeddings"] = new[] { documento.Embedding },
            ["documents"] = new[] { documento.Texto },
            ["metadatas"] = new[]
            {
                CrearMetadatos(documento)
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{RutaColecciones}/{idColeccion}/upsert",
            request,
            cancellationToken);

        await ValidarRespuestaAsync(
            response,
            "indexar el chunk en ChromaDB",
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<ResultadoBusquedaVectorialDto>>
        BuscarAsync(
            BusquedaVectorialRequestDto request,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidarBusqueda(request);

        var idColeccion = await ObtenerIdColeccionAsync(
            cancellationToken);

        if (idColeccion is null)
        {
            return Array.Empty<ResultadoBusquedaVectorialDto>();
        }

        var cuerpo = new Dictionary<string, object>
        {
            ["query_embeddings"] = new[]
            {
                request.EmbeddingConsulta
            },
            ["n_results"] = request.CantidadResultados,
            ["include"] = new[]
            {
                "documents",
                "metadatas",
                "distances"
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{RutaColecciones}/{idColeccion}/query",
            cuerpo,
            cancellationToken);

        await ValidarRespuestaAsync(
            response,
            "buscar documentos en ChromaDB",
            cancellationToken);

        var contenido = await response.Content.ReadAsStringAsync(
            cancellationToken);

        return MapearResultados(
            contenido,
            request.PuntajeMinimo);
    }

    public async Task EliminarPorDocumentoAsync(
        Guid identificadorDocumentoIndexado,
        CancellationToken cancellationToken = default)
    {
        if (identificadorDocumentoIndexado == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del documento indexado es obligatorio.",
                nameof(identificadorDocumentoIndexado));
        }

        var idColeccion = await ObtenerIdColeccionAsync(
            cancellationToken);

        if (idColeccion is null)
        {
            return;
        }

        var request = new Dictionary<string, object>
        {
            ["where"] = new Dictionary<string, string>
            {
                ["identificadorDocumentoIndexado"] =
                    identificadorDocumentoIndexado.ToString("N")
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"{RutaColecciones}/{idColeccion}/delete",
            request,
            cancellationToken);

        await ValidarRespuestaAsync(
            response,
            "eliminar los vectores del documento en ChromaDB",
            cancellationToken);
    }

    private string RutaColecciones
    {
        get
        {
            var tenant = Uri.EscapeDataString(_options.Tenant);
            var database = Uri.EscapeDataString(_options.Database);

            return "api/v2/tenants/" + tenant +
                "/databases/" + database +
                "/collections";
        }
    }

    private async Task<string> ObtenerOCrearColeccionAsync(
        CancellationToken cancellationToken)
    {
        var idExistente = await ObtenerIdColeccionAsync(
            cancellationToken);

        if (idExistente is not null)
        {
            return idExistente;
        }

        var request = new Dictionary<string, object>
        {
            ["name"] = _options.NombreColeccion,
            ["configuration"] = new Dictionary<string, object>
            {
                ["hnsw"] = new Dictionary<string, object>
                {
                    ["space"] = "cosine"
                }
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            RutaColecciones,
            request,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return await ObtenerIdColeccionAsync(cancellationToken)
                ?? throw new InvalidOperationException(
                    "La colección ya existe, pero no fue posible obtenerla.");
        }

        await ValidarRespuestaAsync(
            response,
            "crear la colección de ChromaDB",
            cancellationToken);

        var contenido = await response.Content.ReadAsStringAsync(
            cancellationToken);

        using var documentoJson = JsonDocument.Parse(contenido);

        if (documentoJson.RootElement.TryGetProperty(
                "id",
                out var idColeccion) &&
            !string.IsNullOrWhiteSpace(idColeccion.GetString()))
        {
            return idColeccion.GetString()!;
        }

        throw new InvalidOperationException(
            "ChromaDB creó la colección, pero no devolvió su identificador.");
    }

    private async Task<string?> ObtenerIdColeccionAsync(
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            RutaColecciones,
            cancellationToken);

        await ValidarRespuestaAsync(
            response,
            "obtener las colecciones de ChromaDB",
            cancellationToken);

        var contenido = await response.Content.ReadAsStringAsync(
            cancellationToken);

        using var documentoJson = JsonDocument.Parse(contenido);

        if (documentoJson.RootElement.ValueKind !=
            JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                "ChromaDB devolvió una lista de colecciones no válida.");
        }

        foreach (var coleccion in
                 documentoJson.RootElement.EnumerateArray())
        {
            var nombre = coleccion.TryGetProperty(
                "name",
                out var propiedadNombre)
                ? propiedadNombre.GetString()
                : null;

            if (!string.Equals(
                    nombre,
                    _options.NombreColeccion,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (coleccion.TryGetProperty(
                    "id",
                    out var propiedadId) &&
                !string.IsNullOrWhiteSpace(
                    propiedadId.GetString()))
            {
                return propiedadId.GetString();
            }
        }

        return null;
    }

    private async Task ValidarRespuestaAsync(
        HttpResponseMessage response,
        string operacion,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detalleError = await response.Content
            .ReadAsStringAsync(cancellationToken);

        _logger.LogError(
            "ChromaDB devolvió el estado {StatusCode} al {Operacion}. " +
            "Detalle: {DetalleError}",
            response.StatusCode,
            operacion,
            detalleError);

        throw new HttpRequestException(
            $"No fue posible {operacion}.");
    }

    private static Dictionary<string, object> CrearMetadatos(
        DocumentoVectorialDto documento)
    {
        return new Dictionary<string, object>
        {
            ["identificadorDocumentoIndexado"] =
                documento.IdentificadorDocumentoIndexado.ToString("N"),

            ["idDocumento"] = documento.IdDocumento,
            ["idVersionDocumento"] = documento.IdVersionDocumento,
            ["idDocumentoProcesado"] = documento.IdDocumentoProcesado,
            ["idCategoria"] = documento.IdCategoria,
            ["numeroChunk"] = documento.NumeroChunk,
            ["paginaInicial"] = documento.PaginaInicial,
            ["paginaFinal"] = documento.PaginaFinal
        };
    }

    private static IReadOnlyCollection<ResultadoBusquedaVectorialDto>
        MapearResultados(
            string contenido,
            decimal puntajeMinimo)
    {
        using var documentoJson = JsonDocument.Parse(contenido);
        var raiz = documentoJson.RootElement;

        if (!raiz.TryGetProperty(
                "documents",
                out var gruposDocumentos) ||
            gruposDocumentos.ValueKind != JsonValueKind.Array ||
            gruposDocumentos.GetArrayLength() == 0)
        {
            return Array.Empty<ResultadoBusquedaVectorialDto>();
        }

        var documentos = gruposDocumentos[0];

        if (documentos.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<ResultadoBusquedaVectorialDto>();
        }

        var metadatos = raiz.TryGetProperty(
            "metadatas",
            out var gruposMetadatos) &&
            gruposMetadatos.ValueKind == JsonValueKind.Array &&
            gruposMetadatos.GetArrayLength() > 0
                ? gruposMetadatos[0]
                : default;

        var distancias = raiz.TryGetProperty(
            "distances",
            out var gruposDistancias) &&
            gruposDistancias.ValueKind == JsonValueKind.Array &&
            gruposDistancias.GetArrayLength() > 0
                ? gruposDistancias[0]
                : default;

        var resultados = new List<ResultadoBusquedaVectorialDto>();

        for (var indice = 0;
             indice < documentos.GetArrayLength();
             indice++)
        {
            if (metadatos.ValueKind != JsonValueKind.Array ||
                indice >= metadatos.GetArrayLength() ||
                distancias.ValueKind != JsonValueKind.Array ||
                indice >= distancias.GetArrayLength())
            {
                continue;
            }

            var metadata = metadatos[indice];
            var distancia = distancias[indice];

            if (distancia.ValueKind != JsonValueKind.Number)
            {
                continue;
            }

            var puntaje = Math.Clamp(
                1m - (decimal)distancia.GetDouble(),
                0m,
                1m);

            if (puntaje < puntajeMinimo)
            {
                continue;
            }

            var identificador = ObtenerTexto(
                metadata,
                "identificadorDocumentoIndexado");

            if (!Guid.TryParse(identificador, out var idDocumentoIndexado))
            {
                continue;
            }

            resultados.Add(new ResultadoBusquedaVectorialDto
            {
                IdentificadorDocumentoIndexado = idDocumentoIndexado,
                IdDocumento = ObtenerEntero(metadata, "idDocumento"),
                IdVersionDocumento = ObtenerEntero(
                    metadata,
                    "idVersionDocumento"),

                IdDocumentoProcesado = ObtenerEntero(
                    metadata,
                    "idDocumentoProcesado"),

                IdCategoria = ObtenerEntero(metadata, "idCategoria"),
                NumeroChunk = ObtenerEntero(metadata, "numeroChunk"),
                PaginaInicial = ObtenerEntero(metadata, "paginaInicial"),
                PaginaFinal = ObtenerEntero(metadata, "paginaFinal"),
                Texto = documentos[indice].GetString() ?? string.Empty,
                Puntaje = puntaje
            });
        }

        return resultados;
    }

    private static string ObtenerTexto(
        JsonElement metadata,
        string propiedad)
    {
        return metadata.ValueKind == JsonValueKind.Object &&
            metadata.TryGetProperty(propiedad, out var valor)
                ? valor.GetString() ?? string.Empty
                : string.Empty;
    }

    private static int ObtenerEntero(
        JsonElement metadata,
        string propiedad)
    {
        if (metadata.ValueKind != JsonValueKind.Object ||
            !metadata.TryGetProperty(propiedad, out var valor))
        {
            return 0;
        }

        return valor.ValueKind == JsonValueKind.Number &&
            valor.TryGetInt32(out var resultado)
                ? resultado
                : 0;
    }

    private static void ValidarDocumento(
        DocumentoVectorialDto documento)
    {
        if (documento.IdentificadorDocumentoIndexado == Guid.Empty)
        {
            throw new ArgumentException(
                "El identificador del documento indexado es obligatorio.",
                nameof(documento));
        }

        if (documento.IdDocumento <= 0 ||
            documento.IdVersionDocumento <= 0 ||
            documento.IdDocumentoProcesado <= 0 ||
            documento.IdCategoria <= 0 ||
            documento.NumeroChunk <= 0)
        {
            throw new ArgumentException(
                "Los identificadores del chunk no son válidos.",
                nameof(documento));
        }

        if (string.IsNullOrWhiteSpace(documento.Texto))
        {
            throw new ArgumentException(
                "El texto del chunk es obligatorio.",
                nameof(documento));
        }

        if (documento.Embedding.Length == 0 ||
            documento.Embedding.Any(valor =>
                float.IsNaN(valor) || float.IsInfinity(valor)))
        {
            throw new ArgumentException(
                "El embedding del chunk no es válido.",
                nameof(documento));
        }
    }

    private static void ValidarBusqueda(
        BusquedaVectorialRequestDto request)
    {
        if (request.EmbeddingConsulta.Length == 0 ||
            request.EmbeddingConsulta.Any(valor =>
                float.IsNaN(valor) || float.IsInfinity(valor)))
        {
            throw new ArgumentException(
                "El embedding de consulta no es válido.",
                nameof(request));
        }

        if (request.CantidadResultados <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "La cantidad de resultados debe ser mayor que cero.");
        }

        if (request.PuntajeMinimo < 0m ||
            request.PuntajeMinimo > 1m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "El puntaje mínimo debe estar entre cero y uno.");
        }
    }
}