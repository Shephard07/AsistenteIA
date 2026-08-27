//ConfiguracionMemoria.cs
namespace Asistente.Domain.Entities;

/// <summary>
/// Define los límites activos para la memoria y el contexto conversacional.
/// </summary>
public class ConfiguracionMemoria
{
    public int IdConfiguracion { get; private set; }

    public int MaximoMensajesContexto { get; private set; }

    public int MaximoTokensContexto { get; private set; }

    public int LongitudResumen { get; private set; }

    public int CantidadConversacionesVisibles { get; private set; }

    public bool Activo { get; private set; }

    private ConfiguracionMemoria()
    {
    }

    public ConfiguracionMemoria(
        int maximoMensajesContexto,
        int maximoTokensContexto,
        int longitudResumen,
        int cantidadConversacionesVisibles)
    {
        Actualizar(
            maximoMensajesContexto,
            maximoTokensContexto,
            longitudResumen,
            cantidadConversacionesVisibles);

        Activo = true;
    }

    public void Actualizar(
        int maximoMensajesContexto,
        int maximoTokensContexto,
        int longitudResumen,
        int cantidadConversacionesVisibles)
    {
        if (maximoMensajesContexto <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximoMensajesContexto));
        }

        if (maximoTokensContexto <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximoTokensContexto));
        }

        if (longitudResumen <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(longitudResumen));
        }

        if (cantidadConversacionesVisibles <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cantidadConversacionesVisibles));
        }

        MaximoMensajesContexto = maximoMensajesContexto;
        MaximoTokensContexto = maximoTokensContexto;
        LongitudResumen = longitudResumen;
        CantidadConversacionesVisibles = cantidadConversacionesVisibles;
    }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }
}