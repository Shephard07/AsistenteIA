using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using UglyToad.PdfPig;

namespace Asistente.Infrastructure.Services;

public class ExtractorTextoPdfService : IExtractorTextoDocumento
{
    public bool Soporta(string nombreArchivo)
    {
        return string.Equals(
            Path.GetExtension(nombreArchivo),
            ".pdf",
            StringComparison.OrdinalIgnoreCase);
    }

    public Task<IReadOnlyCollection<PaginaTextoDocumentoDto>> ExtraerAsync(
        Stream contenido,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contenido);

        if (!contenido.CanRead)
        {
            throw new ArgumentException(
                "El contenido del archivo no se puede leer.",
                nameof(contenido));
        }

        if (contenido.CanSeek)
        {
            contenido.Position = 0;
        }

        using var documento = PdfDocument.Open(contenido);

        var paginas = new List<PaginaTextoDocumentoDto>();

        foreach (var pagina in documento.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            paginas.Add(new PaginaTextoDocumentoDto
            {
                NumeroPagina = pagina.Number,
                Texto = pagina.Text
            });
        }

        return Task.FromResult<IReadOnlyCollection<PaginaTextoDocumentoDto>>(
            paginas);
    }
}