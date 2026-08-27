//Conversacion.cs
using Asistente.Domain.Enums;

namespace Asistente.Domain.Entities;

/// <summary>
/// Representa una conversación registrada entre un usuario y el asistente.
/// </summary>
public class Conversacion
{
    public int IdConversacion { get; private set; }

    public int? IdAsistente { get; private set; }

    public Asistente? Asistente { get; private set; }

    public int? IdUsuario { get; private set; }

    public Usuario? Usuario { get; private set; }

    public string? Titulo { get; private set; }

    public DateTime FechaInicio { get; private set; }

    public DateTime? FechaFin { get; private set; }

    public DateTime FechaUltimaActividad { get; private set; }

    public string? ResumenContexto { get; private set; }

    public int TotalMensajes { get; private set; }

    public int TotalMensajesResumidos { get; private set; }

    public EstadoConversacion Estado { get; private set; }

    public ICollection<Mensaje> Mensajes { get; private set; } =
        new List<Mensaje>();

    public Conversacion()
    {
        FechaInicio = DateTime.UtcNow;
        FechaUltimaActividad = FechaInicio;
        Estado = EstadoConversacion.Activa;
    }

    // Se conserva para no romper las conversaciones de la Etapa 4.
    public Conversacion(int idAsistente)
        : this()
    {
        ValidarIdAsistente(idAsistente);
        IdAsistente = idAsistente;
    }

    // Constructor de las nuevas conversaciones con propietario.
    public Conversacion(int idAsistente, int idUsuario)
        : this(idAsistente)
    {
        if (idUsuario <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idUsuario),
                "El identificador del usuario debe ser mayor que cero.");
        }

        IdUsuario = idUsuario;
    }

    public void AgregarMensaje(Mensaje mensaje)
    {
        if (Estado != EstadoConversacion.Activa)
        {
            throw new InvalidOperationException(
                "No se pueden agregar mensajes a una conversación no activa.");
        }

        ArgumentNullException.ThrowIfNull(mensaje);

        Mensajes.Add(mensaje);
        TotalMensajes++;
        FechaUltimaActividad = DateTime.UtcNow;
    }

    public void Renombrar(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new ArgumentException(
                "El título de la conversación es obligatorio.",
                nameof(titulo));
        }

        Titulo = titulo.Trim();
        FechaUltimaActividad = DateTime.UtcNow;
    }

    public void ActualizarResumenContexto(string? resumenContexto)
    {
        ActualizarResumenContexto(
            resumenContexto,
            TotalMensajesResumidos);
    }

    public void ActualizarResumenContexto(
        string? resumenContexto,
        int totalMensajesResumidos)
    {
        if (totalMensajesResumidos < 0 ||
            totalMensajesResumidos > TotalMensajes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalMensajesResumidos),
                "La cantidad de mensajes resumidos no es válida.");
        }

        ResumenContexto = string.IsNullOrWhiteSpace(resumenContexto)
            ? null
            : resumenContexto.Trim();

        TotalMensajesResumidos = string.IsNullOrWhiteSpace(ResumenContexto)
            ? 0
            : totalMensajesResumidos;
    }

    public void Archivar()
    {
        if (Estado == EstadoConversacion.Eliminada)
        {
            throw new InvalidOperationException(
                "No se puede archivar una conversación eliminada.");
        }

        Estado = EstadoConversacion.Archivada;
        FechaUltimaActividad = DateTime.UtcNow;
    }

    public void Reactivar()
    {
        if (Estado == EstadoConversacion.Eliminada)
        {
            throw new InvalidOperationException(
                "No se puede reactivar una conversación eliminada.");
        }

        Estado = EstadoConversacion.Activa;
        FechaUltimaActividad = DateTime.UtcNow;
    }

    public void Eliminar()
    {
        Estado = EstadoConversacion.Eliminada;
        FechaUltimaActividad = DateTime.UtcNow;
    }

    public void Finalizar()
    {
        if (Estado == EstadoConversacion.Finalizada)
        {
            return;
        }

        Estado = EstadoConversacion.Finalizada;
        FechaFin = DateTime.UtcNow;
        FechaUltimaActividad = FechaFin.Value;
    }

    private static void ValidarIdAsistente(int idAsistente)
    {
        if (idAsistente <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(idAsistente),
                "El identificador del asistente debe ser mayor que cero.");
        }
    }
}