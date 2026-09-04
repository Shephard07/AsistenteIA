using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Application.Services;
using Asistente.Domain.Entities;
using Asistente.Domain.Interfaces;
using Moq;
using Xunit;

namespace Asistente.Tests.Services;

public class RecuperacionContextoRagServiceTests
{
    private readonly Mock<IEmbeddingConfiguracionRepository>
        _configuracionRepositoryMock = new();

    private readonly Mock<IEmbeddingProvider>
        _embeddingProviderMock = new();

    private readonly Mock<IVectorStore> _vectorStoreMock = new();

    [Fact]
    public async Task RecuperarAsync_Debe_Lanzar_Error_Cuando_La_Consulta_Esta_Vacia()
    {
        var service = CrearServicio();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.RecuperarAsync("   "));

        Assert.Equal("consulta", exception.ParamName);

        _configuracionRepositoryMock.Verify(
            repository => repository.ObtenerActivaAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RecuperarAsync_Debe_Generar_Embedding_Buscar_Y_Construir_Contexto()
    {
        const string consulta = "¿Qué indica el documento?";
        var embedding = new[] { 0.12f, 0.45f, 0.78f };

        ConfigurarConsulta(
            cantidadResultados: 3,
            puntajeMinimo: 0.65m,
            longitudMaximaContexto: 4000,
            embedding: embedding,
            resultados:
            [
                CrearResultado(
                    idDocumento: 10,
                    idVersion: 2,
                    numeroChunk: 3,
                    paginaInicial: 4,
                    paginaFinal: 5,
                    texto: "El documento indica que el proceso es automático.",
                    puntaje: 0.92m)
            ]);

        var service = CrearServicio();

        var resultado = await service.RecuperarAsync(consulta);

        Assert.True(resultado.TieneResultados);
        Assert.Single(resultado.Fragmentos);

        Assert.Contains(
            "CONTEXTO DOCUMENTAL RECUPERADO",
            resultado.Contenido);

        Assert.Contains(
            "[Documento #10, versión 2, páginas 4-5, chunk 3]",
            resultado.Contenido);

        Assert.Contains(
            "El documento indica que el proceso es automático.",
            resultado.Contenido);

        var fragmento = resultado.Fragmentos.Single();

        Assert.Equal(10, fragmento.IdDocumento);
        Assert.Equal(2, fragmento.IdVersionDocumento);
        Assert.Equal(3, fragmento.NumeroChunk);
        Assert.Equal(4, fragmento.PaginaInicial);
        Assert.Equal(5, fragmento.PaginaFinal);
        Assert.Equal(0.92m, fragmento.Puntaje);

        _embeddingProviderMock.Verify(
            provider => provider.GenerarAsync(
                consulta,
                "nomic-embed-text",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _vectorStoreMock.Verify(
            store => store.BuscarAsync(
                It.Is<BusquedaVectorialRequestDto>(request =>
                    request.EmbeddingConsulta.SequenceEqual(embedding) &&
                    request.CantidadResultados == 3 &&
                    request.PuntajeMinimo == 0.65m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RecuperarAsync_Debe_Devolver_Contexto_Vacio_Si_No_Hay_Resultados()
    {
        ConfigurarConsulta(
            cantidadResultados: 3,
            puntajeMinimo: 0.60m,
            longitudMaximaContexto: 4000,
            embedding: [0.10f, 0.20f],
            resultados: Array.Empty<ResultadoBusquedaVectorialDto>());

        var service = CrearServicio();

        var resultado = await service.RecuperarAsync(
            "Consulta sin coincidencias");

        Assert.False(resultado.TieneResultados);
        Assert.Empty(resultado.Contenido);
        Assert.Empty(resultado.Fragmentos);
    }

    [Fact]
    public async Task RecuperarAsync_Debe_Ignorar_Resultados_Sin_Texto()
    {
        ConfigurarConsulta(
            cantidadResultados: 3,
            puntajeMinimo: 0.60m,
            longitudMaximaContexto: 4000,
            embedding: [0.10f, 0.20f],
            resultados:
            [
                CrearResultado(
                    idDocumento: 1,
                    idVersion: 1,
                    numeroChunk: 1,
                    paginaInicial: 1,
                    paginaFinal: 1,
                    texto: "   ",
                    puntaje: 0.90m),
                CrearResultado(
                    idDocumento: 2,
                    idVersion: 1,
                    numeroChunk: 2,
                    paginaInicial: 2,
                    paginaFinal: 3,
                    texto: "Contenido válido para el contexto.",
                    puntaje: 0.85m)
            ]);

        var service = CrearServicio();

        var resultado = await service.RecuperarAsync(
            "Buscar contenido válido");

        Assert.True(resultado.TieneResultados);
        Assert.Single(resultado.Fragmentos);
        Assert.Equal(2, resultado.Fragmentos.Single().IdDocumento);

        Assert.DoesNotContain(
            "[Documento #1",
            resultado.Contenido);

        Assert.Contains(
            "[Documento #2, versión 1, páginas 2-3, chunk 2]",
            resultado.Contenido);
    }

    [Fact]
    public async Task RecuperarAsync_Debe_Respetar_Longitud_Maxima_Del_Contexto()
    {
        var textoLargo = new string('a', 2000);

        ConfigurarConsulta(
            cantidadResultados: 3,
            puntajeMinimo: 0.60m,
            longitudMaximaContexto: 1200,
            embedding: [0.10f, 0.20f],
            resultados:
            [
                CrearResultado(
                    idDocumento: 5,
                    idVersion: 2,
                    numeroChunk: 1,
                    paginaInicial: 1,
                    paginaFinal: 4,
                    texto: textoLargo,
                    puntaje: 0.95m)
            ]);

        var service = CrearServicio();

        var resultado = await service.RecuperarAsync(
            "Consulta con contexto limitado");

        Assert.True(resultado.TieneResultados);
        Assert.True(resultado.Contenido.Length <= 1200);
        Assert.Single(resultado.Fragmentos);

        Assert.True(
            resultado.Fragmentos.Single().Texto.Length <
            textoLargo.Length);
    }

    private RecuperacionContextoRagService CrearServicio()
    {
        return new RecuperacionContextoRagService(
            _configuracionRepositoryMock.Object,
            _embeddingProviderMock.Object,
            _vectorStoreMock.Object);
    }

    private void ConfigurarConsulta(
        int cantidadResultados,
        decimal puntajeMinimo,
        int longitudMaximaContexto,
        float[] embedding,
        IReadOnlyCollection<ResultadoBusquedaVectorialDto> resultados)
    {
        var configuracion = new EmbeddingConfiguracion(
            proveedor: "Ollama",
            modeloEmbeddings: "nomic-embed-text",
            baseVectorial: "ChromaDB",
            cantidadResultados: cantidadResultados,
            puntajeMinimo: puntajeMinimo,
            longitudMaximaContexto: longitudMaximaContexto);

        _configuracionRepositoryMock
            .Setup(repository => repository.ObtenerActivaAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configuracion);

        _embeddingProviderMock
            .Setup(provider => provider.GenerarAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);

        _vectorStoreMock
            .Setup(store => store.BuscarAsync(
                It.IsAny<BusquedaVectorialRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultados);
    }

    private static ResultadoBusquedaVectorialDto CrearResultado(
        int idDocumento,
        int idVersion,
        int numeroChunk,
        int paginaInicial,
        int paginaFinal,
        string texto,
        decimal puntaje)
    {
        return new ResultadoBusquedaVectorialDto
        {
            IdentificadorDocumentoIndexado = Guid.NewGuid(),
            IdDocumento = idDocumento,
            IdVersionDocumento = idVersion,
            IdDocumentoProcesado = idVersion,
            IdCategoria = 1,
            NumeroChunk = numeroChunk,
            PaginaInicial = paginaInicial,
            PaginaFinal = paginaFinal,
            Texto = texto,
            Puntaje = puntaje
        };
    }
}