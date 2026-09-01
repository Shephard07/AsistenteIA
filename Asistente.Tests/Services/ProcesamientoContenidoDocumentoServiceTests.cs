using Asistente.Application.DTOs;
using Asistente.Application.Services;
using Xunit;

namespace Asistente.Tests.Services;

public class ProcesamientoContenidoDocumentoServiceTests
{
    [Fact]
    public void Normalizar_Debe_Limpiar_Espacios_Saltos_Y_Caracteres_De_Control()
    {
        var service = new NormalizadorContenidoDocumentoService();

        var resultado = service.Normalizar(
            "  Título\r\n\r\nTexto   con\t espacios\u0001\r\n\r\n\r\nLista ");

        Assert.Equal(
            "Título\n\nTexto con espacios\n\nLista",
            resultado);
    }

    [Fact]
    public void GenerarChunks_Debe_Respetar_Tamano_Maximo_Sin_Cortar_Palabras()
    {
        var service = new ChunkingDocumentoService();

        var texto = string.Join(
            ' ',
            Enumerable.Repeat("palabra", 70));

        var resultado = service.GenerarChunks(
            new[]
            {
                new PaginaTextoDocumentoDto
                {
                    NumeroPagina = 1,
                    Texto = texto
                }
            },
            CrearConfiguracion());

        Assert.True(resultado.Count > 1);

        Assert.All(resultado, chunk =>
        {
            Assert.InRange(
                chunk.Texto.Length,
                1,
                200);

            Assert.Equal(1, chunk.PaginaInicial);
            Assert.Equal(1, chunk.PaginaFinal);
        });
    }

    [Fact]
    public void GenerarChunks_Debe_Conservar_Palabra_Muy_Larga_Sin_Cortarla()
    {
        var service = new ChunkingDocumentoService();

        var palabraLarga = new string('x', 250);

        var resultado = service.GenerarChunks(
            new[]
            {
                new PaginaTextoDocumentoDto
                {
                    NumeroPagina = 1,
                    Texto = $"Inicio {palabraLarga} final"
                }
            },
            CrearConfiguracion());

        Assert.Contains(
            resultado,
            chunk => chunk.Texto == palabraLarga);
    }

    [Fact]
    public void GenerarChunks_Debe_Registrar_Correctamente_Las_Paginas()
    {
        var service = new ChunkingDocumentoService();

        var textoPaginaUno = string.Join(
            ' ',
            Enumerable.Repeat("primera", 25));

        var textoPaginaDos = string.Join(
            ' ',
            Enumerable.Repeat("segunda", 25));

        var resultado = service.GenerarChunks(
            new[]
            {
                new PaginaTextoDocumentoDto
                {
                    NumeroPagina = 1,
                    Texto = textoPaginaUno
                },
                new PaginaTextoDocumentoDto
                {
                    NumeroPagina = 2,
                    Texto = textoPaginaDos
                }
            },
            CrearConfiguracion());

        Assert.Equal(2, resultado.Count);

        Assert.Equal(1, resultado.ElementAt(0).PaginaInicial);
        Assert.Equal(1, resultado.ElementAt(0).PaginaFinal);

        Assert.Equal(2, resultado.ElementAt(1).PaginaInicial);
        Assert.Equal(2, resultado.ElementAt(1).PaginaFinal);
    }

    private static ConfiguracionProcesamientoDocumentoDto
        CrearConfiguracion()
    {
        return new ConfiguracionProcesamientoDocumentoDto
        {
            TamanoMaximoChunk = 200,
            SolapamientoChunk = 20,
            LongitudMinimaChunk = 20,
            FrecuenciaSegundos = 15,
            MaximoDocumentosPorCiclo = 2
        };
    }
}