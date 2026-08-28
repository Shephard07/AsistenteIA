// ConfiguracionGestorDocumental.cs
namespace Asistente.Infrastructure.Configuration;

public class ConfiguracionGestorDocumental
{
    public const string Seccion = "GestorDocumental";

    public string RutaArchivos { get; init; } =
        @"C:\AsistenteIAData\Documentos";

    public long TamanoMaximoBytes { get; init; } =
        10 * 1024 * 1024;

    public IReadOnlyCollection<string> ExtensionesPermitidas { get; init; } =
        new[] { ".pdf" };
}