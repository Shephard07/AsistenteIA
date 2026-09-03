using Asistente.Domain.Enums;

namespace Asistente.Domain.Entities;

public class DocumentoIndexado
{
    public int IdDocumentoIndexado { get; private set; }

    public int IdDocumentoProcesado { get; private set; }

    public DocumentoProcesado? DocumentoProcesado { get; private set; }

    // Identificador técnico estable para eliminar o reemplazar
    // los vectores de este documento en ChromaDB.
    public Guid IdentificadorVectorial { get; private set; }

    public DateTime? FechaInicio { get; private set; }

    public DateTime? FechaIndexacion { get; private set; }

    public EstadoIndexacionDocumento Estado { get; private set; }

    public int TotalChunks { get; private set; }

    public int TotalEmbeddings { get; private set; }

    public string Observaciones { get; private set; } = string.Empty;

    private DocumentoIndexado()
    {
    }

    public DocumentoIndexado(int idDocumentoProcesado)
    {
        if (idDocumentoProcesado <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idDocumentoProcesado),
                "El documento procesado es obligatorio.");
        }

        IdDocumentoProcesado = idDocumentoProcesado;
        IdentificadorVectorial = Guid.NewGuid();
        Estado = EstadoIndexacionDocumento.Pendiente;
    }

    public void Iniciar(int totalChunks)
    {
        if (Estado == EstadoIndexacionDocumento.EnProceso)
        {
            throw new InvalidOperationException(
                "El documento ya se encuentra en indexación.");
        }

        if (totalChunks <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalChunks),
                "El total de chunks debe ser mayor que cero.");
        }

        Estado = EstadoIndexacionDocumento.EnProceso;
        FechaInicio = DateTime.UtcNow;
        FechaIndexacion = null;
        TotalChunks = totalChunks;
        TotalEmbeddings = 0;
        Observaciones = string.Empty;
    }

    public void Completar(int totalEmbeddings)
    {
        if (Estado != EstadoIndexacionDocumento.EnProceso)
        {
            throw new InvalidOperationException(
                "El documento debe estar en indexación para completarlo.");
        }

        if (totalEmbeddings != TotalChunks)
        {
            throw new InvalidOperationException(
                "Debe generarse un embedding por cada chunk.");
        }

        Estado = EstadoIndexacionDocumento.Indexado;
        FechaIndexacion = DateTime.UtcNow;
        TotalEmbeddings = totalEmbeddings;
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

        Estado = EstadoIndexacionDocumento.Error;
        FechaIndexacion = DateTime.UtcNow;
        Observaciones = observaciones.Trim();
    }

    public void MarcarPendiente()
    {
        if (Estado == EstadoIndexacionDocumento.EnProceso)
        {
            throw new InvalidOperationException(
                "No se puede reiniciar una indexación en curso.");
        }

        Estado = EstadoIndexacionDocumento.Pendiente;
        FechaInicio = null;
        FechaIndexacion = null;
        TotalChunks = 0;
        TotalEmbeddings = 0;
        Observaciones = string.Empty;
    }
}