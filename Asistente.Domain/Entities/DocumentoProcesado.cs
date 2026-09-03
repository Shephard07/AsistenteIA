//DocumentoProcesado.cs
using Asistente.Domain.Enums;

namespace Asistente.Domain.Entities;

public class DocumentoProcesado
{
    public int IdDocumentoProcesado { get; private set; }

    public int IdVersionDocumento { get; private set; }

    public DocumentoVersion? VersionDocumento { get; private set; }

    public DateTime? FechaInicio { get; private set; }

    public DateTime? FechaFin { get; private set; }

    public EstadoProcesamientoDocumento Estado { get; private set; }

    public int TotalPaginas { get; private set; }

    public int TotalCaracteres { get; private set; }

    public int TotalChunks { get; private set; }

    public DocumentoIndexado? Indexacion { get; private set; }

    public string Observaciones { get; private set; } = string.Empty;

    public ICollection<DocumentoChunk> Chunks { get; private set; }
        = new List<DocumentoChunk>();

    private DocumentoProcesado()
    {
    }

    public DocumentoProcesado(int idVersionDocumento)
    {
        if (idVersionDocumento <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idVersionDocumento),
                "La versión del documento es obligatoria.");
        }

        IdVersionDocumento = idVersionDocumento;
        Estado = EstadoProcesamientoDocumento.PendienteProcesamiento;
    }

    public void Iniciar()
    {
        if (Estado == EstadoProcesamientoDocumento.EnProceso)
        {
            throw new InvalidOperationException(
                "El documento ya se encuentra en procesamiento.");
        }

        Estado = EstadoProcesamientoDocumento.EnProceso;
        FechaInicio = DateTime.UtcNow;
        FechaFin = null;
        Observaciones = string.Empty;
        TotalPaginas = 0;
        TotalCaracteres = 0;
        TotalChunks = 0;
        Chunks.Clear();
    }

    public void Completar(
        int totalPaginas,
        int totalCaracteres,
        IEnumerable<DocumentoChunk> chunks)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        if (Estado != EstadoProcesamientoDocumento.EnProceso)
        {
            throw new InvalidOperationException(
                "El documento debe estar en proceso para completarlo.");
        }

        if (totalPaginas <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalPaginas),
                "El total de páginas debe ser mayor que cero.");
        }

        if (totalCaracteres < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalCaracteres),
                "El total de caracteres no puede ser negativo.");
        }

        var chunksGenerados = chunks.ToArray();

        Chunks.Clear();

        foreach (var chunk in chunksGenerados)
        {
            Chunks.Add(chunk);
        }

        TotalPaginas = totalPaginas;
        TotalCaracteres = totalCaracteres;
        TotalChunks = chunksGenerados.Length;
        Estado = EstadoProcesamientoDocumento.Procesado;
        FechaFin = DateTime.UtcNow;
        Observaciones = string.Empty;
    }

    public void RegistrarError(string observaciones)
    {
        if (string.IsNullOrWhiteSpace(observaciones))
        {
            throw new ArgumentException(
                "La observación del error es obligatoria.",
                nameof(observaciones));
        }

        Estado = EstadoProcesamientoDocumento.Error;
        FechaFin = DateTime.UtcNow;
        Observaciones = observaciones.Trim();
    }

    public void MarcarPendiente()
    {
        if (Estado == EstadoProcesamientoDocumento.EnProceso)
        {
            throw new InvalidOperationException(
                "No se puede reiniciar un procesamiento en curso.");
        }

        Estado = EstadoProcesamientoDocumento.PendienteProcesamiento;
        FechaInicio = null;
        FechaFin = null;
        TotalPaginas = 0;
        TotalCaracteres = 0;
        TotalChunks = 0;
        Observaciones = string.Empty;
        Chunks.Clear();
    }
}