using System.Security.Cryptography;
using Asistente.Application.DTOs;
using Asistente.Application.Interfaces;
using Asistente.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Asistente.Infrastructure.Services;

public class AlmacenamientoDocumentoService
    : IAlmacenamientoDocumentoService
{
    private readonly ConfiguracionGestorDocumental _configuracion;

    public AlmacenamientoDocumentoService(
        IOptions<ConfiguracionGestorDocumental> configuracion)
    {
        _configuracion = configuracion.Value;
    }

    public async Task<ArchivoDocumentoAlmacenadoDto> GuardarAsync(
        ArchivoDocumentoCargaDto archivo,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(archivo);

        ValidarArchivo(archivo);

        var rutaBase = ObtenerRutaBaseSegura();
        Directory.CreateDirectory(rutaBase);

        var nombreInterno = $"{Guid.NewGuid():N}.pdf";
        var rutaTemporal = Path.Combine(
            rutaBase,
            $"{Guid.NewGuid():N}.tmp");

        var rutaFinal = Path.Combine(rutaBase, nombreInterno);

        try
        {
            var hashArchivo = await CopiarYCalcularHashAsync(
                archivo.Contenido,
                rutaTemporal,
                cancellationToken);

            await ValidarContenidoPdfAsync(
                rutaTemporal,
                cancellationToken);

            File.Move(rutaTemporal, rutaFinal);

            return new ArchivoDocumentoAlmacenadoDto
            {
                NombreArchivo = Path.GetFileName(archivo.NombreArchivo),
                RutaArchivo = nombreInterno,
                TamanoArchivo = new FileInfo(rutaFinal).Length,
                HashArchivo = hashArchivo
            };
        }
        catch
        {
            EliminarArchivoSiExiste(rutaTemporal);
            EliminarArchivoSiExiste(rutaFinal);
            throw;
        }
    }

    public Task<Stream> AbrirLecturaAsync(
        string rutaArchivo,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rutaCompleta = ObtenerRutaCompletaSegura(rutaArchivo);

        if (!File.Exists(rutaCompleta))
        {
            throw new FileNotFoundException(
                "No se encontró el archivo solicitado.");
        }

        Stream contenido = new FileStream(
            rutaCompleta,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return Task.FromResult(contenido);
    }

    public Task EliminarAsync(
        string rutaArchivo,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rutaCompleta = ObtenerRutaCompletaSegura(rutaArchivo);

        EliminarArchivoSiExiste(rutaCompleta);

        return Task.CompletedTask;
    }

    private void ValidarArchivo(ArchivoDocumentoCargaDto archivo)
    {
        if (archivo.Contenido is null || !archivo.Contenido.CanRead)
        {
            throw new ArgumentException(
                "El archivo no puede ser leído.",
                nameof(archivo));
        }

        if (string.IsNullOrWhiteSpace(archivo.NombreArchivo))
        {
            throw new ArgumentException(
                "El archivo es obligatorio.",
                nameof(archivo));
        }

        if (archivo.TamanoArchivo <= 0)
        {
            throw new ArgumentException(
                "El archivo no puede estar vacío.",
                nameof(archivo));
        }

        if (archivo.TamanoArchivo > _configuracion.TamanoMaximoBytes)
        {
            throw new ArgumentException(
                "El archivo supera el tamaño máximo permitido.",
                nameof(archivo));
        }

        var extension = Path.GetExtension(archivo.NombreArchivo);

        var extensionPermitida = _configuracion.ExtensionesPermitidas
            .Any(extensionConfigurada =>
                string.Equals(
                    extensionConfigurada,
                    extension,
                    StringComparison.OrdinalIgnoreCase));

        if (!extensionPermitida)
        {
            throw new ArgumentException(
                "Solo se permiten archivos PDF.",
                nameof(archivo));
        }
    }

    private async Task<string> CopiarYCalcularHashAsync(
        Stream contenido,
        string rutaTemporal,
        CancellationToken cancellationToken)
    {
        if (contenido.CanSeek)
        {
            contenido.Position = 0;
        }

        await using var destino = new FileStream(
            rutaTemporal,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        using var hash = IncrementalHash.CreateHash(
            HashAlgorithmName.SHA256);

        var buffer = new byte[81920];
        long totalBytes = 0;
        int bytesLeidos;

        while ((bytesLeidos = await contenido.ReadAsync(
            buffer.AsMemory(0, buffer.Length),
            cancellationToken)) > 0)
        {
            totalBytes += bytesLeidos;

            if (totalBytes > _configuracion.TamanoMaximoBytes)
            {
                throw new ArgumentException(
                    "El archivo supera el tamaño máximo permitido.");
            }

            hash.AppendData(buffer, 0, bytesLeidos);

            await destino.WriteAsync(
                buffer.AsMemory(0, bytesLeidos),
                cancellationToken);
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static async Task ValidarContenidoPdfAsync(
        string rutaTemporal,
        CancellationToken cancellationToken)
    {
        await using var archivo = new FileStream(
            rutaTemporal,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        var cabecera = new byte[5];

        var bytesLeidos = await archivo.ReadAsync(
            cabecera.AsMemory(0, cabecera.Length),
            cancellationToken);

        var firmaPdf = System.Text.Encoding.ASCII.GetString(
            cabecera,
            0,
            bytesLeidos);

        if (!firmaPdf.StartsWith("%PDF-", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "El contenido del archivo no corresponde a un PDF válido.");
        }

        var longitudFinal = (int)Math.Min(1024, archivo.Length);

        archivo.Position = archivo.Length - longitudFinal;

        var final = new byte[longitudFinal];

        await archivo.ReadExactlyAsync(
            final.AsMemory(0, longitudFinal),
            cancellationToken);

        var contenidoFinal = System.Text.Encoding.ASCII.GetString(final);

        if (!contenidoFinal.Contains("%%EOF", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "El archivo PDF parece estar incompleto o dañado.");
        }
    }

    private string ObtenerRutaBaseSegura()
    {
        if (string.IsNullOrWhiteSpace(_configuracion.RutaArchivos))
        {
            throw new InvalidOperationException(
                "No se configuró la ruta de almacenamiento documental.");
        }

        return Path.GetFullPath(_configuracion.RutaArchivos);
    }

    private string ObtenerRutaCompletaSegura(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            throw new ArgumentException(
                "La ruta del archivo es obligatoria.",
                nameof(rutaArchivo));
        }

        var rutaBase = ObtenerRutaBaseSegura();

        var rutaCompleta = Path.GetFullPath(
            Path.Combine(rutaBase, Path.GetFileName(rutaArchivo)));

        if (!rutaCompleta.StartsWith(
            rutaBase + Path.DirectorySeparatorChar,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "La ruta del archivo no es válida.");
        }

        return rutaCompleta;
    }

    private static void EliminarArchivoSiExiste(string rutaArchivo)
    {
        if (File.Exists(rutaArchivo))
        {
            File.Delete(rutaArchivo);
        }
    }
}