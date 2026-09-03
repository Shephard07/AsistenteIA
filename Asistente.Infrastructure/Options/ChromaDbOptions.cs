namespace Asistente.Infrastructure.Options;

public class ChromaDbOptions
{
    public const string SectionName = "ChromaDb";

    public string BaseUrl { get; set; } = string.Empty;

    public string NombreColeccion { get; set; } = string.Empty;

    public string Tenant { get; set; } = "default_tenant";

    public string Database { get; set; } = "default_database";
}