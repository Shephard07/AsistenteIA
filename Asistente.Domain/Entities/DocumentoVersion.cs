// DocumentoVersion.cs
namespace Asistente.Domain.Entities;

public class DocumentoVersion
{
    public int IdVersion { get; private set; }

    public int IdDocumento { get; private set; }

    public Documento? Documento { get; private set; }

    public int NumeroVersion { get; private set; }

    public string NombreArchivo { get; private set; } = string.Empty;

    public string RutaArchivo { get; private set; } = string.Empty;

    public long TamanoArchivo { get; private set; }

    public string HashArchivo { get; private set; } = string.Empty;

    public DateTime FechaCarga { get; private set; }

    public string UsuarioCarga { get; private set; } = string.Empty;

    public bool Activo { get; private set; }

    public DocumentoProcesado? Procesamiento { get; private set; }

    private DocumentoVersion()
    {
    }

    public DocumentoVersion(
        int numeroVersion,
        string nombreArchivo,
        string rutaArchivo,
        long tamanoArchivo,
        string hashArchivo,
        string usuarioCarga)
    {
        if (numeroVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numeroVersion),
                "El número de versión debe ser mayor que cero.");
        }

        if (tamanoArchivo <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tamanoArchivo),
                "El tamaño del archivo debe ser mayor que cero.");
        }

        NumeroVersion = numeroVersion;
        NombreArchivo = ValidarTextoObligatorio(
            nombreArchivo,
            nameof(nombreArchivo));

        RutaArchivo = ValidarTextoObligatorio(
            rutaArchivo,
            nameof(rutaArchivo));

        HashArchivo = ValidarTextoObligatorio(
            hashArchivo,
            nameof(hashArchivo));

        UsuarioCarga = ValidarTextoObligatorio(
            usuarioCarga,
            nameof(usuarioCarga));

        TamanoArchivo = tamanoArchivo;
        FechaCarga = DateTime.UtcNow;
        Activo = true;
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }

    private static string ValidarTextoObligatorio(
        string valor,
        string nombreParametro)
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