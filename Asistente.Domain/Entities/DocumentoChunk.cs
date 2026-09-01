//DocumentoChunk.cs
namespace Asistente.Domain.Entities;

public class DocumentoChunk
{
    public int IdChunk { get; private set; }

    public int IdDocumentoProcesado { get; private set; }

    public DocumentoProcesado? DocumentoProcesado { get; private set; }

    public int IdDocumento { get; private set; }

    public int IdVersionDocumento { get; private set; }

    public int IdCategoria { get; private set; }

    public int NumeroChunk { get; private set; }

    public int PaginaInicial { get; private set; }

    public int PaginaFinal { get; private set; }

    public string Texto { get; private set; } = string.Empty;

    public int TotalCaracteres { get; private set; }

    public int Orden { get; private set; }

    private DocumentoChunk()
    {
    }

    public DocumentoChunk(
        int idDocumento,
        int idVersionDocumento,
        int idCategoria,
        int numeroChunk,
        int paginaInicial,
        int paginaFinal,
        string texto,
        int orden)
    {
        if (idDocumento <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idDocumento),
                "El documento es obligatorio.");
        }

        if (idVersionDocumento <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idVersionDocumento),
                "La versión del documento es obligatoria.");
        }

        if (idCategoria <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idCategoria),
                "La categoría del documento es obligatoria.");
        }

        if (numeroChunk <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numeroChunk),
                "El número de chunk debe ser mayor que cero.");
        }

        if (paginaInicial <= 0 || paginaFinal < paginaInicial)
        {
            throw new ArgumentOutOfRangeException(
                nameof(paginaInicial),
                "El rango de páginas no es válido.");
        }

        if (orden <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(orden),
                "El orden del chunk debe ser mayor que cero.");
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new ArgumentException(
                "El texto del chunk es obligatorio.",
                nameof(texto));
        }

        IdDocumento = idDocumento;
        IdVersionDocumento = idVersionDocumento;
        IdCategoria = idCategoria;
        NumeroChunk = numeroChunk;
        PaginaInicial = paginaInicial;
        PaginaFinal = paginaFinal;
        Texto = texto.Trim();
        TotalCaracteres = Texto.Length;
        Orden = orden;
    }
}