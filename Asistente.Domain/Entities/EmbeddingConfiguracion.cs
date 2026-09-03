namespace Asistente.Domain.Entities;

public class EmbeddingConfiguracion
{
    public int IdConfiguracion { get; private set; }

    public string Proveedor { get; private set; } = string.Empty;

    public string ModeloEmbeddings { get; private set; } = string.Empty;

    public string BaseVectorial { get; private set; } = string.Empty;

    public int CantidadResultados { get; private set; }

    public decimal PuntajeMinimo { get; private set; }

    public int LongitudMaximaContexto { get; private set; }

    public bool Activo { get; private set; }

    private EmbeddingConfiguracion()
    {
    }

    public EmbeddingConfiguracion(
        string proveedor,
        string modeloEmbeddings,
        string baseVectorial,
        int cantidadResultados,
        decimal puntajeMinimo,
        int longitudMaximaContexto,
        bool activo = true)
    {
        Actualizar(
            proveedor,
            modeloEmbeddings,
            baseVectorial,
            cantidadResultados,
            puntajeMinimo,
            longitudMaximaContexto);

        Activo = activo;
    }

    public void Actualizar(
        string proveedor,
        string modeloEmbeddings,
        string baseVectorial,
        int cantidadResultados,
        decimal puntajeMinimo,
        int longitudMaximaContexto)
    {
        Proveedor = ValidarTexto(proveedor, nameof(proveedor));
        ModeloEmbeddings = ValidarTexto(
            modeloEmbeddings,
            nameof(modeloEmbeddings));

        BaseVectorial = ValidarTexto(
            baseVectorial,
            nameof(baseVectorial));

        if (cantidadResultados <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadResultados),
                "La cantidad de resultados debe ser mayor que cero.");
        }

        if (puntajeMinimo < 0 || puntajeMinimo > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(puntajeMinimo),
                "El puntaje mínimo debe estar entre 0 y 1.");
        }

        if (longitudMaximaContexto <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(longitudMaximaContexto),
                "La longitud máxima del contexto debe ser mayor que cero.");
        }

        CantidadResultados = cantidadResultados;
        PuntajeMinimo = puntajeMinimo;
        LongitudMaximaContexto = longitudMaximaContexto;
    }

    public void CambiarEstado(bool activo)
    {
        Activo = activo;
    }

    private static string ValidarTexto(string? valor, string nombreParametro)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException(
                "El valor es obligatorio.",
                nombreParametro);
        }

        return valor.Trim();
    }
}