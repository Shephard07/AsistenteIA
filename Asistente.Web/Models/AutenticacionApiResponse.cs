namespace Asistente.Web.Models;

public class AutenticacionApiResponse
{
    public int IdUsuario { get; set; }

    public int IdSesion { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
}

public class ResultadoLoginApi
{
    public bool Exito { get; init; }

    public AutenticacionApiResponse? Respuesta { get; init; }

    public string MensajeError { get; init; } = string.Empty;
}

public class ErrorApiResponse
{
    public string Mensaje { get; set; } = string.Empty;
}