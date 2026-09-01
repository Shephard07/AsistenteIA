// Documento.cs
using Asistente.Domain.Enums;

namespace Asistente.Domain.Entities;

public class Documento
{
    public int IdDocumento { get; private set; }

    public string Codigo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public int IdCategoria { get; private set; }

    public CategoriaDocumento? Categoria { get; private set; }

    public int VersionActual { get; private set; }

    public EstadoDocumento Estado { get; private set; }

    public EstadoProcesamientoDocumento EstadoProcesamiento { get; private set; }

    public DateTime FechaRegistro { get; private set; }

    public string UsuarioRegistro { get; private set; } = string.Empty;

    public ICollection<DocumentoVersion> Versiones { get; private set; }
        = new List<DocumentoVersion>();

    public ICollection<AuditoriaActividad> ActividadesAuditoria { get; private set; }
        = new List<AuditoriaActividad>();

    private Documento()
    {
    }

    public Documento(
        string codigo,
        string nombre,
        string descripcion,
        int idCategoria,
        string usuarioRegistro)
    {
        if (idCategoria <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idCategoria),
                "La categoría del documento es obligatoria.");
        }

        Codigo = ValidarTextoObligatorio(codigo, nameof(codigo));
        Nombre = ValidarTextoObligatorio(nombre, nameof(nombre));
        Descripcion = descripcion?.Trim() ?? string.Empty;
        IdCategoria = idCategoria;
        UsuarioRegistro = ValidarTextoObligatorio(
            usuarioRegistro,
            nameof(usuarioRegistro));

        VersionActual = 0;
        Estado = EstadoDocumento.Borrador;
        EstadoProcesamiento =
            EstadoProcesamientoDocumento.PendienteProcesamiento;

        FechaRegistro = DateTime.UtcNow;
    }

    public void ActualizarInformacion(
        string nombre,
        string descripcion,
        int idCategoria)
    {
        if (Estado == EstadoDocumento.Eliminado)
        {
            throw new InvalidOperationException(
                "No se puede editar un documento eliminado.");
        }

        if (idCategoria <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idCategoria),
                "La categoría del documento es obligatoria.");
        }

        Nombre = ValidarTextoObligatorio(nombre, nameof(nombre));
        Descripcion = descripcion?.Trim() ?? string.Empty;
        IdCategoria = idCategoria;
    }

    public void AgregarVersion(DocumentoVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);

        if (Estado == EstadoDocumento.Eliminado)
        {
            throw new InvalidOperationException(
                "No se puede agregar una versión a un documento eliminado.");
        }

        if (version.NumeroVersion != VersionActual + 1)
        {
            throw new InvalidOperationException(
                "El número de versión no es consecutivo.");
        }

        foreach (var versionAnterior in Versiones)
        {
            versionAnterior.Desactivar();
        }

        version.Activar();
        Versiones.Add(version);
        VersionActual = version.NumeroVersion;
        EstadoProcesamiento =
            EstadoProcesamientoDocumento.PendienteProcesamiento;
    }

    public void Activar()
    {
        if (Estado == EstadoDocumento.Eliminado)
        {
            throw new InvalidOperationException(
                "No se puede activar un documento eliminado.");
        }

        if (VersionActual == 0)
        {
            throw new InvalidOperationException(
                "El documento debe tener al menos una versión para activarse.");
        }

        Estado = EstadoDocumento.Activo;
    }

    public void Archivar()
    {
        if (Estado == EstadoDocumento.Eliminado)
        {
            throw new InvalidOperationException(
                "No se puede archivar un documento eliminado.");
        }

        Estado = EstadoDocumento.Archivado;
    }

    public void Eliminar()
    {
        Estado = EstadoDocumento.Eliminado;
    }

    public void ActualizarEstadoProcesamiento(
    EstadoProcesamientoDocumento estado)
    {
        EstadoProcesamiento = estado;
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