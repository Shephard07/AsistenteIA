using System.Text;
using System.Text.RegularExpressions;
using Asistente.Application.Interfaces;

namespace Asistente.Application.Services;

public class NormalizadorContenidoDocumentoService
    : INormalizadorContenidoDocumento
{
    public string Normalizar(string contenido)
    {
        if (string.IsNullOrWhiteSpace(contenido))
        {
            return string.Empty;
        }

        var textoSinCaracteresControl = new StringBuilder();

        foreach (var caracter in contenido)
        {
            if (!char.IsControl(caracter) ||
                caracter is '\n' or '\r' or '\t')
            {
                textoSinCaracteresControl.Append(caracter);
            }
        }

        var textoConSaltosNormalizados = textoSinCaracteresControl
            .ToString()
            .Replace("\r\n", "\n")
            .Replace('\r', '\n');

        var lineasNormalizadas = textoConSaltosNormalizados
            .Split('\n')
            .Select(linea => Regex.Replace(
                linea.Trim(),
                @"[ \t]+",
                " "));

        var resultado = string.Join(
            "\n",
            lineasNormalizadas);

        resultado = Regex.Replace(
            resultado,
            @"\n{3,}",
            "\n\n");

        return resultado.Trim();
    }
}