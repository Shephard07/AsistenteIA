using Asistente.Domain.Enums;

namespace Asistente.Domain.Entities;

/// <summary>
/// Representa una conversación registrada entre el usuario y el asistente.
/// </summary>
public class Conversacion
{
    public int IdConversacion { get; private set; }

    public int? IdAsistente { get; private set; }

    public Asistente? Asistente { get; private set; }

    public DateTime FechaInicio { get; private set; }

    public DateTime? FechaFin { get; private set; }

    public EstadoConversacion Estado { get; private set; }

    public ICollection<Mensaje> Mensajes { get; private set; } =
        new List<Mensaje>();

    public Conversacion()
    {
        FechaInicio = DateTime.UtcNow;
        Estado = EstadoConversacion.Activa;
    }

    public Conversacion(int idAsistente)
        : this()
    {
        if (idAsistente <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idAsistente),
                "El identificador del asistente debe ser mayor que cero.");
        }

        IdAsistente = idAsistente;
    }

    public void AgregarMensaje(Mensaje mensaje)
    {
        if (Estado != EstadoConversacion.Activa)
        {
            throw new InvalidOperationException(
                "No se pueden agregar mensajes a una conversación finalizada.");
        }

        ArgumentNullException.ThrowIfNull(mensaje);

        Mensajes.Add(mensaje);
    }

    public void Finalizar()
    {
        if (Estado == EstadoConversacion.Finalizada)
        {
            return;
        }

        Estado = EstadoConversacion.Finalizada;
        FechaFin = DateTime.UtcNow;
    }
}