// CategoriaDocumento.cs
namespace Asistente.Domain.Entities;

public class CategoriaDocumento
{
    public int IdCategoria { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public ICollection<Documento> Documentos { get; private set; }
        = new List<Documento>();

    private CategoriaDocumento()
    {
    }

    public CategoriaDocumento(string nombre, string descripcion)
    {
        Nombre = ValidarTextoObligatorio(nombre, nameof(nombre));
        Descripcion = descripcion?.Trim() ?? string.Empty;
        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public void Actualizar(string nombre, string descripcion)
    {
        Nombre = ValidarTextoObligatorio(nombre, nameof(nombre));
        Descripcion = descripcion?.Trim() ?? string.Empty;
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