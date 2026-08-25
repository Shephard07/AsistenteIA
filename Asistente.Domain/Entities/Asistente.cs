using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asistente.Domain.Entities;

/// <summary>
/// Representa una configuración funcional de asistente de inteligencia artificial.
/// </summary>
public class Asistente
{
    private Asistente()
    {
    }

    public Asistente(
        string nombre,
        string descripcion,
        string modeloIA,
        string idioma,
        string longitudRespuesta,
        string formalidad,
        string formatoRespuesta,
        string restricciones,
        string mensajeBienvenida,
        decimal temperatura,
        int maxTokens,
        int timeoutSeconds)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        ModeloIA = modeloIA;
        Idioma = idioma;
        LongitudRespuesta = longitudRespuesta;
        Formalidad = formalidad;
        FormatoRespuesta = formatoRespuesta;
        Restricciones = restricciones;
        MensajeBienvenida = mensajeBienvenida;
        Temperatura = temperatura;
        MaxTokens = maxTokens;
        TimeoutSeconds = timeoutSeconds;
        Activo = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public int IdAsistente { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public string Descripcion { get; private set; } = string.Empty;

    public string ModeloIA { get; private set; } = string.Empty;

    public string Idioma { get; private set; } = string.Empty;

    public string LongitudRespuesta { get; private set; } = string.Empty;

    public string Formalidad { get; private set; } = string.Empty;

    public string FormatoRespuesta { get; private set; } = string.Empty;

    public string Restricciones { get; private set; } = string.Empty;

    public string MensajeBienvenida { get; private set; } = string.Empty;

    public decimal Temperatura { get; private set; }

    public int MaxTokens { get; private set; }

    public int TimeoutSeconds { get; private set; }

    public bool Activo { get; private set; }

    public DateTime FechaCreacion { get; private set; }

    public ICollection<PromptSistema> Prompts { get; private set; }
        = new List<PromptSistema>();

    public void Actualizar(
        string nombre,
        string descripcion,
        string modeloIA,
        string idioma,
        string longitudRespuesta,
        string formalidad,
        string formatoRespuesta,
        string restricciones,
        string mensajeBienvenida,
        decimal temperatura,
        int maxTokens,
        int timeoutSeconds)
    {
        Nombre = nombre;
        Descripcion = descripcion;
        ModeloIA = modeloIA;
        Idioma = idioma;
        LongitudRespuesta = longitudRespuesta;
        Formalidad = formalidad;
        FormatoRespuesta = formatoRespuesta;
        Restricciones = restricciones;
        MensajeBienvenida = mensajeBienvenida;
        Temperatura = temperatura;
        MaxTokens = maxTokens;
        TimeoutSeconds = timeoutSeconds;
    }

    public void CambiarEstado(bool activo)
    {
        Activo = activo;
    }
}   